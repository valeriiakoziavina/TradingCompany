using Microsoft.EntityFrameworkCore;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.DAL.Repositories;

public class WaybillRepository : Repository<Waybill>, IWaybillRepository
{
    public WaybillRepository(TradingDbContext context) : base(context) { }

    public override async Task<IEnumerable<Waybill>> GetAllAsync() =>
        await _context.Waybills.Include(w => w.Customer).Include(w => w.Order).ToListAsync();

    public override async Task<Waybill?> GetByIdAsync(int id) =>
        await _context.Waybills.Include(w => w.Customer).FirstOrDefaultAsync(w => w.Id == id);

    public async Task<IEnumerable<Waybill>> GetFilteredAsync(DateTime? startDate, DateTime? endDate, string? city, string? paymentType)
    {
        var query = _context.Waybills.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(w => w.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(w => w.CreatedAt <= endDate.Value);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(w => w.City.Contains(city));

        if (!string.IsNullOrWhiteSpace(paymentType))
            query = query.Where(w => w.PaymentType == paymentType);

        return await query.ToListAsync();
    }
}