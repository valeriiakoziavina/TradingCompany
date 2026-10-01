using TradingCompany.Core.Entities;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.DAL.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(TradingDbContext context) : base(context) { }
}