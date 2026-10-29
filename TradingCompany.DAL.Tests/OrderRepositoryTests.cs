using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;

namespace TradingCompany.DAL.Tests;

[TestFixture]
public class OrderRepositoryTests : TestBase
{
    private OrderRepository _repository = null!;
    private User _customer = null!;

    [SetUp]
    public async Task SetUp()
    {
        _repository = new OrderRepository(Context);
        _customer = await SeedUserAsync("customer");
    }

    [Test]
    public async Task AddAsync_ValidOrder_AssignsIdAndSavesToDatabase()
    {
        var order = new Order { OrderNumber = "ORD-100", CustomerId = _customer.Id, TotalAmount = 1500m };

        await _repository.AddAsync(order);

        Assert.That(order.Id, Is.GreaterThan(0));

        await using var verify = CreateContext();
        var saved = await verify.Orders.FindAsync(order.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved!.OrderNumber, Is.EqualTo("ORD-100"));
            Assert.That(saved.CustomerId, Is.EqualTo(_customer.Id));
            Assert.That(saved.TotalAmount, Is.EqualTo(1500m));
        });
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ReturnsOrder()
    {
        var order = await SeedOrderAsync("ORD-200", _customer);

        var result = await _repository.GetByIdAsync(order.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.OrderNumber, Is.EqualTo("ORD-200"));
    }

    [Test]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_TwoOrdersInDatabase_ReturnsBoth()
    {
        await SeedOrderAsync("ORD-1", _customer);
        await SeedOrderAsync("ORD-2", _customer);

        var result = await _repository.GetAllAsync();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAllWithDeliveryAsync_LoadsCustomerWaybillShipmentAndStatus()
    {
        var order = await SeedOrderAsync("ORD-500", _customer);
        var waybill = await SeedWaybillAsync("WB-500", order);
        var manager = await SeedUserAsync("manager", UserRole.DeliveryManager);
        Context.Shipments.Add(new Shipment { WaybillId = waybill.Id, ManagerId = manager.Id, StatusId = ShipmentStatus.PendingId });
        await Context.SaveChangesAsync();
        await SeedOrderAsync("ORD-501", _customer);

        await using var readContext = CreateContext();
        var result = (await new OrderRepository(readContext).GetAllWithDeliveryAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        var withWaybill = result.Single(o => o.OrderNumber == "ORD-500");
        Assert.Multiple(() =>
        {
            Assert.That(withWaybill.Customer, Is.Not.Null);
            Assert.That(withWaybill.Waybill, Is.Not.Null);
            Assert.That(withWaybill.Waybill!.Shipments, Has.Count.EqualTo(1));
            Assert.That(withWaybill.Waybill.Shipments.First().Status!.Name, Is.EqualTo("Очікує відправлення"));
            Assert.That(result.Single(o => o.OrderNumber == "ORD-501").Waybill, Is.Null);
        });
    }

    [Test]
    public async Task UpdateAsync_ChangedTotal_PersistsNewValue()
    {
        var order = await SeedOrderAsync("ORD-300", _customer);

        order.TotalAmount = 999.99m;
        await _repository.UpdateAsync(order);

        await using var verify = CreateContext();
        var updated = await verify.Orders.FindAsync(order.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.TotalAmount, Is.EqualTo(999.99m));
    }

    [Test]
    public async Task DeleteAsync_ExistingOrder_RemovesFromDatabase()
    {
        var order = await SeedOrderAsync("ORD-400", _customer);

        await _repository.DeleteAsync(order.Id);

        await using var verify = CreateContext();
        Assert.That(await verify.Orders.FindAsync(order.Id), Is.Null);
    }
}
