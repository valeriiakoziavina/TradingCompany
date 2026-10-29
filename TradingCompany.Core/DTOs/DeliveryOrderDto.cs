namespace TradingCompany.Core.DTOs;

public class DeliveryOrderDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public int? WaybillId { get; set; }
    public string? WaybillNumber { get; set; }
    public string? City { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? DeliveryMethod { get; set; }
    public string? PaymentType { get; set; }

    public int? ShipmentId { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? DispatchedAt { get; set; }
}
