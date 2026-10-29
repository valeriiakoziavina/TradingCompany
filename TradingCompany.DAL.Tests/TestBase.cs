using Microsoft.EntityFrameworkCore;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Context;

namespace TradingCompany.DAL.Tests;

public abstract class TestBase
{
    private const string ConnectionString =
        "Server=DESKTOP-QPRTM95;Database=projectTradingCompany_Tests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    protected TradingDbContext Context = null!;

    [SetUp]
    public async Task BaseSetUp()
    {
        Context = CreateContext();
        await Context.Database.EnsureDeletedAsync();
        await Context.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }

    protected static TradingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TradingDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new TradingDbContext(options);
    }

    protected async Task<User> SeedUserAsync(string login = "user", UserRole role = UserRole.Customer)
    {
        var user = new User
        {
            Login = login,
            PasswordHash = new byte[32],
            PasswordSalt = Guid.NewGuid(),
            Role = role,
            FullName = "Test User " + login,
            PhoneNumber = "+380000000000",
            Email = login + "@test.com"
        };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user;
    }

    protected async Task<Order> SeedOrderAsync(string number = "ORD-001", User? customer = null)
    {
        customer ??= await SeedUserAsync("customer_" + number);
        var order = new Order { OrderNumber = number, CustomerId = customer.Id, TotalAmount = 500m };
        Context.Orders.Add(order);
        await Context.SaveChangesAsync();
        return order;
    }

    protected async Task<Waybill> SeedWaybillAsync(string number = "WB-001", Order? order = null)
    {
        order ??= await SeedOrderAsync("ORD-" + number);
        var waybill = new Waybill
        {
            WaybillNumber = number,
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            PaymentType = "Card",
            DeliveryMethod = "Courier",
            City = "Kyiv",
            DeliveryAddress = "Khreshchatyk 1"
        };
        Context.Waybills.Add(waybill);
        await Context.SaveChangesAsync();
        return waybill;
    }

    protected async Task<ShipmentStatus> SeedStatusAsync(string name = "Created")
    {
        var status = new ShipmentStatus { Name = name };
        Context.ShipmentStatuses.Add(status);
        await Context.SaveChangesAsync();
        return status;
    }
}
