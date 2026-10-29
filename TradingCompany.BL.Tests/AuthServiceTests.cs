using Moq;
using TradingCompany.Core.DTOs;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.BL.Tests;

[TestFixture]
public class AuthServiceTests
{
    private static readonly byte[] StoredHash = [1, 2, 3];
    private static readonly Guid StoredSalt = Guid.NewGuid();

    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IPasswordHasher> _hasher = null!;
    private UserSession _session = null!;
    private AuthService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _hasher = new Mock<IPasswordHasher>();
        _session = new UserSession();

        _userRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
        _hasher.Setup(h => h.Hash(It.IsAny<string>(), It.IsAny<Guid>())).Returns([9, 9, 9]);

        _service = new AuthService(_userRepository.Object, _hasher.Object, _session);
    }

    private static User Manager() => new()
    {
        Id = 7, Login = "manager", PasswordHash = StoredHash, PasswordSalt = StoredSalt,
        Role = UserRole.DeliveryManager, FullName = "Олена Коваль", Email = "manager@test.com"
    };

    private static CreateUserDto ValidUser() => new()
    {
        Login = "  olena  ", Password = "secret1", FullName = " Олена Коваль ", PhoneNumber = "+380501112233",
        Email = "olena@test.com", IsDeliveryManager = true
    };

    [Test]
    public async Task LoginAsync_UnknownLogin_ReturnsFalseAndStillComputesHash()
    {
        var result = await _service.LoginAsync("nobody", "secret1");

        Assert.That(result, Is.False);
        Assert.That(_session.IsAuthenticated, Is.False);
        _hasher.Verify(h => h.Hash("secret1", Guid.Empty), Times.Once);
    }

    [Test]
    public async Task LoginAsync_WrongPassword_ReturnsFalse()
    {
        _userRepository.Setup(r => r.GetByLoginAsync("manager")).ReturnsAsync(Manager());
        _hasher.Setup(h => h.Verify("wrong", StoredSalt, StoredHash)).Returns(false);

        var result = await _service.LoginAsync("manager", "wrong");

        Assert.That(result, Is.False);
        Assert.That(_session.IsAuthenticated, Is.False);
    }

    [Test]
    public async Task LoginAsync_CorrectPassword_SignsInWithRole()
    {
        _userRepository.Setup(r => r.GetByLoginAsync("manager")).ReturnsAsync(Manager());
        _hasher.Setup(h => h.Verify("secret1", StoredSalt, StoredHash)).Returns(true);

        var result = await _service.LoginAsync("  manager ", "secret1");

        Assert.That(result, Is.True);
        Assert.That(_session.CurrentUser!.Login, Is.EqualTo("manager"));
        Assert.That(_session.IsDeliveryManager, Is.True);
    }

    [Test]
    public void Logout_ClearsSession()
    {
        _session.SignIn(new UserDto { Id = 1, Login = "manager" });

        _service.Logout();

        Assert.That(_session.IsAuthenticated, Is.False);
    }

    [Test]
    public async Task CreateUserAsync_ValidUser_HashesPasswordAndSavesNormalizedData()
    {
        User? saved = null;
        _userRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => { u.Id = 42; saved = u; })
            .Returns(Task.CompletedTask);

        var id = await _service.CreateUserAsync(ValidUser());

        Assert.That(id, Is.EqualTo(42));
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved!.Login, Is.EqualTo("olena"));
            Assert.That(saved.FullName, Is.EqualTo("Олена Коваль"));
            Assert.That(saved.Role, Is.EqualTo(UserRole.DeliveryManager));
            Assert.That(saved.PasswordHash, Is.EqualTo(new byte[] { 9, 9, 9 }));
            Assert.That(saved.PasswordSalt, Is.Not.EqualTo(Guid.Empty));
        });
        _hasher.Verify(h => h.Hash("secret1", saved!.PasswordSalt), Times.Once);
    }

    [TestCase("")]
    [TestCase("12345")]
    public void CreateUserAsync_ShortPassword_ThrowsAndDoesNotSave(string password)
    {
        var user = ValidUser();
        user.Password = password;

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateUserAsync(user));

        Assert.That(ex!.Errors, Has.Some.Contains("щонайменше 6"));
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void CreateUserAsync_EmptyLogin_Throws(string login)
    {
        var user = ValidUser();
        user.Login = login;

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateUserAsync(user));

        Assert.That(ex!.Errors, Has.Some.Contains("Логін обов'язковий"));
    }

    [Test]
    public void CreateUserAsync_LoginLongerThan50_Throws()
    {
        var user = ValidUser();
        user.Login = new string('a', AuthService.LoginMaxLength + 1);

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateUserAsync(user));
    }

    [Test]
    public void CreateUserAsync_DuplicateLoginIgnoringCase_Throws()
    {
        _userRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { new() { Login = "OLENA", Email = "x@test.com" } });

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateUserAsync(ValidUser()));

        Assert.That(ex!.Errors, Has.Some.Contains("вже зайнятий"));
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public void CreateUserAsync_InvalidEmail_Throws()
    {
        var user = ValidUser();
        user.Email = "no-at-sign";

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateUserAsync(user));

        Assert.That(ex!.Errors, Has.Some.Contains("email"));
    }

    [Test]
    public async Task ChangePasswordAsync_GeneratesNewSaltAndHash()
    {
        var user = Manager();
        _userRepository.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(user);

        var result = await _service.ChangePasswordAsync(7, "newpass1");

        Assert.That(result, Is.True);
        Assert.That(user.PasswordSalt, Is.Not.EqualTo(StoredSalt));
        Assert.That(user.PasswordHash, Is.EqualTo(new byte[] { 9, 9, 9 }));
        _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Test]
    public void ChangePasswordAsync_ShortPassword_ThrowsAndDoesNotUpdate()
    {
        Assert.ThrowsAsync<BusinessRuleException>(() => _service.ChangePasswordAsync(7, "123"));
        _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task ChangePasswordAsync_UnknownUser_ReturnsFalse()
    {
        var result = await _service.ChangePasswordAsync(99, "newpass1");

        Assert.That(result, Is.False);
    }

    [Test]
    public void DeleteAsync_CurrentUser_Throws()
    {
        _session.SignIn(new UserDto { Id = 7, Login = "manager" });

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.DeleteAsync(7));
        _userRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
