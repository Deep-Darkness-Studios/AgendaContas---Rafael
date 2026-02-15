using System.Security.Cryptography;

namespace AgendaContas.Domain.Services;

public static class PasswordHasher
{
    public const int DefaultIterations = 120_000;

    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static (string Hash, string Salt, int Iterations) HashPassword(string password, int iterations = DefaultIterations)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Senha não pode ser vazia.", nameof(password));
        }

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return (
            Convert.ToBase64String(hashBytes),
            Convert.ToBase64String(saltBytes),
            iterations);
    }

    public static bool VerifyPassword(string password, string hashBase64, string saltBase64, int iterations)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(hashBase64) ||
            string.IsNullOrWhiteSpace(saltBase64) ||
            iterations <= 0)
        {
            return false;
        }

        try
        {
            var saltBytes = Convert.FromBase64String(saltBase64);
            var expectedHash = Convert.FromBase64String(hashBase64);
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(expectedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }
}
