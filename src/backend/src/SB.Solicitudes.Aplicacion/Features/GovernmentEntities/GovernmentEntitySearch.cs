using System.Globalization;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Aplicacion.Features.GovernmentEntities;

/// <summary>
/// Filtra en memoria para que las tres fuentes respondan igual; el volumen del catálogo es acotado (≈180 registros).
/// </summary>
internal static class GovernmentEntitySearch
{
    private const CompareOptions ACCENT_AND_CASE_INSENSITIVE = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private static readonly CompareInfo COMPARE_INFO = CultureInfo.InvariantCulture.CompareInfo;

    private static readonly StringComparer NAME_COMPARER = StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase);

    public static PagedResult<GovernmentEntityResponse> Apply(
        IEnumerable<GovernmentEntity> governmentEntities,
        GetGovernmentEntitiesQuery query)
    {
        IEnumerable<GovernmentEntity> filtered = governmentEntities
            .Where(entity => Contains(entity.Name, query.Search))
            .Where(entity => EqualsOrEmpty(entity.Category, query.Category))
            .Where(entity => EqualsOrEmpty(entity.StateBranch, query.StateBranch))
            .Where(entity => EqualsOrEmpty(entity.Sector, query.Sector));

        GovernmentEntity[] ordered = (query.SortDirection == SortDirection.Descending
                ? filtered.OrderByDescending(entity => entity.Name, NAME_COMPARER)
                : filtered.OrderBy(entity => entity.Name, NAME_COMPARER))
            .ToArray();

        GovernmentEntityResponse[] pageItems = ordered
            .Skip(Pagination.Skip(query.Page, query.PageSize))
            .Take(query.PageSize)
            .Select(GovernmentEntityResponse.From)
            .ToArray();

        return new PagedResult<GovernmentEntityResponse>(pageItems, query.Page, query.PageSize, ordered.Length);
    }

    public static bool NameEquals(string left, string right)
        => COMPARE_INFO.Compare(left.Trim(), right.Trim(), ACCENT_AND_CASE_INSENSITIVE) == 0;

    public static IReadOnlyList<string> DistinctValues(IEnumerable<string> values)
        => [.. values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(NAME_COMPARER)
            .Order(NAME_COMPARER)];

    private static bool Contains(string source, string? term)
        => string.IsNullOrWhiteSpace(term)
            || COMPARE_INFO.IndexOf(source, term.Trim(), ACCENT_AND_CASE_INSENSITIVE) >= 0;

    private static bool EqualsOrEmpty(string source, string? expected)
        => string.IsNullOrWhiteSpace(expected) || NameEquals(source, expected);
}
