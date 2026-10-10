using System.Security.Claims;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Dominio.Users;
using SB.Solicitudes.Infraestructura.Authentication;

namespace SB.Solicitudes.Servicios.Common;

internal sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid UserId => Guid.TryParse(Principal?.FindFirstValue(ApplicationClaimTypes.USER_ID), out Guid userId)
        ? userId
        : throw new InvalidOperationException("The current request has no authenticated user.");

    public UserRole Role => RoleNames.TryParse(Principal?.FindFirstValue(ApplicationClaimTypes.ROLE), out UserRole role)
        ? role
        : throw new InvalidOperationException("The current user has no valid role claim.");
}
