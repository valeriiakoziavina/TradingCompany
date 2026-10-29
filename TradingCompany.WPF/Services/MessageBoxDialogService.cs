using System.Windows;
using TradingCompany.ViewModels;

namespace TradingCompany.WPF.Services;

public class MessageBoxDialogService : IDialogService
{
    public bool Confirm(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
