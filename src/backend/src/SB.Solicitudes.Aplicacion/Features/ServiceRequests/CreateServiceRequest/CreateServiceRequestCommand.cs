using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.Solicitudes.Aplicacion.Abstractions;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests.CreateServiceRequest;

/// <summary>Datos para registrar una nueva solicitud.</summary>
public sealed record CreateServiceRequestCommand(
    string Title,
    string Description,
    ServiceRequestPriority Priority,
    int RequestTypeId,
    int AreaId,
    string? EvidenceReference) : ICommand<ServiceRequestCreatedResponse>;

internal sealed class CreateServiceRequestCommandValidator : AbstractValidator<CreateServiceRequestCommand>
{
    public CreateServiceRequestCommandValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(ServiceRequest.TITLE_MAXIMUM_LENGTH);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(ServiceRequest.DESCRIPTION_MAXIMUM_LENGTH);
        RuleFor(command => command.Priority).IsInEnum();
        RuleFor(command => command.RequestTypeId).GreaterThan(0);
        RuleFor(command => command.AreaId).GreaterThan(0);
        RuleFor(command => command.EvidenceReference).MaximumLength(ServiceRequest.EVIDENCE_REFERENCE_MAXIMUM_LENGTH);
    }
}

internal sealed class CreateServiceRequestCommandHandler(
    IServiceRequestRepository serviceRequestRepository,
    IAreaRepository areaRepository,
    IRequestTypeRepository requestTypeRepository,
    IServiceRequestCodeGenerator codeGenerator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<CreateServiceRequestCommandHandler> logger)
    : ICommandHandler<CreateServiceRequestCommand, ServiceRequestCreatedResponse>
{
    public async Task<Result<ServiceRequestCreatedResponse>> HandleAsync(
        CreateServiceRequestCommand request,
        CancellationToken cancellationToken)
    {
        Result catalogsResult = await CatalogGuards.EnsureActiveAsync(
            request.AreaId, request.RequestTypeId, areaRepository, requestTypeRepository, cancellationToken);
        if (catalogsResult.IsFailure)
        {
            return catalogsResult.Error;
        }

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;
        string code = await codeGenerator.GenerateAsync(utcNow.Year, cancellationToken);

        ServiceRequest serviceRequest = ServiceRequest.Create(
            code,
            request.Title,
            request.Description,
            request.Priority,
            request.RequestTypeId,
            request.AreaId,
            currentUser.UserId,
            request.EvidenceReference,
            utcNow);

        serviceRequestRepository.Add(serviceRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.ServiceRequestCreated(logger, serviceRequest.Code, currentUser.UserId);
        return new ServiceRequestCreatedResponse(serviceRequest.Id, serviceRequest.Code);
    }
}

internal static class CatalogGuards
{
    public static async Task<Result> EnsureActiveAsync(
        int? areaId,
        int? requestTypeId,
        IAreaRepository areaRepository,
        IRequestTypeRepository requestTypeRepository,
        CancellationToken cancellationToken)
    {
        if (areaId is { } requestedAreaId)
        {
            Area? area = await areaRepository.GetByIdAsync(requestedAreaId, cancellationToken);
            if (area is null)
            {
                return Result.Failure(CatalogErrors.AREA_NOT_FOUND);
            }

            if (!area.IsActive)
            {
                return Result.Failure(CatalogErrors.AREA_INACTIVE);
            }
        }

        if (requestTypeId is { } requestedTypeId)
        {
            RequestType? requestType = await requestTypeRepository.GetByIdAsync(requestedTypeId, cancellationToken);
            if (requestType is null)
            {
                return Result.Failure(CatalogErrors.REQUEST_TYPE_NOT_FOUND);
            }

            if (!requestType.IsActive)
            {
                return Result.Failure(CatalogErrors.REQUEST_TYPE_INACTIVE);
            }
        }

        return Result.Success();
    }
}
