using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.Dashboard;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.GetServiceRequests;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Infraestructura.Persistence.Configurations;

namespace SB.Solicitudes.Infraestructura.Persistence.Repositories;

internal sealed class ServiceRequestReadRepository(ApplicationDbContext context) : IServiceRequestReadRepository
{
    private const int FIRST_RANK = 1;
    private const int SECOND_RANK = 2;
    private const int THIRD_RANK = 3;
    private const int FOURTH_RANK = 4;
    private const int FIFTH_RANK = 5;
    private const int SIXTH_RANK = 6;

    // Los enums se guardan como texto; estos CASE ordenan por significado de negocio y no alfabéticamente.
    private static readonly Expression<Func<ServiceRequest, int>> PRIORITY_RANK = serviceRequest =>
        serviceRequest.Priority == ServiceRequestPriority.Low ? FIRST_RANK
        : serviceRequest.Priority == ServiceRequestPriority.Medium ? SECOND_RANK
        : serviceRequest.Priority == ServiceRequestPriority.High ? THIRD_RANK
        : FOURTH_RANK;

    private static readonly Expression<Func<ServiceRequest, int>> STATUS_RANK = serviceRequest =>
        serviceRequest.Status == ServiceRequestStatus.Registered ? FIRST_RANK
        : serviceRequest.Status == ServiceRequestStatus.InAnalysis ? SECOND_RANK
        : serviceRequest.Status == ServiceRequestStatus.InProgress ? THIRD_RANK
        : serviceRequest.Status == ServiceRequestStatus.WaitingForRequester ? FOURTH_RANK
        : serviceRequest.Status == ServiceRequestStatus.Resolved ? FIFTH_RANK
        : SIXTH_RANK;

    public async Task<PagedResult<ServiceRequestSummaryResponse>> GetPagedAsync(
        GetServiceRequestsQuery query,
        ServiceRequestVisibility visibility,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        IQueryable<ServiceRequest> filtered = ApplyFilters(Visible(visibility), query, utcNow);

        int totalCount = await filtered.CountAsync(cancellationToken);
        List<ServiceRequestSummaryResponse> items = await ApplySorting(filtered, query.SortBy, query.SortDirection)
            .Skip(Pagination.Skip(query.Page, query.PageSize))
            .Take(query.PageSize)
            .Select(ToSummary(utcNow))
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceRequestSummaryResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<ServiceRequestDetailResponse?> GetDetailAsync(
        Guid id,
        ServiceRequestVisibility visibility,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        bool canSeeInternalComments = visibility.CanSeeInternalComments;

        return await Visible(visibility)
            .Where(serviceRequest => serviceRequest.Id == id)
            .AsSplitQuery()
            .Select(serviceRequest => new ServiceRequestDetailResponse(
                serviceRequest.Id,
                serviceRequest.Code,
                serviceRequest.Title,
                serviceRequest.Description,
                serviceRequest.EvidenceReference,
                serviceRequest.Priority,
                serviceRequest.Status,
                serviceRequest.AreaId,
                serviceRequest.Area.Name,
                serviceRequest.RequestTypeId,
                serviceRequest.RequestType.Name,
                serviceRequest.RequesterId,
                serviceRequest.Requester.FullName,
                serviceRequest.AssigneeId,
                serviceRequest.Assignee != null ? serviceRequest.Assignee.FullName : null,
                serviceRequest.CreatedOnUtc,
                serviceRequest.DueDateUtc,
                serviceRequest.UpdatedOnUtc,
                serviceRequest.ResolvedOnUtc,
                serviceRequest.ClosedOnUtc,
                serviceRequest.ResolutionComment,
                serviceRequest.DueDateUtc < utcNow
                    && serviceRequest.Status != ServiceRequestStatus.Resolved
                    && serviceRequest.Status != ServiceRequestStatus.Closed,
                serviceRequest.StatusHistory
                    .OrderBy(history => history.ChangedOnUtc)
                    .Select(history => new StatusHistoryResponse(
                        history.Id,
                        history.PreviousStatus,
                        history.NewStatus,
                        history.ChangedById,
                        history.ChangedBy.FullName,
                        history.ChangedOnUtc,
                        history.Comment))
                    .ToList(),
                serviceRequest.Comments
                    .Where(comment => canSeeInternalComments || comment.Visibility == CommentVisibility.Public)
                    .OrderBy(comment => comment.CreatedOnUtc)
                    .Select(comment => new CommentResponse(
                        comment.Id,
                        comment.Text,
                        comment.Visibility,
                        comment.AuthorId,
                        comment.Author.FullName,
                        comment.CreatedOnUtc))
                    .ToList(),
                new List<ServiceRequestStatus>(),
                EF.Property<byte[]>(serviceRequest, ServiceRequestConfiguration.ROW_VERSION_PROPERTY)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
        ServiceRequestVisibility visibility,
        DateTime utcNow,
        int latestCount,
        CancellationToken cancellationToken)
    {
        IQueryable<ServiceRequest> visible = Visible(visibility);

        Dictionary<ServiceRequestStatus, int> byStatus = await visible
            .GroupBy(serviceRequest => serviceRequest.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);

        Dictionary<ServiceRequestPriority, int> byPriority = await visible
            .GroupBy(serviceRequest => serviceRequest.Priority)
            .Select(group => new { Priority = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Priority, item => item.Count, cancellationToken);

        int overdueCount = await visible.CountAsync(IsOverdue(utcNow), cancellationToken);

        List<ServiceRequestSummaryResponse> latest = await visible
            .OrderByDescending(serviceRequest => serviceRequest.CreatedOnUtc)
            .Take(latestCount)
            .Select(ToSummary(utcNow))
            .ToListAsync(cancellationToken);

        StatusCountResponse[] statusCounts = Enum.GetValues<ServiceRequestStatus>()
            .Select(status => new StatusCountResponse(status, byStatus.GetValueOrDefault(status)))
            .ToArray();
        PriorityCountResponse[] priorityCounts = Enum.GetValues<ServiceRequestPriority>()
            .Select(priority => new PriorityCountResponse(priority, byPriority.GetValueOrDefault(priority)))
            .ToArray();

        return new DashboardSummaryResponse(
            statusCounts.Sum(item => item.Count),
            statusCounts.Where(item => StatusTransitions.IsOpen(item.Status)).Sum(item => item.Count),
            byStatus.GetValueOrDefault(ServiceRequestStatus.Closed),
            overdueCount,
            statusCounts,
            priorityCounts,
            latest);
    }

    private static Expression<Func<ServiceRequest, bool>> IsOverdue(DateTime utcNow)
        => serviceRequest => serviceRequest.DueDateUtc < utcNow
            && serviceRequest.Status != ServiceRequestStatus.Resolved
            && serviceRequest.Status != ServiceRequestStatus.Closed;

    private static Expression<Func<ServiceRequest, ServiceRequestSummaryResponse>> ToSummary(DateTime utcNow)
        => serviceRequest => new ServiceRequestSummaryResponse(
            serviceRequest.Id,
            serviceRequest.Code,
            serviceRequest.Title,
            serviceRequest.Priority,
            serviceRequest.Status,
            serviceRequest.AreaId,
            serviceRequest.Area.Name,
            serviceRequest.RequestTypeId,
            serviceRequest.RequestType.Name,
            serviceRequest.RequesterId,
            serviceRequest.Requester.FullName,
            serviceRequest.AssigneeId,
            serviceRequest.Assignee != null ? serviceRequest.Assignee.FullName : null,
            serviceRequest.CreatedOnUtc,
            serviceRequest.DueDateUtc,
            serviceRequest.DueDateUtc < utcNow
                && serviceRequest.Status != ServiceRequestStatus.Resolved
                && serviceRequest.Status != ServiceRequestStatus.Closed);

    private static IQueryable<ServiceRequest> ApplyFilters(IQueryable<ServiceRequest> source, GetServiceRequestsQuery query, DateTime utcNow)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            source = source.Where(serviceRequest => serviceRequest.Code.Contains(term) || serviceRequest.Title.Contains(term));
        }

        if (query.Status is { } status)
        {
            source = source.Where(serviceRequest => serviceRequest.Status == status);
        }

        if (query.Priority is { } priority)
        {
            source = source.Where(serviceRequest => serviceRequest.Priority == priority);
        }

        if (query.AreaId is { } areaId)
        {
            source = source.Where(serviceRequest => serviceRequest.AreaId == areaId);
        }

        if (query.RequestTypeId is { } requestTypeId)
        {
            source = source.Where(serviceRequest => serviceRequest.RequestTypeId == requestTypeId);
        }

        if (query.RequesterId is { } requesterId)
        {
            source = source.Where(serviceRequest => serviceRequest.RequesterId == requesterId);
        }

        if (query.AssigneeId is { } assigneeId)
        {
            source = source.Where(serviceRequest => serviceRequest.AssigneeId == assigneeId);
        }

        if (query.CreatedFrom is { } createdFrom)
        {
            DateTime fromUtc = createdFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(serviceRequest => serviceRequest.CreatedOnUtc >= fromUtc);
        }

        if (query.CreatedTo is { } createdTo)
        {
            DateTime toExclusiveUtc = createdTo.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(serviceRequest => serviceRequest.CreatedOnUtc < toExclusiveUtc);
        }

        if (query.OnlyOverdue == true)
        {
            source = source.Where(IsOverdue(utcNow));
        }

        return source;
    }

    private static IQueryable<ServiceRequest> ApplySorting(
        IQueryable<ServiceRequest> source,
        ServiceRequestSortField sortBy,
        SortDirection direction)
    {
        IOrderedQueryable<ServiceRequest> ordered = sortBy switch
        {
            ServiceRequestSortField.DueDate => OrderBy(source, serviceRequest => serviceRequest.DueDateUtc, direction),
            ServiceRequestSortField.Code => OrderBy(source, serviceRequest => serviceRequest.Code, direction),
            ServiceRequestSortField.Status => OrderBy(source, STATUS_RANK, direction),
            ServiceRequestSortField.Priority => OrderBy(source, PRIORITY_RANK, direction),
            _ => OrderBy(source, serviceRequest => serviceRequest.CreatedOnUtc, direction),
        };

        return ordered.ThenBy(serviceRequest => serviceRequest.Id);
    }

    private static IOrderedQueryable<ServiceRequest> OrderBy<TKey>(
        IQueryable<ServiceRequest> source,
        Expression<Func<ServiceRequest, TKey>> key,
        SortDirection direction)
        => direction == SortDirection.Ascending ? source.OrderBy(key) : source.OrderByDescending(key);

    private IQueryable<ServiceRequest> Visible(ServiceRequestVisibility visibility)
        => context.ServiceRequests.AsNoTracking().Where(visibility.ToExpression());
}
