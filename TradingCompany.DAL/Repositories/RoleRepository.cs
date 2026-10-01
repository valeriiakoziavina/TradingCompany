using TradingCompany.Core.Entities;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.DAL.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(TradingDbContext context) : base(context) { }
}