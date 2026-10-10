using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Aplicacion.Features.GovernmentEntities;

public enum GovernmentEntityDataSource
{
    TextFile = 1,
    Sqlite = 2,
    Excel = 3,
}

public interface IGovernmentEntityReader
{
    Task<IReadOnlyList<GovernmentEntity>> GetAllAsync(CancellationToken cancellationToken);
}

public interface IGovernmentEntityWriter
{
    Task<GovernmentEntity> AddAsync(GovernmentEntity governmentEntity, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(GovernmentEntity governmentEntity, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);

    Task ReplaceAllAsync(IReadOnlyList<GovernmentEntity> governmentEntities, CancellationToken cancellationToken);
}

/// <summary>
/// Resuelve la estrategia de almacenamiento según la fuente elegida; Excel solo implementa lectura.
/// </summary>
public interface IGovernmentEntityStoreFactory
{
    bool IsWritable(GovernmentEntityDataSource source);

    Result<IGovernmentEntityReader> GetReader(GovernmentEntityDataSource source);

    Result<IGovernmentEntityWriter> GetWriter(GovernmentEntityDataSource source);
}

public sealed class GovernmentEntitiesOptions
{
    public const string SECTION_NAME = "GovernmentEntities";

    public GovernmentEntityDataSource DefaultSource { get; init; } = GovernmentEntityDataSource.TextFile;
}

public sealed record GovernmentEntityResponse(int Id, string Name, string Category, string StateBranch, string Sector)
{
    public static GovernmentEntityResponse From(GovernmentEntity governmentEntity)
        => new(
            governmentEntity.Id,
            governmentEntity.Name,
            governmentEntity.Category,
            governmentEntity.StateBranch,
            governmentEntity.Sector);
}

public sealed record GovernmentEntityFilterValuesResponse(
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> StateBranches,
    IReadOnlyList<string> Sectors);

public sealed record GovernmentEntityDataSourceResponse(GovernmentEntityDataSource Source, bool IsReadOnly, bool IsDefault);

public sealed record GovernmentEntityImportResponse(
    GovernmentEntityDataSource Source,
    GovernmentEntityDataSource Destination,
    int ImportedCount);
