using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Dominio.Tests.ServiceRequests;

internal static class ServiceRequestBuilder
{
    public const string CODE = "SOL-2026-0001";
    public const string TITLE = "Acceso a VPN";
    public const string DESCRIPTION = "No puedo conectarme a la VPN institucional.";
    public const int REQUEST_TYPE_ID = 1;
    public const int AREA_ID = 2;
    public const string COMMENT = "Comentario de prueba";

    public static readonly DateTime CREATED_ON_UTC = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
    public static readonly Guid REQUESTER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ANALYST_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static ServiceRequest Registered(ServiceRequestPriority priority = ServiceRequestPriority.Medium)
        => ServiceRequest.Create(CODE, TITLE, DESCRIPTION, priority, REQUEST_TYPE_ID, AREA_ID, REQUESTER_ID, null, CREATED_ON_UTC);

    public static ServiceRequest InStatus(ServiceRequestStatus status)
    {
        ServiceRequest serviceRequest = Registered();
        ServiceRequestStatus[] path = status switch
        {
            ServiceRequestStatus.Registered => [],
            ServiceRequestStatus.InAnalysis => [ServiceRequestStatus.InAnalysis],
            ServiceRequestStatus.InProgress => [ServiceRequestStatus.InAnalysis, ServiceRequestStatus.InProgress],
            ServiceRequestStatus.WaitingForRequester => [ServiceRequestStatus.InAnalysis, ServiceRequestStatus.WaitingForRequester],
            ServiceRequestStatus.Resolved => [ServiceRequestStatus.InAnalysis, ServiceRequestStatus.Resolved],
            ServiceRequestStatus.Closed => [ServiceRequestStatus.InAnalysis, ServiceRequestStatus.Resolved, ServiceRequestStatus.Closed],
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };

        foreach (ServiceRequestStatus step in path)
        {
            serviceRequest.ChangeStatus(step, ANALYST_ID, UserRole.Analyst, COMMENT, CREATED_ON_UTC).IsSuccess.ShouldBeTrue();
        }

        serviceRequest.ClearDomainEvents();
        return serviceRequest;
    }
}
