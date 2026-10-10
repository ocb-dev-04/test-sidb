using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests.Events;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Dominio.ServiceRequests;

public sealed class ServiceRequest : Entity<Guid>
{
    public const int TITLE_MAXIMUM_LENGTH = 150;
    public const int DESCRIPTION_MAXIMUM_LENGTH = 4000;
    public const int EVIDENCE_REFERENCE_MAXIMUM_LENGTH = 500;
    public const int TRANSITION_COMMENT_MAXIMUM_LENGTH = 1000;

    private readonly List<StatusHistory> _statusHistory = [];
    private readonly List<Comment> _comments = [];

    private ServiceRequest()
    {
    }

    private ServiceRequest(Guid id)
        : base(id)
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? EvidenceReference { get; private set; }

    public ServiceRequestPriority Priority { get; private set; }

    public ServiceRequestStatus Status { get; private set; }

    public int RequestTypeId { get; private set; }

    public RequestType RequestType { get; private set; } = null!;

    public int AreaId { get; private set; }

    public Area Area { get; private set; } = null!;

    public Guid RequesterId { get; private set; }

    public User Requester { get; private set; } = null!;

    public Guid? AssigneeId { get; private set; }

    public User? Assignee { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime DueDateUtc { get; private set; }

    public DateTime? UpdatedOnUtc { get; private set; }

    public DateTime? ResolvedOnUtc { get; private set; }

    public DateTime? ClosedOnUtc { get; private set; }

    public string? ResolutionComment { get; private set; }

    public IReadOnlyCollection<StatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    public static ServiceRequest Create(
        string code,
        string title,
        string description,
        ServiceRequestPriority priority,
        int requestTypeId,
        int areaId,
        Guid requesterId,
        string? evidenceReference,
        DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        ServiceRequest serviceRequest = new(Guid.NewGuid())
        {
            Code = code,
            Title = title.Trim(),
            Description = description.Trim(),
            EvidenceReference = NormalizeOptional(evidenceReference),
            Priority = priority,
            Status = ServiceRequestStatus.Registered,
            RequestTypeId = requestTypeId,
            AreaId = areaId,
            RequesterId = requesterId,
            CreatedOnUtc = utcNow,
            DueDateUtc = utcNow + PriorityServiceLevels.ResolutionTimeFor(priority),
        };

        serviceRequest._statusHistory.Add(
            ServiceRequests.StatusHistory.Create(serviceRequest.Id, null, ServiceRequestStatus.Registered, requesterId, null, utcNow));
        serviceRequest.RaiseDomainEvent(new ServiceRequestCreatedDomainEvent(serviceRequest.Id, requesterId, utcNow));

        return serviceRequest;
    }

    public bool IsOverdue(DateTime utcNow) => StatusTransitions.IsOpen(Status) && DueDateUtc < utcNow;

    public Result Update(
        string? title,
        string? description,
        ServiceRequestPriority? priority,
        int? requestTypeId,
        int? areaId,
        string? evidenceReference,
        DateTime utcNow)
    {
        if (Status == ServiceRequestStatus.Closed)
        {
            return Result.Failure(ServiceRequestErrors.CLOSED_CANNOT_BE_MODIFIED);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            Description = description.Trim();
        }

        if (priority is { } newPriority && newPriority != Priority)
        {
            Priority = newPriority;
            DueDateUtc = CreatedOnUtc + PriorityServiceLevels.ResolutionTimeFor(newPriority);
        }

        RequestTypeId = requestTypeId ?? RequestTypeId;
        AreaId = areaId ?? AreaId;

        if (evidenceReference is not null)
        {
            EvidenceReference = NormalizeOptional(evidenceReference);
        }

        UpdatedOnUtc = utcNow;
        return Result.Success();
    }

    public Result Assign(Guid assigneeId, Guid assignedById, DateTime utcNow)
    {
        if (Status == ServiceRequestStatus.Closed)
        {
            return Result.Failure(ServiceRequestErrors.CLOSED_CANNOT_BE_MODIFIED);
        }

        if (AssigneeId == assigneeId)
        {
            return Result.Failure(ServiceRequestErrors.ALREADY_ASSIGNED);
        }

        Guid? previousAssigneeId = AssigneeId;
        AssigneeId = assigneeId;
        UpdatedOnUtc = utcNow;

        RaiseDomainEvent(new ServiceRequestAssignedDomainEvent(Id, assigneeId, previousAssigneeId, assignedById, utcNow));
        return Result.Success();
    }

    public Result ChangeStatus(
        ServiceRequestStatus newStatus,
        Guid changedById,
        UserRole changedByRole,
        string? comment,
        DateTime utcNow)
    {
        if (newStatus == Status)
        {
            return Result.Failure(ServiceRequestErrors.SAME_STATUS);
        }

        if (!StatusTransitions.IsAllowed(Status, newStatus))
        {
            return Result.Failure(ServiceRequestErrors.InvalidTransition(Status, newStatus));
        }

        if (StatusTransitions.IsReopening(Status) && changedByRole == UserRole.Requester)
        {
            return Result.Failure(ServiceRequestErrors.REOPEN_NOT_ALLOWED);
        }

        string? normalizedComment = NormalizeOptional(comment);
        if (normalizedComment is null && StatusTransitions.RequiresComment(Status, newStatus))
        {
            return Result.Failure(ServiceRequestErrors.COMMENT_REQUIRED);
        }

        ServiceRequestStatus previousStatus = Status;
        ApplyStatusTimestamps(previousStatus, newStatus, normalizedComment, utcNow);
        Status = newStatus;
        UpdatedOnUtc = utcNow;

        _statusHistory.Add(ServiceRequests.StatusHistory.Create(Id, previousStatus, newStatus, changedById, normalizedComment, utcNow));
        RaiseDomainEvent(new ServiceRequestStatusChangedDomainEvent(Id, previousStatus, newStatus, changedById, utcNow));
        return Result.Success();
    }

    public Comment AddComment(Guid authorId, string text, CommentVisibility visibility, DateTime utcNow)
    {
        Comment comment = Comment.Create(Id, authorId, text, visibility, utcNow);
        _comments.Add(comment);
        UpdatedOnUtc = utcNow;

        RaiseDomainEvent(new ServiceRequestCommentAddedDomainEvent(Id, comment.Id, authorId, visibility, utcNow));
        return comment;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void ApplyStatusTimestamps(
        ServiceRequestStatus previousStatus,
        ServiceRequestStatus newStatus,
        string? comment,
        DateTime utcNow)
    {
        if (StatusTransitions.IsReopening(previousStatus))
        {
            ClosedOnUtc = null;
            ResolvedOnUtc = null;
            ResolutionComment = null;
        }

        if (newStatus == ServiceRequestStatus.Resolved)
        {
            ResolvedOnUtc = utcNow;
            ResolutionComment = comment;
        }

        if (newStatus == ServiceRequestStatus.Closed)
        {
            ClosedOnUtc = utcNow;
            ResolvedOnUtc ??= utcNow;
            ResolutionComment = comment;
        }

        if (previousStatus == ServiceRequestStatus.Resolved && newStatus == ServiceRequestStatus.InProgress)
        {
            ResolvedOnUtc = null;
            ResolutionComment = null;
        }
    }
}
