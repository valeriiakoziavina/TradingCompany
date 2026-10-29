namespace TradingCompany.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = [];
    public Guid PasswordSalt { get; set; }
    public UserRole Role { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Waybill> Waybills { get; set; } = new List<Waybill>();
    public ICollection<Shipment> ManagedShipments { get; set; } = new List<Shipment>();
}
