using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;

namespace TradingCompany.DAL.Tests;

[TestFixture]
public class ShipmentStatusRepositoryTests : TestBase
{
    private ShipmentStatusRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new ShipmentStatusRepository(Context);
    }

    [Test]
    public async Task AddAsync_ValidStatus_AssignsIdAndSavesToDatabase()
    {
        var status = new ShipmentStatus { Name = "In transit" };

        await _repository.AddAsync(status);

        Assert.That(status.Id, Is.GreaterThan(0));

        await using var verify = CreateContext();
        var saved = await verify.ShipmentStatuses.FindAsync(status.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Name, Is.EqualTo("In transit"));
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ReturnsStatus()
    {
        var status = await SeedStatusAsync("Delivered");

        var result = await _repository.GetByIdAsync(status.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Delivered"));
    }

    [Test]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_SeededStatusesPlusOneNew_ReturnsAllThree()
    {
        await SeedStatusAsync("Delivered");

        var result = await _repository.GetAllAsync();

        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task UpdateAsync_ChangedName_PersistsNewValue()
    {
        var status = await SeedStatusAsync("Created");

        status.Name = "Formed";
        await _repository.UpdateAsync(status);

        await using var verify = CreateContext();
        var updated = await verify.ShipmentStatuses.FindAsync(status.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("Formed"));
    }

    [Test]
    public async Task DeleteAsync_ExistingStatus_RemovesFromDatabase()
    {
        var status = await SeedStatusAsync("Cancelled");

        await _repository.DeleteAsync(status.Id);

        await using var verify = CreateContext();
        Assert.That(await verify.ShipmentStatuses.FindAsync(status.Id), Is.Null);
    }
}
