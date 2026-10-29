namespace TradingCompany.Core.DTOs;

public class CreateWaybillDto
{
    public int OrderId { get; set; }
    public string WaybillNumber { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
