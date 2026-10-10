using System.Collections.Frozen;

namespace SB.Solicitudes.Dominio.ServiceRequests;

public static class StatusTransitions
{
    private static readonly FrozenDictionary<ServiceRequestStatus, ServiceRequestStatus[]> ALLOWED_TRANSITIONS =
        new Dictionary<ServiceRequestStatus, ServiceRequestStatus[]>
        {
            [ServiceRequestStatus.Registered] = [ServiceRequestStatus.InAnalysis, ServiceRequestStatus.Closed],
            [ServiceRequestStatus.InAnalysis] =
            [
                ServiceRequestStatus.InProgress,
                ServiceRequestStatus.WaitingForRequester,
                ServiceRequestStatus.Resolved,
                ServiceRequestStatus.Closed,
            ],
            [ServiceRequestStatus.InProgress] =
            [
                ServiceRequestStatus.InAnalysis,
                ServiceRequestStatus.WaitingForRequester,
                ServiceRequestStatus.Resolved,
            ],
            [ServiceRequestStatus.WaitingForRequester] =
            [
                ServiceRequestStatus.InAnalysis,
                ServiceRequestStatus.InProgress,
                ServiceRequestStatus.Closed,
            ],
            [ServiceRequestStatus.Resolved] = [ServiceRequestStatus.InProgress, ServiceRequestStatus.Closed],
            [ServiceRequestStatus.Closed] = [ServiceRequestStatus.InAnalysis],
        }.ToFrozenDictionary();

    public static bool IsAllowed(ServiceRequestStatus from, ServiceRequestStatus to)
        => ALLOWED_TRANSITIONS.TryGetValue(from, out ServiceRequestStatus[]? targets) && targets.Contains(to);

    public static IReadOnlyCollection<ServiceRequestStatus> AllowedFrom(ServiceRequestStatus from)
        => ALLOWED_TRANSITIONS.TryGetValue(from, out ServiceRequestStatus[]? targets) ? targets : [];

    public static bool IsReopening(ServiceRequestStatus from) => from == ServiceRequestStatus.Closed;

    public static bool RequiresComment(ServiceRequestStatus from, ServiceRequestStatus to)
        => to is ServiceRequestStatus.Resolved or ServiceRequestStatus.Closed || IsReopening(from);

    public static bool IsOpen(ServiceRequestStatus status)
        => status is not (ServiceRequestStatus.Resolved or ServiceRequestStatus.Closed);
}
