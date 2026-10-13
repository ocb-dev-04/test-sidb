using FluentValidation;
using MediatR;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.CreateServiceRequest;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests.UpdateServiceRequest;

public sealed record UpdateServiceRequestCommand(
    Guid ServiceRequestId,
    byte[] Version,
    string? Title,
    string? Description,
    ServiceRequestPriority? Priority,
    int? RequestTypeId,
    int? AreaId,
    string? EvidenceReference) : ICommand<Unit>;

internal sealed class UpdateServiceRequestCommandValidator : AbstractValidator<UpdateServiceRequestCommand>
{
    public UpdateServiceRequestCommandValidator()
    {
        RuleFor(command => command.Version).NotEmpty();
        RuleFor(command => command.Title)
            .Must(title => title is null || !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title cannot be blank.")
            .MaximumLength(ServiceRequest.TITLE_MAXIMUM_LENGTH);
        RuleFor(command => command.Description)
            .Must(description => description is null || !string.IsNullOrWhiteSpace(description))
            .WithMessage("Description cannot be blank.")
            .MaximumLength(ServiceRequest.DESCRIPTION_MAXIMUM_LENGTH);
        RuleFor(command => command.Priority).IsInEnum();
        RuleFor(command => command.RequestTypeId).GreaterThan(0);
        RuleFor(command => command.AreaId).GreaterThan(0);
        RuleFor(command => command.EvidenceReference).MaximumLength(ServiceRequest.EVIDENCE_REFERENCE_MAXIMUM_LENGTH);
    }
}

internal sealed class UpdateServiceRequestCommandHandler(
    IServiceRequestRepository serviceRequestRepository,
    IAreaRepository areaRepository,
    IRequestTypeRepository requestTypeRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateServiceRequestCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(UpdateServiceRequestCommand request, CancellationToken cancellationToken)
    {
        ServiceRequest? serviceRequest = await serviceRequestRepository.GetByIdAsync(request.ServiceRequestId, cancellationToken);
        ServiceRequestVisibility visibility = ServiceRequestVisibility.For(currentUser);
        if (serviceRequest is null || !visibility.CanView(serviceRequest))
        {
            return ServiceRequestErrors.NOT_FOUND;
        }

        // Regla de negocio: el solicitante solo corrige su solicitud antes de que alguien la analice.
        bool isRequesterEditingOwnRegistered = currentUser.Role == UserRole.Requester
            && serviceRequest.RequesterId == currentUser.UserId
            && serviceRequest.Status == ServiceRequestStatus.Registered;
        if (!visibility.CanManage && !isRequesterEditingOwnRegistered)
        {
            return ServiceRequestErrors.EDIT_NOT_ALLOWED;
        }

        if (!serviceRequestRepository.HasVersion(serviceRequest, request.Version))
        {
            return ServiceRequestErrors.CONCURRENCY_CONFLICT;
        }

        Result catalogsResult = await CatalogGuards.EnsureActiveAsync(
            request.AreaId, request.RequestTypeId, areaRepository, requestTypeRepository, cancellationToken);
        if (catalogsResult.IsFailure)
        {
            return catalogsResult.Error;
        }

        Result updateResult = serviceRequest.Update(
            request.Title,
            request.Description,
            request.Priority,
            request.RequestTypeId,
            request.AreaId,
            request.EvidenceReference,
            timeProvider.GetUtcNow().UtcDateTime);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
