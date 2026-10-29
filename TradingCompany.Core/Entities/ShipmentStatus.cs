namespace TradingCompany.Core.Entities;

public class ShipmentStatus
{
    public const int PendingId = 1;
    public const int ShippedId = 2;

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
