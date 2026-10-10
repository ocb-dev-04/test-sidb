namespace SB.Solicitudes.Dominio.ServiceRequests;

public static class PriorityServiceLevels
{
    public const int CRITICAL_RESOLUTION_HOURS = 8;
    public const int HIGH_RESOLUTION_HOURS = 24;
    public const int MEDIUM_RESOLUTION_HOURS = 72;
    public const int LOW_RESOLUTION_HOURS = 120;

    public static TimeSpan ResolutionTimeFor(ServiceRequestPriority priority) => priority switch
    {
        ServiceRequestPriority.Critical => TimeSpan.FromHours(CRITICAL_RESOLUTION_HOURS),
        ServiceRequestPriority.High => TimeSpan.FromHours(HIGH_RESOLUTION_HOURS),
        ServiceRequestPriority.Medium => TimeSpan.FromHours(MEDIUM_RESOLUTION_HOURS),
        ServiceRequestPriority.Low => TimeSpan.FromHours(LOW_RESOLUTION_HOURS),
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Unknown priority."),
    };
}
