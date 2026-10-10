using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.Users;
using SB.Solicitudes.Aplicacion.Features.Users.GetUsers;
using SB.Solicitudes.Aplicacion.Features.Users.ManageUsers;
using SB.Solicitudes.Dominio.Users;
using SB.Solicitudes.Servicios.Common;

namespace SB.Solicitudes.Servicios.Controllers;

[Route("api/usuarios")]
public sealed class UsuariosController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(typeof(PagedResult<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetPaged([FromQuery] GetUsersQuery query, CancellationToken cancellationToken)
        => SendAsync(query, Ok, cancellationToken);

    /// <summary>Administradores y analistas activos a los que se puede asignar una solicitud.</summary>
    [HttpGet("responsables")]
    [Authorize(Policy = AuthorizationPolicies.MANAGEMENT)]
    [ProducesResponseType(typeof(IReadOnlyList<UserLookupResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAssignable(CancellationToken cancellationToken)
        => SendAsync(new GetAssignableUsersQuery(), Ok, cancellationToken);

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Create(CreateUserCommand command, CancellationToken cancellationToken)
        => SendAsync(command, user => StatusCode(StatusCodes.Status201Created, user), cancellationToken);

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
        => SendForNoContentAsync(new UpdateUserCommand(id, request.FullName, request.Role, request.IsActive), cancellationToken);
}

public sealed record UpdateUserRequest(string FullName, UserRole Role, bool IsActive);
