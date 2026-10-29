using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;

namespace TradingCompany.DAL.Tests;

[TestFixture]
public class WaybillRepositoryTests : TestBase
{
    private WaybillRepository _repository = null!;
    private User _customer = null!;

    [SetUp]
    public async Task SetUp()
    {
        _repository = new WaybillRepository(Context);
        _customer = await SeedUserAsync("customer");
    }

    [Test]
    public async Task AddAsync_ValidWaybill_AssignsIdAndSavesToDatabase()
    {
        var order = await SeedOrderAsync("ORD-100", _customer);
        var waybill = new Waybill
        {
            WaybillNumber = "WB-100",
            OrderId = order.Id,
            CustomerId = _customer.Id,
            PaymentType = "Cash",
            DeliveryMethod = "Pickup",
            City = "Lviv",
            DeliveryAddress = "Svobody 10"
        };

        await _repository.AddAsync(waybill);

        Assert.That(waybill.Id, Is.GreaterThan(0));

        await using var verify = CreateContext();
        var saved = await verify.Waybills.FindAsync(waybill.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved!.WaybillNumber, Is.EqualTo("WB-100"));
            Assert.That(saved.OrderId, Is.EqualTo(order.Id));
            Assert.That(saved.City, Is.EqualTo("Lviv"));
            Assert.That(saved.PaymentType, Is.EqualTo("Cash"));
        });
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ReturnsWaybillWithCustomer()
    {
        var order = await SeedOrderAsync("ORD-200", _customer);
        var waybill = await SeedWaybillAsync("WB-200", order);

        var result = await _repository.GetByIdAsync(waybill.Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.WaybillNumber, Is.EqualTo("WB-200"));
            Assert.That(result.Customer, Is.Not.Null);
            Assert.That(result.Customer!.Login, Is.EqualTo("customer"));
        });
    }

    [Test]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_TwoWaybillsInDatabase_ReturnsBoth()
    {
        await SeedWaybillAsync("WB-1", await SeedOrderAsync("ORD-1", _customer));
        await SeedWaybillAsync("WB-2", await SeedOrderAsync("ORD-2", _customer));

        var result = await _repository.GetAllAsync();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetFilteredAsync_ByCityAndPaymentType_ReturnsOnlyMatching()
    {
        var kyiv = await SeedWaybillAsync("WB-K", await SeedOrderAsync("ORD-K", _customer));
        var lviv = await SeedWaybillAsync("WB-L", await SeedOrderAsync("ORD-L", _customer));
        lviv.City = "Lviv";
        lviv.PaymentType = "Cash";
        await Context.SaveChangesAsync();

        var result = (await _repository.GetFilteredAsync(null, null, "Lviv", "Cash")).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].WaybillNumber, Is.EqualTo("WB-L"));
        Assert.That(result.Select(w => w.Id), Does.Not.Contain(kyiv.Id));
    }

    [Test]
    public async Task ExistsByNumberAsync_ReturnsTrueOnlyForExistingNumber()
    {
        await SeedWaybillAsync("WB-777", await SeedOrderAsync("ORD-777", _customer));

        Assert.That(await _repository.ExistsByNumberAsync("WB-777"), Is.True);
        Assert.That(await _repository.ExistsByNumberAsync("WB-000"), Is.False);
    }

    [Test]
    public async Task UpdateAsync_ChangedDeliveryData_PersistsNewValues()
    {
        var waybill = await SeedWaybillAsync("WB-300", await SeedOrderAsync("ORD-300", _customer));

        waybill.City = "Odesa";
        waybill.DeliveryAddress = "Derybasivska 5";
        waybill.DeliveryMethod = "Nova Poshta";
        await _repository.UpdateAsync(waybill);

        await using var verify = CreateContext();
        var updated = await verify.Waybills.FindAsync(waybill.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updated!.City, Is.EqualTo("Odesa"));
            Assert.That(updated.DeliveryAddress, Is.EqualTo("Derybasivska 5"));
            Assert.That(updated.DeliveryMethod, Is.EqualTo("Nova Poshta"));
        });
    }

    [Test]
    public async Task DeleteAsync_ExistingWaybill_RemovesFromDatabase()
    {
        var waybill = await SeedWaybillAsync("WB-400", await SeedOrderAsync("ORD-400", _customer));

        await _repository.DeleteAsync(waybill.Id);

        await using var verify = CreateContext();
        Assert.That(await verify.Waybills.FindAsync(waybill.Id), Is.Null);
    }
}
