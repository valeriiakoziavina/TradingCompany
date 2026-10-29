using TradingCompany.Core.DTOs;

namespace TradingCompany.BL;

public interface IAuthService
{
    Task<bool> LoginAsync(string login, string password);
    void Logout();
    Task<List<UserDto>> GetAllAsync();
    Task<int> CreateUserAsync(CreateUserDto user);
    Task<bool> ChangePasswordAsync(int userId, string newPassword);
    Task<bool> DeleteAsync(int userId);
}
