using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Infraestructura.GovernmentEntities;

/// <summary>
/// Base de datos en archivo de texto plano delimitado por '|' (CsvHelper), con encabezado y escritura atómica.
/// </summary>
internal sealed class TextFileGovernmentEntityStore(IOptions<GovernmentEntityStorageOptions> options)
    : IGovernmentEntityReader, IGovernmentEntityWriter, IDisposable
{
    public const string DELIMITER = "|";
    public const string HEADER = $"{ID_COLUMN}{DELIMITER}{NAME_COLUMN}{DELIMITER}{CATEGORY_COLUMN}{DELIMITER}{STATE_BRANCH_COLUMN}{DELIMITER}{SECTOR_COLUMN}";

    private const string ID_COLUMN = "Id";
    private const string NAME_COLUMN = "Nombre";
    private const string CATEGORY_COLUMN = "Categoria";
    private const string STATE_BRANCH_COLUMN = "PoderDelEstado";
    private const string SECTOR_COLUMN = "Sector";
    private const string TEMPORARY_EXTENSION = ".tmp";

    private static readonly UTF8Encoding UTF8_WITHOUT_BOM = new(encoderShouldEmitUTF8Identifier: false);

    private static readonly CsvConfiguration CSV_CONFIGURATION = new(CultureInfo.InvariantCulture)
    {
        Delimiter = DELIMITER,
        IgnoreBlankLines = true,

        // Un archivo editado a mano puede traer comillas sin escapar ("Dr. Rafael M. Moscoso"): se leen tal cual.
        BadDataFound = null,
    };

    private readonly SemaphoreSlim _fileLock = new(1, 1);

    private string FilePath => options.Value.TextFilePath;

    public async Task<IReadOnlyList<GovernmentEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            return await ReadUnsafeAsync(cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public Task<GovernmentEntity> AddAsync(GovernmentEntity governmentEntity, CancellationToken cancellationToken)
        => MutateAsync(
            entities =>
            {
                int nextId = entities.Count == 0 ? 1 : entities.Max(entity => entity.Id) + 1;
                GovernmentEntity created = governmentEntity.WithId(nextId);
                entities.Add(created);
                return created;
            },
            cancellationToken);

    public Task<bool> UpdateAsync(GovernmentEntity governmentEntity, CancellationToken cancellationToken)
        => MutateAsync(
            entities =>
            {
                int index = entities.FindIndex(entity => entity.Id == governmentEntity.Id);
                if (index < 0)
                {
                    return false;
                }

                entities[index] = governmentEntity;
                return true;
            },
            cancellationToken);

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        => MutateAsync(entities => entities.RemoveAll(entity => entity.Id == id) > 0, cancellationToken);

    public Task ReplaceAllAsync(IReadOnlyList<GovernmentEntity> governmentEntities, CancellationToken cancellationToken)
        => MutateAsync(
            entities =>
            {
                entities.Clear();
                entities.AddRange(governmentEntities);
                return true;
            },
            cancellationToken);

    public void Dispose() => _fileLock.Dispose();

    private async Task<TResult> MutateAsync<TResult>(Func<List<GovernmentEntity>, TResult> mutation, CancellationToken cancellationToken)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            List<GovernmentEntity> entities = [.. await ReadUnsafeAsync(cancellationToken)];
            TResult result = mutation(entities);
            await WriteUnsafeAsync(entities, cancellationToken);
            return result;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<IReadOnlyList<GovernmentEntity>> ReadUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(FilePath))
        {
            return [];
        }

        using StreamReader reader = new(FilePath, UTF8_WITHOUT_BOM);
        using CsvReader csv = new(reader, CSV_CONFIGURATION);
        csv.Context.RegisterClassMap<TextFileRowMap>();

        List<GovernmentEntity> entities = [];
        await foreach (TextFileRow row in csv.GetRecordsAsync<TextFileRow>(cancellationToken))
        {
            entities.Add(GovernmentEntity.Restore(row.Id, row.Name, row.Category, row.StateBranch, row.Sector));
        }

        return entities;
    }

    // Se escribe en un archivo temporal y luego se reemplaza: un fallo a mitad nunca deja el archivo corrupto.
    private async Task WriteUnsafeAsync(IEnumerable<GovernmentEntity> entities, CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = FilePath + TEMPORARY_EXTENSION;
        IEnumerable<TextFileRow> rows = entities
            .OrderBy(entity => entity.Id)
            .Select(entity => new TextFileRow
            {
                Id = entity.Id,
                Name = entity.Name,
                Category = entity.Category,
                StateBranch = entity.StateBranch,
                Sector = entity.Sector,
            });

        await using (StreamWriter writer = new(temporaryPath, append: false, UTF8_WITHOUT_BOM))
        await using (CsvWriter csv = new(writer, CSV_CONFIGURATION))
        {
            csv.Context.RegisterClassMap<TextFileRowMap>();
            await csv.WriteRecordsAsync(rows, cancellationToken);
        }

        File.Move(temporaryPath, FilePath, overwrite: true);
    }

    private sealed class TextFileRow
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public string StateBranch { get; init; } = string.Empty;

        public string Sector { get; init; } = string.Empty;
    }

    private sealed class TextFileRowMap : ClassMap<TextFileRow>
    {
        public TextFileRowMap()
        {
            Map(row => row.Id).Name(ID_COLUMN);
            Map(row => row.Name).Name(NAME_COLUMN);
            Map(row => row.Category).Name(CATEGORY_COLUMN);
            Map(row => row.StateBranch).Name(STATE_BRANCH_COLUMN);
            Map(row => row.Sector).Name(SECTOR_COLUMN);
        }
    }
}
