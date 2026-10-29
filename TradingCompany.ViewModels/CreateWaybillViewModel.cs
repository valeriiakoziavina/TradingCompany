using TradingCompany.BL;
using TradingCompany.Core.DTOs;

namespace TradingCompany.ViewModels;

public class CreateWaybillViewModel : ValidatableViewModelBase
{
    private readonly IDeliveryService _deliveryService;
    private string _waybillNumber;
    private string? _paymentType;
    private string? _deliveryMethod;
    private string _city = string.Empty;
    private string _deliveryAddress = string.Empty;
    private string _notes = string.Empty;
    private string? _errorMessage;

    public CreateWaybillViewModel(IDeliveryService deliveryService, DeliveryOrderDto order)
    {
        _deliveryService = deliveryService;
        OrderId = order.OrderId;
        OrderNumber = order.OrderNumber;
        CustomerName = order.CustomerName;
        TotalAmount = order.TotalAmount;
        _waybillNumber = $"WB-{order.OrderNumber}";

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !HasErrors);
        CancelCommand = new RelayCommand(() => RequestClose?.Invoke(this, false));

        ErrorsChanged += (_, _) => SaveCommand.RaiseCanExecuteChanged();
    }

    public int OrderId { get; }
    public string OrderNumber { get; }
    public string CustomerName { get; }
    public decimal TotalAmount { get; }

    public IReadOnlyList<string> PaymentTypes => DeliveryService.PaymentTypes;
    public IReadOnlyList<string> DeliveryMethods => DeliveryService.DeliveryMethods;

    public string WaybillNumber
    {
        get => _waybillNumber;
        set { if (SetProperty(ref _waybillNumber, value)) ValidateWaybillNumber(); }
    }

    public string? PaymentType
    {
        get => _paymentType;
        set { if (SetProperty(ref _paymentType, value)) ValidatePaymentType(); }
    }

    public string? DeliveryMethod
    {
        get => _deliveryMethod;
        set { if (SetProperty(ref _deliveryMethod, value)) ValidateDeliveryMethod(); }
    }

    public string City
    {
        get => _city;
        set { if (SetProperty(ref _city, value)) ValidateCity(); }
    }

    public string DeliveryAddress
    {
        get => _deliveryAddress;
        set { if (SetProperty(ref _deliveryAddress, value)) ValidateDeliveryAddress(); }
    }

    public string Notes
    {
        get => _notes;
        set { if (SetProperty(ref _notes, value)) ValidateNotes(); }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event EventHandler<bool>? RequestClose;

    private async Task SaveAsync()
    {
        if (!ValidateAll())
            return;

        try
        {
            await _deliveryService.CreateWaybillAsync(new CreateWaybillDto
            {
                OrderId = OrderId,
                WaybillNumber = WaybillNumber,
                PaymentType = PaymentType ?? string.Empty,
                DeliveryMethod = DeliveryMethod ?? string.Empty,
                City = City,
                DeliveryAddress = DeliveryAddress,
                Notes = Notes
            });

            ErrorMessage = null;
            RequestClose?.Invoke(this, true);
        }
        catch (BusinessRuleException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    protected override void ValidateAllProperties()
    {
        ValidateWaybillNumber();
        ValidatePaymentType();
        ValidateDeliveryMethod();
        ValidateCity();
        ValidateDeliveryAddress();
        ValidateNotes();
    }

    private void ValidateWaybillNumber() =>
        SetErrors(nameof(WaybillNumber), Required(WaybillNumber, "Номер накладної", DeliveryService.WaybillNumberMaxLength));

    private void ValidatePaymentType() =>
        SetErrors(nameof(PaymentType), PaymentType is null ? ["Виберіть спосіб оплати."] : []);

    private void ValidateDeliveryMethod() =>
        SetErrors(nameof(DeliveryMethod), DeliveryMethod is null ? ["Виберіть спосіб доставки."] : []);

    private void ValidateCity() =>
        SetErrors(nameof(City), Required(City, "Місто", DeliveryService.CityMaxLength));

    private void ValidateDeliveryAddress() =>
        SetErrors(nameof(DeliveryAddress), Required(DeliveryAddress, "Адреса", DeliveryService.AddressMaxLength));

    private void ValidateNotes() =>
        SetErrors(nameof(Notes), Notes.Trim().Length > DeliveryService.NotesMaxLength
            ? [$"Не довше за {DeliveryService.NotesMaxLength} символів."]
            : []);

    private static List<string> Required(string value, string field, int maxLength)
    {
        var text = value.Trim();
        if (text.Length == 0)
            return [$"{field} — обов'язкове поле."];
        if (text.Length > maxLength)
            return [$"Не довше за {maxLength} символів."];
        return [];
    }
}
