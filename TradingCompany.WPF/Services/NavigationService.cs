using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TradingCompany.ViewModels;
using TradingCompany.WPF.Views;

namespace TradingCompany.WPF.Services;

public class NavigationService(IServiceProvider services) : INavigationService
{
    public void ShowLogin() => SwitchTo<LoginWindow>();

    public void ShowMain() => SwitchTo<MainWindow>();

    public void ShowDeliveryOrders() => SwitchTo<DeliveryOrdersWindow>();

    public bool ShowCreateWaybill(CreateWaybillViewModel viewModel)
    {
        var window = new CreateWaybillWindow(viewModel) { Owner = Application.Current.MainWindow };
        return window.ShowDialog() == true;
    }

    private void SwitchTo<TWindow>() where TWindow : Window
    {
        var next = services.GetRequiredService<TWindow>();
        var previous = Application.Current.Windows.OfType<Window>().Where(w => w != next).ToList();

        Application.Current.MainWindow = next;
        next.Show();
        foreach (var window in previous)
            window.Close();
    }
}
