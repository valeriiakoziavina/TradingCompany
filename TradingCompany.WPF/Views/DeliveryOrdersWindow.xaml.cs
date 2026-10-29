using System.Windows;
using TradingCompany.ViewModels;

namespace TradingCompany.WPF.Views;

public partial class DeliveryOrdersWindow : Window
{
    public DeliveryOrdersWindow(DeliveryOrdersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.LoadCommand.Execute(null);
    }
}
