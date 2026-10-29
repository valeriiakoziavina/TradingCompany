using Microsoft.EntityFrameworkCore;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.DAL.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(TradingDbContext context) : base(context) { }

    public async Task<User?> GetByLoginAsync(string login) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
}
