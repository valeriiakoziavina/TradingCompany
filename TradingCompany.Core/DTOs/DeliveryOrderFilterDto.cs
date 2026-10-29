namespace TradingCompany.Core.DTOs;

public class DeliveryOrderFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? StatusId { get; set; }
    public string? SearchText { get; set; }
    public OrderSortField SortBy { get; set; } = OrderSortField.OrderDate;
    public bool Descending { get; set; } = true;
}
