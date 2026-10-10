using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Abstractions.Authentication;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid UserId { get; }

    UserRole Role { get; }
}

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}

public interface IJwtTokenProvider
{
    AccessToken Create(User user);
}

public sealed record AccessToken(string Value, DateTime ExpiresOnUtc);
