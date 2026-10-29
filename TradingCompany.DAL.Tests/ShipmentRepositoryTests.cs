using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;

namespace TradingCompany.DAL.Tests;

[TestFixture]
public class ShipmentRepositoryTests : TestBase
{
    private ShipmentRepository _repository = null!;
    private Waybill _waybill = null!;
    private User _manager = null!;
    private ShipmentStatus _created = null!;
    private ShipmentStatus _inTransit = null!;

    [SetUp]
    public async Task SetUp()
    {
        _repository = new ShipmentRepository(Context);
        _waybill = await SeedWaybillAsync("WB-001");
        _manager = await SeedUserAsync("manager", UserRole.DeliveryManager);
        _created = (await Context.ShipmentStatuses.FindAsync(ShipmentStatus.PendingId))!;
        _inTransit = (await Context.ShipmentStatuses.FindAsync(ShipmentStatus.ShippedId))!;
    }

    private async Task<Shipment> SeedShipmentAsync(string? notes = null)
    {
        var shipment = new Shipment
        {
            WaybillId = _waybill.Id,
            ManagerId = _manager.Id,
            StatusId = _created.Id,
            Notes = notes
        };
        Context.Shipments.Add(shipment);
        await Context.SaveChangesAsync();
        return shipment;
    }

    [Test]
    public async Task AddAsync_ValidShipment_AssignsIdAndSavesToDatabase()
    {
        var dispatched = new DateTime(2026, 10, 1, 12, 0, 0);
        var shipment = new Shipment
        {
            WaybillId = _waybill.Id,
            ManagerId = _manager.Id,
            StatusId = _created.Id,
            DispatchedAt = dispatched,
            Notes = "Fragile"
        };

        await _repository.AddAsync(shipment);

        Assert.That(shipment.Id, Is.GreaterThan(0));

        await using var verify = CreateContext();
        var saved = await verify.Shipments.FindAsync(shipment.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved!.WaybillId, Is.EqualTo(_waybill.Id));
            Assert.That(saved.ManagerId, Is.EqualTo(_manager.Id));
            Assert.That(saved.StatusId, Is.EqualTo(_created.Id));
            Assert.That(saved.DispatchedAt, Is.EqualTo(dispatched));
            Assert.That(saved.Notes, Is.EqualTo("Fragile"));
        });
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ReturnsShipment()
    {
        var shipment = await SeedShipmentAsync("Check");

        var result = await _repository.GetByIdAsync(shipment.Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.WaybillId, Is.EqualTo(_waybill.Id));
            Assert.That(result.Notes, Is.EqualTo("Check"));
        });
    }

    [Test]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_TwoShipmentsInDatabase_ReturnsBoth()
    {
        await SeedShipmentAsync("first");
        await SeedShipmentAsync("second");

        var result = await _repository.GetAllAsync();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task UpdateAsync_ChangedStatusAndNotes_PersistsNewValues()
    {
        var shipment = await SeedShipmentAsync();

        shipment.StatusId = _inTransit.Id;
        shipment.Notes = "Left the warehouse";
        await _repository.UpdateAsync(shipment);

        await using var verify = CreateContext();
        var updated = await verify.Shipments.FindAsync(shipment.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updated!.StatusId, Is.EqualTo(_inTransit.Id));
            Assert.That(updated.Notes, Is.EqualTo("Left the warehouse"));
        });
    }

    [Test]
    public async Task DeleteAsync_ExistingShipment_RemovesFromDatabase()
    {
        var shipment = await SeedShipmentAsync();

        await _repository.DeleteAsync(shipment.Id);

        await using var verify = CreateContext();
        Assert.That(await verify.Shipments.FindAsync(shipment.Id), Is.Null);
    }
}
