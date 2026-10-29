using TradingCompany.Core.DTOs;

namespace TradingCompany.BL;

public class UserSession : IUserSession
{
    public UserDto? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;
    public bool IsDeliveryManager => CurrentUser?.IsDeliveryManager == true;

    public void SignIn(UserDto user) => CurrentUser = user;

    public void SignOut() => CurrentUser = null;
}
