using System.Linq.Expressions;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests;

/// <summary>
/// Especificación única de qué solicitudes ve cada rol; la usan tanto las consultas SQL como las validaciones en memoria.
/// </summary>
public sealed class ServiceRequestVisibility
{
    private ServiceRequestVisibility(Guid userId, UserRole role)
    {
        UserId = userId;
        Role = role;
    }

    public Guid UserId { get; }

    public UserRole Role { get; }

    public bool CanSeeInternalComments => Role is UserRole.Administrator or UserRole.Analyst;

    public bool CanManage => Role is UserRole.Administrator or UserRole.Analyst;

    public static ServiceRequestVisibility For(ICurrentUser currentUser) => new(currentUser.UserId, currentUser.Role);

    public static ServiceRequestVisibility For(Guid userId, UserRole role) => new(userId, role);

    public Expression<Func<ServiceRequest, bool>> ToExpression()
    {
        Guid userId = UserId;
        return Role switch
        {
            UserRole.Administrator => serviceRequest => true,
            UserRole.Analyst => serviceRequest => serviceRequest.RequesterId == userId
                || serviceRequest.AssigneeId == null
                || serviceRequest.AssigneeId == userId,
            _ => serviceRequest => serviceRequest.RequesterId == userId,
        };
    }

    public bool CanView(ServiceRequest serviceRequest) => Role switch
    {
        UserRole.Administrator => true,
        UserRole.Analyst => serviceRequest.RequesterId == UserId
            || serviceRequest.AssigneeId is null
            || serviceRequest.AssigneeId == UserId,
        _ => serviceRequest.RequesterId == UserId,
    };
}
