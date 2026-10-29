using TradingCompany.Core.DTOs;
using TradingCompany.Core.Entities;

namespace TradingCompany.ConsoleApp.Mapping;

public static class OrderMapper
{
    public static OrderDto ToDto(this Order entity) => new()
    {
        Id = entity.Id,
        OrderNumber = entity.OrderNumber,
        CustomerId = entity.CustomerId,
        OrderDate = entity.OrderDate,
        TotalAmount = entity.TotalAmount
    };

    public static Order ToEntity(this CreateOrderDto dto) => new()
    {
        OrderNumber = dto.OrderNumber.Trim(),
        CustomerId = dto.CustomerId,
        TotalAmount = dto.TotalAmount,
        OrderDate = DateTime.Now
    };
}
