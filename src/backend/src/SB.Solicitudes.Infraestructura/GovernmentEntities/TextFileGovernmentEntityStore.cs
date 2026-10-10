using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Infraestructura.GovernmentEntities;

/// <summary>
/// Base de datos en archivo de texto plano delimitado por '|', con encabezado y escritura atómica.
/// </summary>
internal sealed class TextFileGovernmentEntityStore(IOptions<GovernmentEntityStorageOptions> options)
    : IGovernmentEntityReader, IGovernmentEntityWriter, IDisposable
{
    public const string HEADER = "Id|Nombre|Categoria|PoderDelEstado|Sector";

    private const char DELIMITER = '|';
    private const char ESCAPE = '\\';
    private const char ESCAPED_NEW_LINE = 'n';
    private const int FIELDS_PER_LINE = 5;
    private const int ID_INDEX = 0;
    private const int NAME_INDEX = 1;
    private const int CATEGORY_INDEX = 2;
    private const int STATE_BRANCH_INDEX = 3;
    private const int SECTOR_INDEX = 4;
    private const string TEMPORARY_EXTENSION = ".tmp";

    private static readonly UTF8Encoding UTF8_WITHOUT_BOM = new(encoderShouldEmitUTF8Identifier: false);

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

    internal static string Escape(string value)
        => value
            .Replace(ESCAPE.ToString(), string.Concat(ESCAPE, ESCAPE), StringComparison.Ordinal)
            .Replace(DELIMITER.ToString(), string.Concat(ESCAPE, DELIMITER), StringComparison.Ordinal)
            .Replace(Environment.NewLine, string.Concat(ESCAPE, ESCAPED_NEW_LINE), StringComparison.Ordinal)
            .Replace("\n", string.Concat(ESCAPE, ESCAPED_NEW_LINE), StringComparison.Ordinal);

    internal static List<string> SplitLine(string line)
    {
        List<string> fields = [];
        StringBuilder current = new();
        for (int index = 0; index < line.Length; index++)
        {
            char character = line[index];
            if (character == ESCAPE && index + 1 < line.Length)
            {
                char next = line[++index];
                current.Append(next == ESCAPED_NEW_LINE ? '\n' : next);
            }
            else if (character == DELIMITER)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }

    private static string ToLine(GovernmentEntity entity)
        => string.Join(
            DELIMITER,
            entity.Id.ToString(CultureInfo.InvariantCulture),
            Escape(entity.Name),
            Escape(entity.Category),
            Escape(entity.StateBranch),
            Escape(entity.Sector));

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

        string[] lines = await File.ReadAllLinesAsync(FilePath, UTF8_WITHOUT_BOM, cancellationToken);
        return lines
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(SplitLine)
            .Where(fields => fields.Count == FIELDS_PER_LINE)
            .Select(fields => GovernmentEntity.Restore(
                int.Parse(fields[ID_INDEX], NumberStyles.None, CultureInfo.InvariantCulture),
                fields[NAME_INDEX],
                fields[CATEGORY_INDEX],
                fields[STATE_BRANCH_INDEX],
                fields[SECTOR_INDEX]))
            .ToArray();
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
        IEnumerable<string> lines = entities.OrderBy(entity => entity.Id).Select(ToLine).Prepend(HEADER);
        await File.WriteAllLinesAsync(temporaryPath, lines, UTF8_WITHOUT_BOM, cancellationToken);
        File.Move(temporaryPath, FilePath, overwrite: true);
    }
}
