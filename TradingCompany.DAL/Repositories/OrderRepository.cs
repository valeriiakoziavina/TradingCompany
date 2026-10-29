using Microsoft.EntityFrameworkCore;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.DAL.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(TradingDbContext context) : base(context) { }

    public async Task<IEnumerable<Order>> GetAllWithDeliveryAsync() =>
        await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Waybill)
                .ThenInclude(w => w!.Shipments)
                    .ThenInclude(s => s.Status)
            .ToListAsync();
}
