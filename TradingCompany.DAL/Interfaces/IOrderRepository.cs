using TradingCompany.Core.Entities;

namespace TradingCompany.DAL.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetAllWithDeliveryAsync();
}
