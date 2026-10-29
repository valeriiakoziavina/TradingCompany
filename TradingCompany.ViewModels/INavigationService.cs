namespace TradingCompany.ViewModels;

public interface INavigationService
{
    void ShowLogin();
    void ShowMain();
    void ShowDeliveryOrders();

    bool ShowCreateWaybill(CreateWaybillViewModel viewModel);
}
