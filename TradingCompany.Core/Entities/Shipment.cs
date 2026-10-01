using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingCompany.Core.Entities;

public class Shipment
{
    public int Id { get; set; }
    public int WaybillId { get; set; }
    public int ManagerId { get; set; }
    public int StatusId { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public string? Notes { get; set; }

    public Waybill? Waybill { get; set; }
    public User? Manager { get; set; }
    public ShipmentStatus? Status { get; set; }
}