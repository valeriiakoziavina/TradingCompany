namespace TradingCompany.Core.DTOs;

public class CreateOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
}
