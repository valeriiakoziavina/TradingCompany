using System.Windows;
using TradingCompany.ViewModels;

namespace TradingCompany.WPF.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.PasswordProvider = () => pwdPassword.Password;
        viewModel.PasswordResetRequested += (_, _) =>
        {
            pwdPassword.Clear();
            pwdPassword.Focus();
        };
    }
}
