using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Aplicacion.Features.Notifications;

public sealed class NotificationOptions
{
    public const string SECTION_NAME = "Notifications";

    public IReadOnlyList<NotificationChannel> Channels { get; init; } = [NotificationChannel.InApp, NotificationChannel.Email];
}

internal static class ServiceRequestStatusNames
{
    public const string REGISTERED = "Registrada";
    public const string IN_ANALYSIS = "En análisis";
    public const string IN_PROGRESS = "En progreso";
    public const string WAITING_FOR_REQUESTER = "En espera del solicitante";
    public const string RESOLVED = "Resuelta";
    public const string CLOSED = "Cerrada";

    public static string From(ServiceRequestStatus status) => status switch
    {
        ServiceRequestStatus.Registered => REGISTERED,
        ServiceRequestStatus.InAnalysis => IN_ANALYSIS,
        ServiceRequestStatus.InProgress => IN_PROGRESS,
        ServiceRequestStatus.WaitingForRequester => WAITING_FOR_REQUESTER,
        ServiceRequestStatus.Resolved => RESOLVED,
        ServiceRequestStatus.Closed => CLOSED,
        _ => status.ToString(),
    };
}
