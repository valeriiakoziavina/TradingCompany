using System.Windows;
using TradingCompany.ViewModels;

namespace TradingCompany.WPF.Views;

public partial class CreateWaybillWindow : Window
{
    public CreateWaybillWindow(CreateWaybillViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.RequestClose += (_, created) => DialogResult = created;
    }
}
