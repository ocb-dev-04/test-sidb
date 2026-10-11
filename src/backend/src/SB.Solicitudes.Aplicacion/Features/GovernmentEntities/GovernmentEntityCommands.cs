using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Abstractions;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Aplicacion.Features.GovernmentEntities;

public sealed record CreateGovernmentEntityCommand(
    string Name,
    string Category,
    string StateBranch,
    string Sector,
    GovernmentEntityDataSource? Source) : ICommand<GovernmentEntityResponse>;

public sealed record UpdateGovernmentEntityCommand(
    int GovernmentEntityId,
    string Name,
    string Category,
    string StateBranch,
    string Sector,
    GovernmentEntityDataSource? Source) : ICommand<Unit>;

public sealed record DeleteGovernmentEntityCommand(int GovernmentEntityId, GovernmentEntityDataSource? Source) : ICommand<Unit>;

public sealed record ImportGovernmentEntitiesCommand(
    GovernmentEntityDataSource Source,
    GovernmentEntityDataSource Destination) : ICommand<GovernmentEntityImportResponse>;

internal sealed class CreateGovernmentEntityCommandValidator : AbstractValidator<CreateGovernmentEntityCommand>
{
    public CreateGovernmentEntityCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(GovernmentEntity.NAME_MAXIMUM_LENGTH);
        RuleFor(command => command.Category).NotEmpty().MaximumLength(GovernmentEntity.CATEGORY_MAXIMUM_LENGTH);
        RuleFor(command => command.StateBranch).NotEmpty().MaximumLength(GovernmentEntity.STATE_BRANCH_MAXIMUM_LENGTH);
        RuleFor(command => command.Sector).NotEmpty().MaximumLength(GovernmentEntity.SECTOR_MAXIMUM_LENGTH);
        RuleFor(command => command.Source).IsInEnum();
    }
}

internal sealed class UpdateGovernmentEntityCommandValidator : AbstractValidator<UpdateGovernmentEntityCommand>
{
    public UpdateGovernmentEntityCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(GovernmentEntity.NAME_MAXIMUM_LENGTH);
        RuleFor(command => command.Category).NotEmpty().MaximumLength(GovernmentEntity.CATEGORY_MAXIMUM_LENGTH);
        RuleFor(command => command.StateBranch).NotEmpty().MaximumLength(GovernmentEntity.STATE_BRANCH_MAXIMUM_LENGTH);
        RuleFor(command => command.Sector).NotEmpty().MaximumLength(GovernmentEntity.SECTOR_MAXIMUM_LENGTH);
        RuleFor(command => command.Source).IsInEnum();
    }
}

internal sealed class ImportGovernmentEntitiesCommandValidator : AbstractValidator<ImportGovernmentEntitiesCommand>
{
    public ImportGovernmentEntitiesCommandValidator()
    {
        RuleFor(command => command.Source).IsInEnum();
        RuleFor(command => command.Destination).IsInEnum();
    }
}

internal sealed class CreateGovernmentEntityCommandHandler(
    IGovernmentEntityStoreFactory storeFactory,
    IOptions<GovernmentEntitiesOptions> options)
    : ICommandHandler<CreateGovernmentEntityCommand, GovernmentEntityResponse>
{
    public async Task<Result<GovernmentEntityResponse>> HandleAsync(CreateGovernmentEntityCommand request, CancellationToken cancellationToken)
    {
        GovernmentEntityDataSource source = request.Source ?? options.Value.DefaultSource;
        Result<IGovernmentEntityWriter> writer = storeFactory.GetWriter(source);
        if (writer.IsFailure)
        {
            return writer.Error;
        }

        Result<IReadOnlyList<GovernmentEntity>> existing = await storeFactory.ReadAllAsync(source, cancellationToken);
        if (existing.IsFailure)
        {
            return existing.Error;
        }

        if (existing.Value.Any(entity => GovernmentEntitySearch.NameEquals(entity.Name, request.Name)))
        {
            return GovernmentEntityErrors.NAME_IN_USE;
        }

        GovernmentEntity created = await writer.Value.AddAsync(
            GovernmentEntity.Create(request.Name, request.Category, request.StateBranch, request.Sector),
            cancellationToken);

        return GovernmentEntityResponse.From(created);
    }
}

internal sealed class UpdateGovernmentEntityCommandHandler(
    IGovernmentEntityStoreFactory storeFactory,
    IOptions<GovernmentEntitiesOptions> options)
    : ICommandHandler<UpdateGovernmentEntityCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(UpdateGovernmentEntityCommand request, CancellationToken cancellationToken)
    {
        GovernmentEntityDataSource source = request.Source ?? options.Value.DefaultSource;
        Result<IGovernmentEntityWriter> writer = storeFactory.GetWriter(source);
        if (writer.IsFailure)
        {
            return writer.Error;
        }

        Result<IReadOnlyList<GovernmentEntity>> existing = await storeFactory.ReadAllAsync(source, cancellationToken);
        if (existing.IsFailure)
        {
            return existing.Error;
        }

        GovernmentEntity? governmentEntity = existing.Value.FirstOrDefault(entity => entity.Id == request.GovernmentEntityId);
        if (governmentEntity is null)
        {
            return GovernmentEntityErrors.NOT_FOUND;
        }

        bool nameTaken = existing.Value.Any(entity =>
            entity.Id != request.GovernmentEntityId && GovernmentEntitySearch.NameEquals(entity.Name, request.Name));
        if (nameTaken)
        {
            return GovernmentEntityErrors.NAME_IN_USE;
        }

        governmentEntity.Update(request.Name, request.Category, request.StateBranch, request.Sector);
        bool updated = await writer.Value.UpdateAsync(governmentEntity, cancellationToken);
        return updated ? Unit.Value : GovernmentEntityErrors.NOT_FOUND;
    }
}

internal sealed class DeleteGovernmentEntityCommandHandler(
    IGovernmentEntityStoreFactory storeFactory,
    IOptions<GovernmentEntitiesOptions> options)
    : ICommandHandler<DeleteGovernmentEntityCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(DeleteGovernmentEntityCommand request, CancellationToken cancellationToken)
    {
        Result<IGovernmentEntityWriter> writer = storeFactory.GetWriter(request.Source ?? options.Value.DefaultSource);
        if (writer.IsFailure)
        {
            return writer.Error;
        }

        bool deleted = await writer.Value.DeleteAsync(request.GovernmentEntityId, cancellationToken);
        return deleted ? Unit.Value : GovernmentEntityErrors.NOT_FOUND;
    }
}

internal sealed class ImportGovernmentEntitiesCommandHandler(
    IGovernmentEntityStoreFactory storeFactory,
    ILogger<ImportGovernmentEntitiesCommandHandler> logger)
    : ICommandHandler<ImportGovernmentEntitiesCommand, GovernmentEntityImportResponse>
{
    public async Task<Result<GovernmentEntityImportResponse>> HandleAsync(
        ImportGovernmentEntitiesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Source == request.Destination)
        {
            return GovernmentEntityErrors.SOURCE_AND_DESTINATION_EQUAL;
        }

        Result<IGovernmentEntityWriter> writer = storeFactory.GetWriter(request.Destination);
        if (writer.IsFailure)
        {
            return writer.Error;
        }

        Result<IReadOnlyList<GovernmentEntity>> entities = await storeFactory.ReadAllAsync(request.Source, cancellationToken);
        if (entities.IsFailure)
        {
            return entities.Error;
        }

        await writer.Value.ReplaceAllAsync(entities.Value, cancellationToken);

        ApplicationLogMessages.GovernmentEntitiesImported(
            logger, entities.Value.Count, request.Source.ToString(), request.Destination.ToString());
        return new GovernmentEntityImportResponse(request.Source, request.Destination, entities.Value.Count);
    }
}
