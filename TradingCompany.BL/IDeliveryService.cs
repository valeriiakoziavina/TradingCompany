using TradingCompany.Core.DTOs;

namespace TradingCompany.BL;

public interface IDeliveryService
{
    Task<List<ShipmentStatusDto>> GetStatusesAsync();

    Task<List<DeliveryOrderDto>> GetOrdersAsync();

    List<DeliveryOrderDto> ApplyFilter(IEnumerable<DeliveryOrderDto> orders, DeliveryOrderFilterDto filter);

    Task<int> CreateWaybillAsync(CreateWaybillDto waybill);

    Task ConfirmDispatchAsync(int shipmentId);
}
