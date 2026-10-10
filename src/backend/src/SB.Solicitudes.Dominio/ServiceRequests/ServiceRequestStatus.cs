namespace SB.Solicitudes.Dominio.ServiceRequests;

public enum ServiceRequestStatus
{
    Registered = 1,
    InAnalysis = 2,
    InProgress = 3,
    WaitingForRequester = 4,
    Resolved = 5,
    Closed = 6,
}
