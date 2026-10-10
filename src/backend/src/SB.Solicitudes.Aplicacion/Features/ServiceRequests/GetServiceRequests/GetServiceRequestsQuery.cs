using FluentValidation;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests.GetServiceRequests;

/// <summary>Filtros, orden y página del listado de solicitudes.</summary>
public sealed record GetServiceRequestsQuery : IQuery<PagedResult<ServiceRequestSummaryResponse>>
{
    public const int SEARCH_MAXIMUM_LENGTH = 100;

    /// <summary>Busca por código o título.</summary>
    public string? Search { get; init; }

    public ServiceRequestStatus? Status { get; init; }

    public ServiceRequestPriority? Priority { get; init; }

    public int? AreaId { get; init; }

    public int? RequestTypeId { get; init; }

    public Guid? RequesterId { get; init; }

    public Guid? AssigneeId { get; init; }

    /// <summary>Fecha de creación desde (inclusive).</summary>
    public DateOnly? CreatedFrom { get; init; }

    /// <summary>Fecha de creación hasta (inclusive).</summary>
    public DateOnly? CreatedTo { get; init; }

    public bool? OnlyOverdue { get; init; }

    public ServiceRequestSortField SortBy { get; init; } = ServiceRequestSortField.CreatedOn;

    public SortDirection SortDirection { get; init; } = SortDirection.Descending;

    public int Page { get; init; } = Pagination.DEFAULT_PAGE;

    public int PageSize { get; init; } = Pagination.DEFAULT_PAGE_SIZE;
}

internal sealed class GetServiceRequestsQueryValidator : AbstractValidator<GetServiceRequestsQuery>
{
    public GetServiceRequestsQueryValidator()
    {
        this.ValidatePagination(query => query.Page, query => query.PageSize);
        RuleFor(query => query.Search).MaximumLength(GetServiceRequestsQuery.SEARCH_MAXIMUM_LENGTH);
        RuleFor(query => query.Status).IsInEnum();
        RuleFor(query => query.Priority).IsInEnum();
        RuleFor(query => query.SortBy).IsInEnum();
        RuleFor(query => query.SortDirection).IsInEnum();
        RuleFor(query => query.CreatedTo)
            .GreaterThanOrEqualTo(query => query.CreatedFrom)
            .When(query => query.CreatedFrom.HasValue && query.CreatedTo.HasValue);
    }
}

internal sealed class GetServiceRequestsQueryHandler(
    IServiceRequestReadRepository readRepository,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IQueryHandler<GetServiceRequestsQuery, PagedResult<ServiceRequestSummaryResponse>>
{
    public async Task<Result<PagedResult<ServiceRequestSummaryResponse>>> HandleAsync(
        GetServiceRequestsQuery request,
        CancellationToken cancellationToken)
        => await readRepository.GetPagedAsync(
            request,
            ServiceRequestVisibility.For(currentUser),
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);
}
