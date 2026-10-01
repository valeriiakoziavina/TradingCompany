using TradingCompany.Core.Entities;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.DAL.Repositories;

public class ShipmentRepository : Repository<Shipment>, IShipmentRepository
{
    public ShipmentRepository(TradingDbContext context) : base(context) { }
}