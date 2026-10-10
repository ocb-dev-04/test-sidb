using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Features.Authentication;
using SB.Solicitudes.Aplicacion.Features.Users;
using SB.Solicitudes.Servicios.Common;

namespace SB.Solicitudes.Servicios.Controllers;

[Route("api/auth")]
public sealed class AuthController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Autentica al usuario y emite un token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.LOGIN)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
        => SendAsync(command, Ok, cancellationToken);

    /// <summary>Perfil del usuario autenticado.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> Me(CancellationToken cancellationToken)
        => SendAsync(new GetCurrentUserQuery(), Ok, cancellationToken);
}
