using FluentValidation;

namespace SB.Solicitudes.Aplicacion.Abstractions.Pagination;

internal static class PaginationValidationExtensions
{
    public static void ValidatePagination<TQuery>(
        this AbstractValidator<TQuery> validator,
        Func<TQuery, int> page,
        Func<TQuery, int> pageSize)
    {
        validator.RuleFor(query => page(query))
            .GreaterThanOrEqualTo(Pagination.DEFAULT_PAGE)
            .OverridePropertyName("page");

        validator.RuleFor(query => pageSize(query))
            .InclusiveBetween(Pagination.DEFAULT_PAGE, Pagination.MAXIMUM_PAGE_SIZE)
            .OverridePropertyName("pageSize");
    }
}
