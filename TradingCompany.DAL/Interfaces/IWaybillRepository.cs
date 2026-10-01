using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingCompany.Core.Entities;

namespace TradingCompany.DAL.Interfaces;

public interface IWaybillRepository : IRepository<Waybill>
{
    Task<IEnumerable<Waybill>> GetFilteredAsync(DateTime? startDate, DateTime? endDate, string? city, string? paymentType);
}
