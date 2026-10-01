using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingCompany.Core.Entities;

public class Waybill
{
    public int Id { get; set; }
    public string WaybillNumber { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Order? Order { get; set; }
    public User? Customer { get; set; }
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}