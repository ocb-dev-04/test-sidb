namespace SB.Solicitudes.Aplicacion.Abstractions.Pagination;

public sealed record PagedResult<TItem>(IReadOnlyList<TItem> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > Pagination.DEFAULT_PAGE;
}

public static class Pagination
{
    public const int DEFAULT_PAGE = 1;
    public const int DEFAULT_PAGE_SIZE = 10;
    public const int MAXIMUM_PAGE_SIZE = 100;

    public static int Skip(int page, int pageSize) => (page - DEFAULT_PAGE) * pageSize;
}

public enum SortDirection
{
    Ascending = 1,
    Descending = 2,
}
