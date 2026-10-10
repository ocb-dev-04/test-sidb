using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.Dashboard;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.GetServiceRequests;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests;

public interface IServiceRequestReadRepository
{
    Task<PagedResult<ServiceRequestSummaryResponse>> GetPagedAsync(
        GetServiceRequestsQuery query,
        ServiceRequestVisibility visibility,
        DateTime utcNow,
        CancellationToken cancellationToken);

    Task<ServiceRequestDetailResponse?> GetDetailAsync(
        Guid id,
        ServiceRequestVisibility visibility,
        DateTime utcNow,
        CancellationToken cancellationToken);

    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
        ServiceRequestVisibility visibility,
        DateTime utcNow,
        int latestCount,
        CancellationToken cancellationToken);
}
