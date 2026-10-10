using FluentValidation;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Aplicacion.Features.GovernmentEntities;

public sealed record GetGovernmentEntitiesQuery : IQuery<PagedResult<GovernmentEntityResponse>>
{
    public const int SEARCH_MAXIMUM_LENGTH = 100;

    /// <summary>Fuente de datos a consultar; si se omite se usa la configurada por defecto.</summary>
    public GovernmentEntityDataSource? Source { get; init; }

    /// <summary>Busca por nombre sin distinguir mayúsculas ni tildes.</summary>
    public string? Search { get; init; }

    public string? Category { get; init; }

    public string? StateBranch { get; init; }

    public string? Sector { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;

    public int Page { get; init; } = Pagination.DEFAULT_PAGE;

    public int PageSize { get; init; } = Pagination.DEFAULT_PAGE_SIZE;
}

public sealed record GetGovernmentEntityByIdQuery(int GovernmentEntityId, GovernmentEntityDataSource? Source) : IQuery<GovernmentEntityResponse>;

public sealed record GetGovernmentEntityFilterValuesQuery(GovernmentEntityDataSource? Source) : IQuery<GovernmentEntityFilterValuesResponse>;

public sealed record GetGovernmentEntityDataSourcesQuery : IQuery<IReadOnlyList<GovernmentEntityDataSourceResponse>>;

internal sealed class GetGovernmentEntitiesQueryValidator : AbstractValidator<GetGovernmentEntitiesQuery>
{
    public GetGovernmentEntitiesQueryValidator()
    {
        this.ValidatePagination(query => query.Page, query => query.PageSize);
        RuleFor(query => query.Source).IsInEnum();
        RuleFor(query => query.SortDirection).IsInEnum();
        RuleFor(query => query.Search).MaximumLength(GetGovernmentEntitiesQuery.SEARCH_MAXIMUM_LENGTH);
    }
}

internal sealed class GetGovernmentEntitiesQueryHandler(
    IGovernmentEntityStoreFactory storeFactory,
    IOptions<GovernmentEntitiesOptions> options)
    : IQueryHandler<GetGovernmentEntitiesQuery, PagedResult<GovernmentEntityResponse>>
{
    public async Task<Result<PagedResult<GovernmentEntityResponse>>> HandleAsync(
        GetGovernmentEntitiesQuery request,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<GovernmentEntity>> entities = await storeFactory.ReadAllAsync(
            request.Source ?? options.Value.DefaultSource, cancellationToken);

        return entities.Map(values => GovernmentEntitySearch.Apply(values, request));
    }
}

internal sealed class GetGovernmentEntityByIdQueryHandler(
    IGovernmentEntityStoreFactory storeFactory,
    IOptions<GovernmentEntitiesOptions> options)
    : IQueryHandler<GetGovernmentEntityByIdQuery, GovernmentEntityResponse>
{
    public async Task<Result<GovernmentEntityResponse>> HandleAsync(GetGovernmentEntityByIdQuery request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<GovernmentEntity>> entities = await storeFactory.ReadAllAsync(
            request.Source ?? options.Value.DefaultSource, cancellationToken);
        if (entities.IsFailure)
        {
            return entities.Error;
        }

        GovernmentEntity? governmentEntity = entities.Value.FirstOrDefault(entity => entity.Id == request.GovernmentEntityId);
        return governmentEntity is null
            ? GovernmentEntityErrors.NOT_FOUND
            : GovernmentEntityResponse.From(governmentEntity);
    }
}

internal sealed class GetGovernmentEntityFilterValuesQueryHandler(
    IGovernmentEntityStoreFactory storeFactory,
    IOptions<GovernmentEntitiesOptions> options)
    : IQueryHandler<GetGovernmentEntityFilterValuesQuery, GovernmentEntityFilterValuesResponse>
{
    public async Task<Result<GovernmentEntityFilterValuesResponse>> HandleAsync(
        GetGovernmentEntityFilterValuesQuery request,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<GovernmentEntity>> entities = await storeFactory.ReadAllAsync(
            request.Source ?? options.Value.DefaultSource, cancellationToken);

        return entities.Map(values => new GovernmentEntityFilterValuesResponse(
            GovernmentEntitySearch.DistinctValues(values.Select(entity => entity.Category)),
            GovernmentEntitySearch.DistinctValues(values.Select(entity => entity.StateBranch)),
            GovernmentEntitySearch.DistinctValues(values.Select(entity => entity.Sector))));
    }
}

internal sealed class GetGovernmentEntityDataSourcesQueryHandler(
    IGovernmentEntityStoreFactory storeFactory,
    IOptions<GovernmentEntitiesOptions> options)
    : IQueryHandler<GetGovernmentEntityDataSourcesQuery, IReadOnlyList<GovernmentEntityDataSourceResponse>>
{
    public Task<Result<IReadOnlyList<GovernmentEntityDataSourceResponse>>> HandleAsync(
        GetGovernmentEntityDataSourcesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<GovernmentEntityDataSourceResponse> sources = [.. Enum.GetValues<GovernmentEntityDataSource>()
            .Select(source => new GovernmentEntityDataSourceResponse(
                source,
                !storeFactory.IsWritable(source),
                source == options.Value.DefaultSource))];

        return Task.FromResult(Result.Success(sources));
    }
}

internal static class GovernmentEntityStoreFactoryExtensions
{
    public static async Task<Result<IReadOnlyList<GovernmentEntity>>> ReadAllAsync(
        this IGovernmentEntityStoreFactory storeFactory,
        GovernmentEntityDataSource source,
        CancellationToken cancellationToken)
    {
        Result<IGovernmentEntityReader> reader = storeFactory.GetReader(source);
        if (reader.IsFailure)
        {
            return reader.Error;
        }

        IReadOnlyList<GovernmentEntity> entities = await reader.Value.GetAllAsync(cancellationToken);
        return Result.Success(entities);
    }
}
