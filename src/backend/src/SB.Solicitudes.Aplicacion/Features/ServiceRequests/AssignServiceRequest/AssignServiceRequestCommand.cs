using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.Solicitudes.Aplicacion.Abstractions;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests.AssignServiceRequest;

public sealed record AssignServiceRequestCommand(Guid ServiceRequestId, Guid AssigneeId) : ICommand<Unit>;

internal sealed class AssignServiceRequestCommandValidator : AbstractValidator<AssignServiceRequestCommand>
{
    public AssignServiceRequestCommandValidator()
        => RuleFor(command => command.AssigneeId).NotEmpty();
}

internal sealed class AssignServiceRequestCommandHandler(
    IServiceRequestRepository serviceRequestRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<AssignServiceRequestCommandHandler> logger)
    : ICommandHandler<AssignServiceRequestCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(AssignServiceRequestCommand request, CancellationToken cancellationToken)
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

        User? assignee = await userRepository.GetByIdAsync(request.AssigneeId, cancellationToken);
        if (assignee is null || !assignee.IsActive || !assignee.CanManageRequests)
        {
            return ServiceRequestErrors.INVALID_ASSIGNEE;
        }

        Result assignResult = serviceRequest.Assign(assignee.Id, currentUser.UserId, timeProvider.GetUtcNow().UtcDateTime);
        if (assignResult.IsFailure)
        {
            return assignResult.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.ServiceRequestAssigned(logger, serviceRequest.Id, assignee.Id, currentUser.UserId);
        return Unit.VALUE;
    }
}
