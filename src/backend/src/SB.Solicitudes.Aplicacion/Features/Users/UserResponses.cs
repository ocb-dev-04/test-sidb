using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.Users.GetUsers;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.Users;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedOnUtc)
{
    public static UserResponse From(User user)
        => new(user.Id, user.FullName, user.Email, user.Role, user.IsActive, user.CreatedOnUtc);
}

public sealed record UserLookupResponse(Guid Id, string FullName, UserRole Role);

public interface IUserReadRepository
{
    Task<PagedResult<UserResponse>> GetPagedAsync(GetUsersQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserLookupResponse>> GetAssignableAsync(CancellationToken cancellationToken);
}

public static class PasswordPolicy
{
    public const int MINIMUM_LENGTH = 8;
    public const int MAXIMUM_LENGTH = 128;
}
