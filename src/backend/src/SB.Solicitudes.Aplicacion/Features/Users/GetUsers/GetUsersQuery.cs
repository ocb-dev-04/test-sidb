using FluentValidation;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.Users.GetUsers;

public sealed record GetUsersQuery : IQuery<PagedResult<UserResponse>>
{
    public const int SEARCH_MAXIMUM_LENGTH = 100;

    /// <summary>Busca por nombre o correo.</summary>
    public string? Search { get; init; }

    public UserRole? Role { get; init; }

    public bool? IsActive { get; init; }

    public int Page { get; init; } = Pagination.DEFAULT_PAGE;

    public int PageSize { get; init; } = Pagination.DEFAULT_PAGE_SIZE;
}

internal sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        this.ValidatePagination(query => query.Page, query => query.PageSize);
        RuleFor(query => query.Search).MaximumLength(GetUsersQuery.SEARCH_MAXIMUM_LENGTH);
        RuleFor(query => query.Role).IsInEnum();
    }
}

internal sealed class GetUsersQueryHandler(IUserReadRepository userReadRepository)
    : IQueryHandler<GetUsersQuery, PagedResult<UserResponse>>
{
    public async Task<Result<PagedResult<UserResponse>>> HandleAsync(GetUsersQuery request, CancellationToken cancellationToken)
        => await userReadRepository.GetPagedAsync(request, cancellationToken);
}

public sealed record GetAssignableUsersQuery : IQuery<IReadOnlyList<UserLookupResponse>>;

internal sealed class GetAssignableUsersQueryHandler(IUserReadRepository userReadRepository)
    : IQueryHandler<GetAssignableUsersQuery, IReadOnlyList<UserLookupResponse>>
{
    public async Task<Result<IReadOnlyList<UserLookupResponse>>> HandleAsync(GetAssignableUsersQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<UserLookupResponse> users = await userReadRepository.GetAssignableAsync(cancellationToken);
        return Result.Success(users);
    }
}
