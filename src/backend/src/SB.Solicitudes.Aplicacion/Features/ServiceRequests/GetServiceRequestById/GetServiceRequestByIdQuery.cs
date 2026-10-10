using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests.GetServiceRequestById;

public sealed record GetServiceRequestByIdQuery(Guid ServiceRequestId) : IQuery<ServiceRequestDetailResponse>;

internal sealed class GetServiceRequestByIdQueryHandler(
    IServiceRequestReadRepository readRepository,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IQueryHandler<GetServiceRequestByIdQuery, ServiceRequestDetailResponse>
{
    public async Task<Result<ServiceRequestDetailResponse>> HandleAsync(
        GetServiceRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        ServiceRequestVisibility visibility = ServiceRequestVisibility.For(currentUser);
        ServiceRequestDetailResponse? detail = await readRepository.GetDetailAsync(
            request.ServiceRequestId,
            visibility,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        if (detail is null)
        {
            return ServiceRequestErrors.NOT_FOUND;
        }

        IReadOnlyList<ServiceRequestStatus> allowedTransitions = visibility.CanManage
            ? [.. StatusTransitions.AllowedFrom(detail.Status)]
            : [];

        return detail with { AllowedTransitions = allowedTransitions };
    }
}
