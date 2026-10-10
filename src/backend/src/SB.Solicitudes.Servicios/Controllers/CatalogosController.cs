using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Features.Catalogs;
using SB.Solicitudes.Servicios.Common;

namespace SB.Solicitudes.Servicios.Controllers;

[Route("api/catalogos")]
public sealed class CatalogosController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Áreas activas; un administrador puede incluir las inactivas.</summary>
    [HttpGet("areas")]
    [ProducesResponseType(typeof(IReadOnlyList<AreaResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAreas([FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => SendAsync(new GetAreasQuery(includeInactive), Ok, cancellationToken);

    [HttpPost("areas")]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(typeof(AreaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> CreateArea(CreateAreaCommand command, CancellationToken cancellationToken)
        => SendAsync(command, area => StatusCode(StatusCodes.Status201Created, area), cancellationToken);

    [HttpPut("areas/{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> UpdateArea(int id, UpdateCatalogItemRequest request, CancellationToken cancellationToken)
        => SendForNoContentAsync(new UpdateAreaCommand(id, request.Name, request.IsActive), cancellationToken);

    /// <summary>Tipos de solicitud activos; un administrador puede incluir los inactivos.</summary>
    [HttpGet("tipos-solicitud")]
    [ProducesResponseType(typeof(IReadOnlyList<RequestTypeResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetRequestTypes([FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => SendAsync(new GetRequestTypesQuery(includeInactive), Ok, cancellationToken);

    [HttpPost("tipos-solicitud")]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(typeof(RequestTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> CreateRequestType(CreateRequestTypeCommand command, CancellationToken cancellationToken)
        => SendAsync(command, requestType => StatusCode(StatusCodes.Status201Created, requestType), cancellationToken);

    [HttpPut("tipos-solicitud/{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> UpdateRequestType(int id, UpdateRequestTypeRequest request, CancellationToken cancellationToken)
        => SendForNoContentAsync(new UpdateRequestTypeCommand(id, request.Name, request.Description, request.IsActive), cancellationToken);
}

public sealed record UpdateCatalogItemRequest(string Name, bool IsActive);

public sealed record UpdateRequestTypeRequest(string Name, string Description, bool IsActive);
