using FluentValidation;
using MediatR;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.Catalogs;

public sealed record AreaResponse(int Id, string Name, bool IsActive)
{
    public static AreaResponse From(Area area) => new(area.Id, area.Name, area.IsActive);
}

public sealed record RequestTypeResponse(int Id, string Name, string Description, bool IsActive)
{
    public static RequestTypeResponse From(RequestType requestType)
        => new(requestType.Id, requestType.Name, requestType.Description, requestType.IsActive);
}

/// <summary>Solo los administradores pueden incluir registros inactivos.</summary>
public sealed record GetAreasQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<AreaResponse>>;

public sealed record CreateAreaCommand(string Name) : ICommand<AreaResponse>;

public sealed record UpdateAreaCommand(int AreaId, string Name, bool IsActive) : ICommand<Unit>;

public sealed record GetRequestTypesQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<RequestTypeResponse>>;

public sealed record CreateRequestTypeCommand(string Name, string Description) : ICommand<RequestTypeResponse>;

public sealed record UpdateRequestTypeCommand(int RequestTypeId, string Name, string Description, bool IsActive) : ICommand<Unit>;

internal sealed class CreateAreaCommandValidator : AbstractValidator<CreateAreaCommand>
{
    public CreateAreaCommandValidator()
        => RuleFor(command => command.Name).NotEmpty().MaximumLength(Area.NAME_MAXIMUM_LENGTH);
}

internal sealed class UpdateAreaCommandValidator : AbstractValidator<UpdateAreaCommand>
{
    public UpdateAreaCommandValidator()
        => RuleFor(command => command.Name).NotEmpty().MaximumLength(Area.NAME_MAXIMUM_LENGTH);
}

internal sealed class CreateRequestTypeCommandValidator : AbstractValidator<CreateRequestTypeCommand>
{
    public CreateRequestTypeCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(RequestType.NAME_MAXIMUM_LENGTH);
        RuleFor(command => command.Description).NotNull().MaximumLength(RequestType.DESCRIPTION_MAXIMUM_LENGTH);
    }
}

internal sealed class UpdateRequestTypeCommandValidator : AbstractValidator<UpdateRequestTypeCommand>
{
    public UpdateRequestTypeCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(RequestType.NAME_MAXIMUM_LENGTH);
        RuleFor(command => command.Description).NotNull().MaximumLength(RequestType.DESCRIPTION_MAXIMUM_LENGTH);
    }
}

internal sealed class GetAreasQueryHandler(IAreaRepository areaRepository, ICurrentUser currentUser)
    : IQueryHandler<GetAreasQuery, IReadOnlyList<AreaResponse>>
{
    public async Task<Result<IReadOnlyList<AreaResponse>>> HandleAsync(GetAreasQuery request, CancellationToken cancellationToken)
    {
        bool includeInactive = request.IncludeInactive && currentUser.Role == UserRole.Administrator;
        IReadOnlyList<Area> areas = await areaRepository.GetAllAsync(includeInactive, cancellationToken);
        return Result.Success<IReadOnlyList<AreaResponse>>([.. areas.Select(AreaResponse.From)]);
    }
}

internal sealed class CreateAreaCommandHandler(IAreaRepository areaRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAreaCommand, AreaResponse>
{
    public async Task<Result<AreaResponse>> HandleAsync(CreateAreaCommand request, CancellationToken cancellationToken)
    {
        if (await areaRepository.ExistsByNameAsync(request.Name.Trim(), null, cancellationToken))
        {
            return CatalogErrors.AREA_NAME_IN_USE;
        }

        Area area = Area.Create(request.Name);
        areaRepository.Add(area);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AreaResponse.From(area);
    }
}

internal sealed class UpdateAreaCommandHandler(IAreaRepository areaRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAreaCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(UpdateAreaCommand request, CancellationToken cancellationToken)
    {
        Area? area = await areaRepository.GetByIdAsync(request.AreaId, cancellationToken);
        if (area is null)
        {
            return CatalogErrors.AREA_NOT_FOUND;
        }

        if (await areaRepository.ExistsByNameAsync(request.Name.Trim(), area.Id, cancellationToken))
        {
            return CatalogErrors.AREA_NAME_IN_USE;
        }

        area.Update(request.Name, request.IsActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

internal sealed class GetRequestTypesQueryHandler(IRequestTypeRepository requestTypeRepository, ICurrentUser currentUser)
    : IQueryHandler<GetRequestTypesQuery, IReadOnlyList<RequestTypeResponse>>
{
    public async Task<Result<IReadOnlyList<RequestTypeResponse>>> HandleAsync(GetRequestTypesQuery request, CancellationToken cancellationToken)
    {
        bool includeInactive = request.IncludeInactive && currentUser.Role == UserRole.Administrator;
        IReadOnlyList<RequestType> requestTypes = await requestTypeRepository.GetAllAsync(includeInactive, cancellationToken);
        return Result.Success<IReadOnlyList<RequestTypeResponse>>([.. requestTypes.Select(RequestTypeResponse.From)]);
    }
}

internal sealed class CreateRequestTypeCommandHandler(IRequestTypeRepository requestTypeRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRequestTypeCommand, RequestTypeResponse>
{
    public async Task<Result<RequestTypeResponse>> HandleAsync(CreateRequestTypeCommand request, CancellationToken cancellationToken)
    {
        if (await requestTypeRepository.ExistsByNameAsync(request.Name.Trim(), null, cancellationToken))
        {
            return CatalogErrors.REQUEST_TYPE_NAME_IN_USE;
        }

        RequestType requestType = RequestType.Create(request.Name, request.Description);
        requestTypeRepository.Add(requestType);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return RequestTypeResponse.From(requestType);
    }
}

internal sealed class UpdateRequestTypeCommandHandler(IRequestTypeRepository requestTypeRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateRequestTypeCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(UpdateRequestTypeCommand request, CancellationToken cancellationToken)
    {
        RequestType? requestType = await requestTypeRepository.GetByIdAsync(request.RequestTypeId, cancellationToken);
        if (requestType is null)
        {
            return CatalogErrors.REQUEST_TYPE_NOT_FOUND;
        }

        if (await requestTypeRepository.ExistsByNameAsync(request.Name.Trim(), requestType.Id, cancellationToken))
        {
            return CatalogErrors.REQUEST_TYPE_NAME_IN_USE;
        }

        requestType.Update(request.Name, request.Description, request.IsActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
