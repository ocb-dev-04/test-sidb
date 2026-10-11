using Microsoft.AspNetCore.Identity;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Infraestructura.Authentication;

/// <summary>
/// Delegado en el hasher de ASP.NET Core Identity (PBKDF2-HMAC-SHA512, sal aleatoria, formato versionado).
/// </summary>
internal sealed class IdentityPasswordHasher : IPasswordHasher
{
    // La implementación por defecto no usa el usuario; la clave se genera antes de que el usuario exista.
    private static readonly User NO_USER = null!;

    private readonly PasswordHasher<User> _passwordHasher = new();

    public string Hash(string password) => _passwordHasher.HashPassword(NO_USER, password);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return _passwordHasher.VerifyHashedPassword(NO_USER, passwordHash, password) != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            // Un hash dañado en la base es un login fallido, no un error 500.
            return false;
        }
    }
}
