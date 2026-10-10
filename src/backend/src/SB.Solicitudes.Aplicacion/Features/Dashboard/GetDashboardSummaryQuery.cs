using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Aplicacion.Features.Dashboard;

public sealed record GetDashboardSummaryQuery : IQuery<DashboardSummaryResponse>
{
    public const int LATEST_REQUESTS_COUNT = 5;
}

public sealed record DashboardSummaryResponse(
    int TotalCount,
    int OpenCount,
    int ClosedCount,
    int OverdueCount,
    IReadOnlyList<StatusCountResponse> ByStatus,
    IReadOnlyList<PriorityCountResponse> ByPriority,
    IReadOnlyList<ServiceRequestSummaryResponse> LatestRequests);

public sealed record StatusCountResponse(ServiceRequestStatus Status, int Count);

public sealed record PriorityCountResponse(ServiceRequestPriority Priority, int Count);

internal sealed class GetDashboardSummaryQueryHandler(
    IServiceRequestReadRepository readRepository,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    public async Task<Result<DashboardSummaryResponse>> HandleAsync(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        => await readRepository.GetDashboardSummaryAsync(
            ServiceRequestVisibility.For(currentUser),
            timeProvider.GetUtcNow().UtcDateTime,
            GetDashboardSummaryQuery.LATEST_REQUESTS_COUNT,
            cancellationToken);
}
