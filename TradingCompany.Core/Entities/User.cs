using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingCompany.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }

    public Role? Role { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Waybill> Waybills { get; set; } = new List<Waybill>();
    public ICollection<Shipment> ManagedShipments { get; set; } = new List<Shipment>();
}