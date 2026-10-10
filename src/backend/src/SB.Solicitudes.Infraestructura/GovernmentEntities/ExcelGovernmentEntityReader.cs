using System.Globalization;
using System.Text;
using ExcelDataReader;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Infraestructura.GovernmentEntities;

/// <summary>
/// Lee el Excel original por encabezado de columna (no por posición) y cachea mientras el archivo no cambie.
/// </summary>
internal sealed class ExcelGovernmentEntityReader(IOptions<GovernmentEntityStorageOptions> options)
    : IGovernmentEntityReader
{
    public const string NAME_HEADER = "Nombre";
    public const string CATEGORY_HEADER = "Categoría";
    public const string STATE_BRANCH_HEADER = "Poder del Estado";
    public const string SECTOR_HEADER = "Sector";

    private const int NOT_FOUND = -1;
    private const CompareOptions HEADER_COMPARISON = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private static readonly CompareInfo COMPARE_INFO = CultureInfo.InvariantCulture.CompareInfo;

    private readonly object _cacheLock = new();
    private (DateTime LastWriteUtc, ExcelRow[] Rows)? _cache;

    static ExcelGovernmentEntityReader() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public Task<IReadOnlyList<GovernmentEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        ExcelRow[] rows = GetRows();
        IReadOnlyList<GovernmentEntity> entities = [.. rows.Select(row =>
            GovernmentEntity.Restore(row.Id, row.Name, row.Category, row.StateBranch, row.Sector))];
        return Task.FromResult(entities);
    }

    private ExcelRow[] GetRows()
    {
        string path = options.Value.ExcelFilePath;
        if (!File.Exists(path))
        {
            return [];
        }

        DateTime lastWriteUtc = File.GetLastWriteTimeUtc(path);
        lock (_cacheLock)
        {
            if (_cache is { } cache && cache.LastWriteUtc == lastWriteUtc)
            {
                return cache.Rows;
            }

            ExcelRow[] rows = ReadWorkbook(path);
            _cache = (lastWriteUtc, rows);
            return rows;
        }
    }

    private static ExcelRow[] ReadWorkbook(string path)
    {
        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);

        if (!reader.Read())
        {
            return [];
        }

        string[] headers = [.. Enumerable.Range(0, reader.FieldCount).Select(index => Convert.ToString(reader.GetValue(index), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty)];
        int nameColumn = FindColumn(headers, NAME_HEADER);
        int categoryColumn = FindColumn(headers, CATEGORY_HEADER);
        int stateBranchColumn = FindColumn(headers, STATE_BRANCH_HEADER);
        int sectorColumn = FindColumn(headers, SECTOR_HEADER);
        if (nameColumn == NOT_FOUND)
        {
            throw new InvalidDataException($"The Excel file does not contain the '{NAME_HEADER}' column.");
        }

        List<ExcelRow> rows = [];
        while (reader.Read())
        {
            string name = ReadCell(reader, nameColumn);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            rows.Add(new ExcelRow(
                rows.Count + 1,
                name,
                ReadCell(reader, categoryColumn),
                ReadCell(reader, stateBranchColumn),
                ReadCell(reader, sectorColumn)));
        }

        return [.. rows];
    }

    private static int FindColumn(string[] headers, string expectedHeader)
        => Array.FindIndex(headers, header => COMPARE_INFO.Compare(header, expectedHeader, HEADER_COMPARISON) == 0);

    private static string ReadCell(IExcelDataReader reader, int column)
        => column == NOT_FOUND
            ? string.Empty
            : Convert.ToString(reader.GetValue(column), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;

    private sealed record ExcelRow(int Id, string Name, string Category, string StateBranch, string Sector);
}
