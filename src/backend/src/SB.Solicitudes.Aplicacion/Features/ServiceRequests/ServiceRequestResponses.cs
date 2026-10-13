using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests;

public sealed record ServiceRequestSummaryResponse(
    Guid Id,
    string Code,
    string Title,
    ServiceRequestPriority Priority,
    ServiceRequestStatus Status,
    int AreaId,
    string AreaName,
    int RequestTypeId,
    string RequestTypeName,
    Guid RequesterId,
    string RequesterName,
    Guid? AssigneeId,
    string? AssigneeName,
    DateTime CreatedOnUtc,
    DateTime DueDateUtc,
    bool IsOverdue);

public sealed record ServiceRequestDetailResponse(
    Guid Id,
    string Code,
    string Title,
    string Description,
    string? EvidenceReference,
    ServiceRequestPriority Priority,
    ServiceRequestStatus Status,
    int AreaId,
    string AreaName,
    int RequestTypeId,
    string RequestTypeName,
    Guid RequesterId,
    string RequesterName,
    Guid? AssigneeId,
    string? AssigneeName,
    DateTime CreatedOnUtc,
    DateTime DueDateUtc,
    DateTime? UpdatedOnUtc,
    DateTime? ResolvedOnUtc,
    DateTime? ClosedOnUtc,
    string? ResolutionComment,
    bool IsOverdue,
    IReadOnlyList<StatusHistoryResponse> StatusHistory,
    IReadOnlyList<CommentResponse> Comments,
    IReadOnlyList<ServiceRequestStatus> AllowedTransitions,
    byte[] Version);

public sealed record StatusHistoryResponse(
    Guid Id,
    ServiceRequestStatus? PreviousStatus,
    ServiceRequestStatus NewStatus,
    Guid ChangedById,
    string ChangedByName,
    DateTime ChangedOnUtc,
    string? Comment);

public sealed record CommentResponse(
    Guid Id,
    string Text,
    CommentVisibility Visibility,
    Guid AuthorId,
    string AuthorName,
    DateTime CreatedOnUtc);

public sealed record ServiceRequestCreatedResponse(Guid Id, string Code);

public enum ServiceRequestSortField
{
    CreatedOn = 1,
    DueDate = 2,
    Priority = 3,
    Status = 4,
    Code = 5,
}
