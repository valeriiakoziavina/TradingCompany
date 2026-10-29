namespace TradingCompany.BL.Tests;

[TestFixture]
public class PasswordHasherTests
{
    private PasswordHasher _hasher = null!;

    [SetUp]
    public void SetUp()
    {
        _hasher = new PasswordHasher();
    }

    [Test]
    public void Hash_SamePasswordAndSalt_ReturnsSameHash()
    {
        var salt = Guid.NewGuid();

        Assert.That(_hasher.Hash("secret1", salt), Is.EqualTo(_hasher.Hash("secret1", salt)));
    }

    [Test]
    public void Hash_SamePasswordDifferentSalt_ReturnsDifferentHashes()
    {
        var first = _hasher.Hash("secret1", Guid.NewGuid());
        var second = _hasher.Hash("secret1", Guid.NewGuid());

        Assert.That(first, Is.Not.EqualTo(second));
        Assert.That(first, Has.Length.EqualTo(PasswordHasher.HashSize));
    }

    [Test]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var salt = Guid.NewGuid();
        var hash = _hasher.Hash("secret1", salt);

        Assert.That(_hasher.Verify("secret1", salt, hash), Is.True);
    }

    [Test]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var salt = Guid.NewGuid();
        var hash = _hasher.Hash("secret1", salt);

        Assert.That(_hasher.Verify("Secret1", salt, hash), Is.False);
    }
}
