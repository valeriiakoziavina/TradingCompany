using TradingCompany.Core.DTOs;

namespace TradingCompany.BL;

public interface IUserSession
{
    UserDto? CurrentUser { get; }
    bool IsAuthenticated { get; }
    bool IsDeliveryManager { get; }
    void SignIn(UserDto user);
    void SignOut();
}
