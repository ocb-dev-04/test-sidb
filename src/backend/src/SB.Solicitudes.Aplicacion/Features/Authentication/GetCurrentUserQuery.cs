using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Aplicacion.Features.Users;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.Authentication;

public sealed record GetCurrentUserQuery : IQuery<UserResponse>;

internal sealed class GetCurrentUserQueryHandler(IUserRepository userRepository, ICurrentUser currentUser)
    : IQueryHandler<GetCurrentUserQuery, UserResponse>
{
    public async Task<Result<UserResponse>> HandleAsync(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(currentUser.UserId, cancellationToken);
        return user is null ? UserErrors.NOT_FOUND : UserResponse.From(user);
    }
}
