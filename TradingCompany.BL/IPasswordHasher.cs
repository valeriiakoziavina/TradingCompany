namespace TradingCompany.BL;

public interface IPasswordHasher
{
    byte[] Hash(string password, Guid salt);
    bool Verify(string password, Guid salt, byte[] expectedHash);
}
