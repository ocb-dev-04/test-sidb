using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Dominio.ServiceRequests;

public sealed class StatusHistory : Entity<Guid>
{
    private StatusHistory()
    {
    }

    private StatusHistory(Guid id)
        : base(id)
    {
    }

    public Guid ServiceRequestId { get; private set; }

    public ServiceRequestStatus? PreviousStatus { get; private set; }

    public ServiceRequestStatus NewStatus { get; private set; }

    public Guid ChangedById { get; private set; }

    public User ChangedBy { get; private set; } = null!;

    public DateTime ChangedOnUtc { get; private set; }

    public string? Comment { get; private set; }

    internal static StatusHistory Create(
        Guid serviceRequestId,
        ServiceRequestStatus? previousStatus,
        ServiceRequestStatus newStatus,
        Guid changedById,
        string? comment,
        DateTime utcNow)
        => new(Guid.NewGuid())
        {
            ServiceRequestId = serviceRequestId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedById = changedById,
            ChangedOnUtc = utcNow,
            Comment = comment,
        };
}
