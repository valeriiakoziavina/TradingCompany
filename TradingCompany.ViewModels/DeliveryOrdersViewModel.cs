using System.Collections.ObjectModel;
using TradingCompany.BL;
using TradingCompany.Core.DTOs;
using TradingCompany.Core.Entities;

namespace TradingCompany.ViewModels;

public class DeliveryOrdersViewModel : ValidatableViewModelBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private List<DeliveryOrderDto> _allOrders = [];
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    private StatusFilterItem? _selectedStatus;
    private string _searchText = string.Empty;
    private SortOption _selectedSort;
    private bool _sortDescending = true;
    private DeliveryOrderDto? _selectedOrder;
    private string? _errorMessage;
    private string? _statusMessage;

    public DeliveryOrdersViewModel(
        IDeliveryService deliveryService,
        IAuthService authService,
        IUserSession userSession,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _deliveryService = deliveryService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        UserName = userSession.CurrentUser?.FullName ?? string.Empty;

        SortOptions =
        [
            new SortOption(OrderSortField.OrderDate, "Дата замовлення"),
            new SortOption(OrderSortField.OrderNumber, "Номер замовлення"),
            new SortOption(OrderSortField.CustomerName, "Клієнт"),
            new SortOption(OrderSortField.TotalAmount, "Сума"),
            new SortOption(OrderSortField.Status, "Статус")
        ];
        _selectedSort = SortOptions[0];

        LoadCommand = new AsyncRelayCommand(() => LoadAsync(SelectedOrder?.OrderId));
        ResetFiltersCommand = new RelayCommand(ResetFilters);
        CreateWaybillCommand = new AsyncRelayCommand(CreateWaybillAsync, () => SelectedOrder is { WaybillId: null });
        ConfirmDispatchCommand = new AsyncRelayCommand(ConfirmDispatchAsync,
            () => SelectedOrder is { ShipmentId: not null, StatusId: ShipmentStatus.PendingId });
        GoHomeCommand = new RelayCommand(navigationService.ShowMain);
        LogoutCommand = new RelayCommand(() =>
        {
            authService.Logout();
            navigationService.ShowLogin();
        });
    }

    public string UserName { get; }

    public ObservableCollection<DeliveryOrderDto> Orders { get; } = new();
    public ObservableCollection<StatusFilterItem> Statuses { get; } = new();
    public IReadOnlyList<SortOption> SortOptions { get; }

    public DateTime? DateFrom
    {
        get => _dateFrom;
        set { if (SetProperty(ref _dateFrom, value)) OnFilterChanged(); }
    }

    public DateTime? DateTo
    {
        get => _dateTo;
        set { if (SetProperty(ref _dateTo, value)) OnFilterChanged(); }
    }

    public StatusFilterItem? SelectedStatus
    {
        get => _selectedStatus;
        set { if (SetProperty(ref _selectedStatus, value)) ApplyFilter(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }

    public SortOption SelectedSort
    {
        get => _selectedSort;
        set { if (SetProperty(ref _selectedSort, value)) ApplyFilter(); }
    }

    public bool SortDescending
    {
        get => _sortDescending;
        set { if (SetProperty(ref _sortDescending, value)) ApplyFilter(); }
    }

    public DeliveryOrderDto? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (SetProperty(ref _selectedOrder, value))
            {
                OnPropertyChanged(nameof(HasSelection));
                CreateWaybillCommand.RaiseCanExecuteChanged();
                ConfirmDispatchCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasSelection => SelectedOrder is not null;

    public string Summary => $"Показано: {Orders.Count} з {_allOrders.Count}";

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public AsyncRelayCommand LoadCommand { get; }
    public RelayCommand ResetFiltersCommand { get; }
    public AsyncRelayCommand CreateWaybillCommand { get; }
    public AsyncRelayCommand ConfirmDispatchCommand { get; }
    public RelayCommand GoHomeCommand { get; }
    public RelayCommand LogoutCommand { get; }

    public async Task LoadAsync(int? selectOrderId = null)
    {
        try
        {
            if (Statuses.Count == 0)
            {
                Statuses.Add(new StatusFilterItem(null, "Усі статуси"));
                foreach (var status in await _deliveryService.GetStatusesAsync())
                    Statuses.Add(new StatusFilterItem(status.Id, status.Name));

                _selectedStatus = Statuses[0];
                OnPropertyChanged(nameof(SelectedStatus));
            }

            _allOrders = await _deliveryService.GetOrdersAsync();
            ErrorMessage = null;
        }
        catch (BusinessRuleException ex)
        {
            _allOrders = [];
            ErrorMessage = ex.Message;
        }

        ApplyFilter(selectOrderId);
    }

    private void OnFilterChanged()
    {
        ValidateDates();
        ApplyFilter();
    }

    private void ApplyFilter(int? selectOrderId = null)
    {
        if (HasErrors)
            return;

        var keepId = selectOrderId ?? SelectedOrder?.OrderId;
        try
        {
            var filtered = _deliveryService.ApplyFilter(_allOrders, new DeliveryOrderFilterDto
            {
                DateFrom = DateFrom,
                DateTo = DateTo,
                StatusId = SelectedStatus?.Id,
                SearchText = SearchText,
                SortBy = SelectedSort.Field,
                Descending = SortDescending
            });

            Orders.Clear();
            foreach (var order in filtered)
                Orders.Add(order);

            SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == keepId);
        }
        catch (BusinessRuleException ex)
        {
            ErrorMessage = ex.Message;
        }

        OnPropertyChanged(nameof(Summary));
    }

    private void ResetFilters()
    {
        _dateFrom = null;
        _dateTo = null;
        _searchText = string.Empty;
        _selectedStatus = Statuses.FirstOrDefault();
        _selectedSort = SortOptions[0];
        _sortDescending = true;
        OnPropertyChanged(nameof(DateFrom));
        OnPropertyChanged(nameof(DateTo));
        OnPropertyChanged(nameof(SearchText));
        OnPropertyChanged(nameof(SelectedStatus));
        OnPropertyChanged(nameof(SelectedSort));
        OnPropertyChanged(nameof(SortDescending));
        ValidateDates();
        ApplyFilter();
    }

    private async Task CreateWaybillAsync()
    {
        var order = SelectedOrder!;
        var editor = new CreateWaybillViewModel(_deliveryService, order);
        if (!_navigationService.ShowCreateWaybill(editor))
            return;

        await LoadAsync(order.OrderId);
        StatusMessage = $"Накладну {editor.WaybillNumber.Trim()} для замовлення {order.OrderNumber} створено.";
    }

    private async Task ConfirmDispatchAsync()
    {
        var order = SelectedOrder!;
        if (!_dialogService.Confirm($"Підтвердити відправлення замовлення {order.OrderNumber} (накладна {order.WaybillNumber})?", "Підтвердження"))
            return;

        try
        {
            await _deliveryService.ConfirmDispatchAsync(order.ShipmentId!.Value);
            await LoadAsync(order.OrderId);
            StatusMessage = $"Відправлення замовлення {order.OrderNumber} підтверджено.";
        }
        catch (BusinessRuleException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    protected override void ValidateAllProperties() => ValidateDates();

    private void ValidateDates()
    {
        var errors = DateFrom is { } from && DateTo is { } to && from.Date > to.Date
            ? new[] { "Дата «по» не може бути раніше за дату «з»." }
            : [];
        SetErrors(nameof(DateTo), errors);
    }
}
