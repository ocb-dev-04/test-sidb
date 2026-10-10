using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.Solicitudes.Aplicacion.Abstractions;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests.ChangeServiceRequestStatus;

public sealed record ChangeServiceRequestStatusCommand(
    Guid ServiceRequestId,
    ServiceRequestStatus NewStatus,
    string? Comment) : ICommand<Unit>;

internal sealed class ChangeServiceRequestStatusCommandValidator : AbstractValidator<ChangeServiceRequestStatusCommand>
{
    public ChangeServiceRequestStatusCommandValidator()
    {
        RuleFor(command => command.NewStatus).IsInEnum();
        RuleFor(command => command.Comment).MaximumLength(ServiceRequest.TRANSITION_COMMENT_MAXIMUM_LENGTH);
    }
}

internal sealed class ChangeServiceRequestStatusCommandHandler(
    IServiceRequestRepository serviceRequestRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<ChangeServiceRequestStatusCommandHandler> logger)
    : ICommandHandler<ChangeServiceRequestStatusCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(ChangeServiceRequestStatusCommand request, CancellationToken cancellationToken)
    {
        ServiceRequestVisibility visibility = ServiceRequestVisibility.For(currentUser);
        if (!visibility.CanManage)
        {
            return ServiceRequestErrors.MANAGEMENT_REQUIRED;
        }

        ServiceRequest? serviceRequest = await serviceRequestRepository.GetByIdAsync(request.ServiceRequestId, cancellationToken);
        if (serviceRequest is null || !visibility.CanView(serviceRequest))
        {
            return ServiceRequestErrors.NOT_FOUND;
        }

        Result changeResult = serviceRequest.ChangeStatus(
            request.NewStatus,
            currentUser.UserId,
            currentUser.Role,
            request.Comment,
            timeProvider.GetUtcNow().UtcDateTime);
        if (changeResult.IsFailure)
        {
            return changeResult.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.ServiceRequestStatusChanged(logger, serviceRequest.Id, request.NewStatus, currentUser.UserId);
        return Unit.VALUE;
    }
}
