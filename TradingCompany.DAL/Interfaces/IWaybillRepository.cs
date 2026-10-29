using TradingCompany.Core.Entities;

namespace TradingCompany.DAL.Interfaces;

public interface IWaybillRepository : IRepository<Waybill>
{
    Task<IEnumerable<Waybill>> GetFilteredAsync(DateTime? startDate, DateTime? endDate, string? city, string? paymentType);
    Task<bool> ExistsByNumberAsync(string waybillNumber);
}
