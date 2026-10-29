using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingCompany.BL;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DAL.Repositories;
using TradingCompany.ViewModels;
using TradingCompany.WPF.Services;
using TradingCompany.WPF.Views;

namespace TradingCompany.WPF;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("TradingCompany");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            MessageBox.Show("У appsettings.json не задано ConnectionStrings:TradingCompany.", "Помилка конфігурації",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        var services = new ServiceCollection();
        ConfigureServices(services, connectionString);

        Services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        var loginWindow = Services.GetRequiredService<LoginWindow>();
        MainWindow = loginWindow;
        loginWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TradingDbContext>(
            options => options.UseSqlServer(connectionString),
            ServiceLifetime.Transient,
            ServiceLifetime.Singleton);
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IOrderRepository, OrderRepository>();
        services.AddTransient<IWaybillRepository, WaybillRepository>();
        services.AddTransient<IShipmentRepository, ShipmentRepository>();
        services.AddTransient<IShipmentStatusRepository, ShipmentStatusRepository>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IUserSession, UserSession>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IDeliveryService, DeliveryService>();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, MessageBoxDialogService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DeliveryOrdersViewModel>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<Views.MainWindow>();
        services.AddTransient<DeliveryOrdersWindow>();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
