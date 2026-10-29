using TradingCompany.BL;

namespace TradingCompany.ViewModels;

public class LoginViewModel : ValidatableViewModelBase
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private string _login = string.Empty;
    private string? _errorMessage;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        LoginCommand = new AsyncRelayCommand(SignInAsync);
    }

    public string Login
    {
        get => _login;
        set
        {
            if (SetProperty(ref _login, value))
                ValidateLogin();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public Func<string> PasswordProvider { get; set; } = () => string.Empty;

    public event EventHandler? PasswordResetRequested;

    public AsyncRelayCommand LoginCommand { get; }

    private async Task SignInAsync()
    {
        if (!ValidateAll())
            return;

        var password = PasswordProvider();
        if (password.Length == 0)
        {
            ErrorMessage = "Введіть пароль.";
            return;
        }

        if (!await _authService.LoginAsync(Login, password))
        {
            ErrorMessage = "Невірний логін або пароль.";
            PasswordResetRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        ErrorMessage = null;
        _navigationService.ShowMain();
    }

    protected override void ValidateAllProperties() => ValidateLogin();

    private void ValidateLogin()
    {
        var errors = new List<string>();
        var login = Login.Trim();
        if (login.Length == 0)
            errors.Add("Введіть логін.");
        else if (login.Length > AuthService.LoginMaxLength)
            errors.Add($"Не довше за {AuthService.LoginMaxLength} символів.");

        SetErrors(nameof(Login), errors);
    }
}
