using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.ServiceRequests.Events;

public sealed record ServiceRequestCreatedDomainEvent(
    Guid ServiceRequestId,
    Guid RequesterId,
    DateTime OccurredOnUtc) : IDomainEvent;

public sealed record ServiceRequestAssignedDomainEvent(
    Guid ServiceRequestId,
    Guid AssigneeId,
    Guid? PreviousAssigneeId,
    Guid AssignedById,
    DateTime OccurredOnUtc) : IDomainEvent;

public sealed record ServiceRequestStatusChangedDomainEvent(
    Guid ServiceRequestId,
    ServiceRequestStatus PreviousStatus,
    ServiceRequestStatus NewStatus,
    Guid ChangedById,
    DateTime OccurredOnUtc) : IDomainEvent;

public sealed record ServiceRequestCommentAddedDomainEvent(
    Guid ServiceRequestId,
    Guid CommentId,
    Guid AuthorId,
    CommentVisibility Visibility,
    DateTime OccurredOnUtc) : IDomainEvent;
