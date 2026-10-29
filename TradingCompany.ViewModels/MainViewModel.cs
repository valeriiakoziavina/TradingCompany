using TradingCompany.BL;

namespace TradingCompany.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IUserSession _userSession;

    public MainViewModel(IUserSession userSession, IAuthService authService, INavigationService navigationService)
    {
        _userSession = userSession;
        OpenDeliveryOrdersCommand = new RelayCommand(navigationService.ShowDeliveryOrders, () => IsDeliveryManager);
        LogoutCommand = new RelayCommand(() =>
        {
            authService.Logout();
            navigationService.ShowLogin();
        });
    }

    public string UserName => _userSession.CurrentUser?.FullName ?? string.Empty;
    public bool IsDeliveryManager => _userSession.IsDeliveryManager;
    public string RoleName => IsDeliveryManager ? "Менеджер доставки" : "Клієнт";

    public string? AccessMessage => IsDeliveryManager
        ? null
        : $"Ваша роль «{RoleName}» не має доступу до функцій менеджера доставки. Зверніться до адміністратора.";

    public RelayCommand OpenDeliveryOrdersCommand { get; }
    public RelayCommand LogoutCommand { get; }
}
