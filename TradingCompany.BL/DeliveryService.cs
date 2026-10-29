using TradingCompany.Core.DTOs;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.BL;

public class DeliveryService(
    IOrderRepository orderRepository,
    IWaybillRepository waybillRepository,
    IShipmentRepository shipmentRepository,
    IShipmentStatusRepository statusRepository,
    IUserSession userSession,
    TimeProvider timeProvider) : IDeliveryService
{
    public const int AwaitingWaybillStatusId = 0;
    public const string AwaitingWaybillStatusName = "Очікує накладної";

    public const int WaybillNumberMaxLength = 50;
    public const int CityMaxLength = 100;
    public const int AddressMaxLength = 200;
    public const int NotesMaxLength = 500;

    public static readonly IReadOnlyList<string> PaymentTypes = ["Готівка", "Банківська картка", "Безготівковий розрахунок", "Накладений платіж"];
    public static readonly IReadOnlyList<string> DeliveryMethods = ["Кур'єр", "Нова пошта", "Укрпошта", "Самовивіз"];

    public async Task<List<ShipmentStatusDto>> GetStatusesAsync()
    {
        EnsureDeliveryManager();

        var statuses = (await statusRepository.GetAllAsync())
            .OrderBy(s => s.Id)
            .Select(s => new ShipmentStatusDto { Id = s.Id, Name = s.Name });

        return [new ShipmentStatusDto { Id = AwaitingWaybillStatusId, Name = AwaitingWaybillStatusName }, .. statuses];
    }

    public async Task<List<DeliveryOrderDto>> GetOrdersAsync()
    {
        EnsureDeliveryManager();

        return (await orderRepository.GetAllWithDeliveryAsync())
            .Select(ToDeliveryDto)
            .ToList();
    }

    public List<DeliveryOrderDto> ApplyFilter(IEnumerable<DeliveryOrderDto> orders, DeliveryOrderFilterDto filter)
    {
        if (filter.DateFrom is { } from && filter.DateTo is { } to && from.Date > to.Date)
            throw new BusinessRuleException("Дата «з» не може бути пізнішою за дату «по».");

        var query = orders;

        if (filter.DateFrom is { } dateFrom)
            query = query.Where(o => o.OrderDate.Date >= dateFrom.Date);

        if (filter.DateTo is { } dateTo)
            query = query.Where(o => o.OrderDate.Date <= dateTo.Date);

        if (filter.StatusId is { } statusId)
            query = query.Where(o => o.StatusId == statusId);

        var text = filter.SearchText?.Trim();
        if (!string.IsNullOrEmpty(text))
        {
            query = query.Where(o =>
                Contains(o.OrderNumber, text) ||
                Contains(o.CustomerName, text) ||
                Contains(o.City, text) ||
                Contains(o.WaybillNumber, text));
        }

        Func<DeliveryOrderDto, object> key = filter.SortBy switch
        {
            OrderSortField.OrderNumber => o => o.OrderNumber,
            OrderSortField.CustomerName => o => o.CustomerName,
            OrderSortField.TotalAmount => o => o.TotalAmount,
            OrderSortField.Status => o => o.StatusId,
            _ => o => o.OrderDate
        };

        var sorted = filter.Descending
            ? query.OrderByDescending(key, Comparer<object>.Create(CompareKeys))
            : query.OrderBy(key, Comparer<object>.Create(CompareKeys));

        return sorted.ThenBy(o => o.OrderNumber, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    public async Task<int> CreateWaybillAsync(CreateWaybillDto waybill)
    {
        var manager = EnsureDeliveryManager();

        var number = waybill.WaybillNumber.Trim();
        var city = waybill.City.Trim();
        var address = waybill.DeliveryAddress.Trim();
        var notes = string.IsNullOrWhiteSpace(waybill.Notes) ? null : waybill.Notes.Trim();
        var errors = new List<string>();

        if (number.Length == 0)
            errors.Add("Номер накладної обов'язковий.");
        else if (number.Length > WaybillNumberMaxLength)
            errors.Add($"Номер накладної не може бути довшим за {WaybillNumberMaxLength} символів.");

        if (!PaymentTypes.Contains(waybill.PaymentType))
            errors.Add("Виберіть спосіб оплати зі списку.");

        if (!DeliveryMethods.Contains(waybill.DeliveryMethod))
            errors.Add("Виберіть спосіб доставки зі списку.");

        if (city.Length == 0)
            errors.Add("Місто обов'язкове.");
        else if (city.Length > CityMaxLength)
            errors.Add($"Місто не може бути довшим за {CityMaxLength} символів.");

        if (address.Length == 0)
            errors.Add("Адреса доставки обов'язкова.");
        else if (address.Length > AddressMaxLength)
            errors.Add($"Адреса не може бути довшою за {AddressMaxLength} символів.");

        if (notes is { Length: > NotesMaxLength })
            errors.Add($"Примітка не може бути довшою за {NotesMaxLength} символів.");

        var orders = await orderRepository.GetAllWithDeliveryAsync();
        var order = orders.FirstOrDefault(o => o.Id == waybill.OrderId);
        if (order is null)
            errors.Add("Замовлення не знайдено.");
        else if (order.Waybill is not null)
            errors.Add($"Для замовлення {order.OrderNumber} накладна вже створена.");

        if (errors.Count == 0 && await waybillRepository.ExistsByNumberAsync(number))
            errors.Add($"Накладна з номером \"{number}\" вже існує.");

        if (errors.Count > 0)
            throw new BusinessRuleException(errors);

        var entity = new Waybill
        {
            WaybillNumber = number,
            OrderId = order!.Id,
            CustomerId = order.CustomerId,
            PaymentType = waybill.PaymentType,
            DeliveryMethod = waybill.DeliveryMethod,
            City = city,
            DeliveryAddress = address,
            CreatedAt = timeProvider.GetLocalNow().DateTime
        };
        await waybillRepository.AddAsync(entity);

        await shipmentRepository.AddAsync(new Shipment
        {
            WaybillId = entity.Id,
            ManagerId = manager.Id,
            StatusId = ShipmentStatus.PendingId,
            Notes = notes
        });

        return entity.Id;
    }

    public async Task ConfirmDispatchAsync(int shipmentId)
    {
        var manager = EnsureDeliveryManager();

        var shipment = await shipmentRepository.GetByIdAsync(shipmentId)
            ?? throw new BusinessRuleException("Відвантаження не знайдено.");

        if (shipment.StatusId != ShipmentStatus.PendingId)
            throw new BusinessRuleException("Відправлення вже підтверджено.");

        shipment.StatusId = ShipmentStatus.ShippedId;
        shipment.DispatchedAt = timeProvider.GetLocalNow().DateTime;
        shipment.ManagerId = manager.Id;
        await shipmentRepository.UpdateAsync(shipment);
    }

    private UserDto EnsureDeliveryManager()
    {
        if (userSession.CurrentUser is not { IsDeliveryManager: true } user)
            throw new BusinessRuleException("Доступ дозволено лише менеджеру доставки.");

        return user;
    }

    private static DeliveryOrderDto ToDeliveryDto(Order order)
    {
        var waybill = order.Waybill;
        var shipment = waybill?.Shipments.OrderByDescending(s => s.Id).FirstOrDefault();

        return new DeliveryOrderDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            CustomerName = order.Customer?.FullName ?? string.Empty,
            CustomerPhone = order.Customer?.PhoneNumber ?? string.Empty,
            WaybillId = waybill?.Id,
            WaybillNumber = waybill?.WaybillNumber,
            City = waybill?.City,
            DeliveryAddress = waybill?.DeliveryAddress,
            DeliveryMethod = waybill?.DeliveryMethod,
            PaymentType = waybill?.PaymentType,
            ShipmentId = shipment?.Id,
            StatusId = shipment?.StatusId ?? AwaitingWaybillStatusId,
            StatusName = shipment?.Status?.Name ?? AwaitingWaybillStatusName,
            DispatchedAt = shipment?.DispatchedAt
        };
    }

    private static bool Contains(string? value, string text) =>
        value is not null && value.Contains(text, StringComparison.CurrentCultureIgnoreCase);

    private static int CompareKeys(object? x, object? y) => (x, y) switch
    {
        (string a, string b) => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase),
        (IComparable a, _) => a.CompareTo(y),
        _ => 0
    };
}
