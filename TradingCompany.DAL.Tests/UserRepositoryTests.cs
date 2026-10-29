using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;

namespace TradingCompany.DAL.Tests;

[TestFixture]
public class UserRepositoryTests : TestBase
{
    private UserRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new UserRepository(Context);
    }

    [Test]
    public async Task AddAsync_ValidUser_AssignsIdAndSavesToDatabase()
    {
        var user = new User
        {
            Login = "ivan",
            PasswordHash = new byte[] { 1, 2, 3 },
            PasswordSalt = Guid.NewGuid(),
            Role = UserRole.DeliveryManager,
            FullName = "Ivan Petrenko",
            PhoneNumber = "+380501112233",
            Email = "ivan@test.com"
        };

        await _repository.AddAsync(user);

        Assert.That(user.Id, Is.GreaterThan(0));

        await using var verify = CreateContext();
        var saved = await verify.Users.FindAsync(user.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved!.FullName, Is.EqualTo("Ivan Petrenko"));
            Assert.That(saved.Email, Is.EqualTo("ivan@test.com"));
            Assert.That(saved.Login, Is.EqualTo("ivan"));
            Assert.That(saved.Role, Is.EqualTo(UserRole.DeliveryManager));
            Assert.That(saved.PasswordHash, Is.EqualTo(new byte[] { 1, 2, 3 }));
        });
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        var user = await SeedUserAsync("anna");

        var result = await _repository.GetByIdAsync(user.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Login, Is.EqualTo("anna"));
    }

    [Test]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_TwoUsersInDatabase_ReturnsBoth()
    {
        await SeedUserAsync("a");
        await SeedUserAsync("b");

        var result = await _repository.GetAllAsync();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetByLoginAsync_ExistingLogin_ReturnsUser()
    {
        var user = await SeedUserAsync("manager", UserRole.DeliveryManager);

        var result = await _repository.GetByLoginAsync("manager");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task GetByLoginAsync_UnknownLogin_ReturnsNull()
    {
        await SeedUserAsync("manager");

        var result = await _repository.GetByLoginAsync("nobody");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ChangedFields_PersistsNewValues()
    {
        var user = await SeedUserAsync("old");

        user.FullName = "New Name";
        user.Email = "new@test.com";
        user.PhoneNumber = "+380999999999";
        user.Role = UserRole.DeliveryManager;
        await _repository.UpdateAsync(user);

        await using var verify = CreateContext();
        var updated = await verify.Users.FindAsync(user.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updated!.FullName, Is.EqualTo("New Name"));
            Assert.That(updated.Email, Is.EqualTo("new@test.com"));
            Assert.That(updated.PhoneNumber, Is.EqualTo("+380999999999"));
            Assert.That(updated.Role, Is.EqualTo(UserRole.DeliveryManager));
        });
    }

    [Test]
    public async Task DeleteAsync_ExistingUser_RemovesFromDatabase()
    {
        var user = await SeedUserAsync("delete");

        await _repository.DeleteAsync(user.Id);

        await using var verify = CreateContext();
        Assert.That(await verify.Users.FindAsync(user.Id), Is.Null);
    }
}
