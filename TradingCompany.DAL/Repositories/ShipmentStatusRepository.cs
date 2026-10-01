using TradingCompany.Core.Entities;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.DAL.Repositories;

public class ShipmentStatusRepository : Repository<ShipmentStatus>, IShipmentStatusRepository
{
    public ShipmentStatusRepository(TradingDbContext context) : base(context) { }
}