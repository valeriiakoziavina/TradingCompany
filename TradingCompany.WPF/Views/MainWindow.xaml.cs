using System.Windows;
using TradingCompany.ViewModels;

namespace TradingCompany.WPF.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
