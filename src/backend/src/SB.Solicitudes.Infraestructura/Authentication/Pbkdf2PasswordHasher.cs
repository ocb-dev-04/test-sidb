using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;

namespace SB.Solicitudes.Infraestructura.Authentication;

/// <summary>
/// PBKDF2-SHA256 con sal aleatoria. Formato: algoritmo.iteraciones.sal.hash (Base64).
/// </summary>
internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public const string ALGORITHM = "PBKDF2-SHA256";
    public const int ITERATIONS = 100_000;
    public const int SALT_SIZE_BYTES = 16;
    public const int HASH_SIZE_BYTES = 32;

    private const char SEPARATOR = '.';
    private const int HASH_PARTS_COUNT = 4;
    private const int ALGORITHM_INDEX = 0;
    private const int ITERATIONS_INDEX = 1;
    private const int SALT_INDEX = 2;
    private const int HASH_INDEX = 3;

    private static readonly HashAlgorithmName HASH_ALGORITHM = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SALT_SIZE_BYTES);
        byte[] hash = Derive(password, salt, ITERATIONS);

        return string.Join(
            SEPARATOR,
            ALGORITHM,
            ITERATIONS.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string passwordHash)
    {
        string[] parts = passwordHash.Split(SEPARATOR);
        if (parts.Length != HASH_PARTS_COUNT
            || parts[ALGORITHM_INDEX] != ALGORITHM
            || !int.TryParse(parts[ITERATIONS_INDEX], NumberStyles.None, CultureInfo.InvariantCulture, out int iterations))
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[SALT_INDEX]);
            byte[] expectedHash = Convert.FromBase64String(parts[HASH_INDEX]);
            byte[] actualHash = Derive(password, salt, iterations);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Derive(string password, byte[] salt, int iterations)
        => Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HASH_ALGORITHM, HASH_SIZE_BYTES);
}
