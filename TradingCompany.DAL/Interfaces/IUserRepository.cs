using TradingCompany.Core.Entities;

namespace TradingCompany.DAL.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByLoginAsync(string login);
}
