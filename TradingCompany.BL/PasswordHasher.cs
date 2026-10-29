using System.Security.Cryptography;

namespace TradingCompany.BL;

public class PasswordHasher : IPasswordHasher
{
    public const int Iterations = 210_000;
    public const int HashSize = 32;

    public byte[] Hash(string password, Guid salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt.ToByteArray(), Iterations, HashAlgorithmName.SHA512, HashSize);
    }

    public bool Verify(string password, Guid salt, byte[] expectedHash)
    {
        return CryptographicOperations.FixedTimeEquals(Hash(password, salt), expectedHash);
    }
}
