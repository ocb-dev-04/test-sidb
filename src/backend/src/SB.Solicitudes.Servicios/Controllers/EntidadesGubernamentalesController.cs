using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Servicios.Common;

namespace SB.Solicitudes.Servicios.Controllers;

/// <summary>
/// Mantenimiento de entidades gubernamentales. El parámetro <c>source</c> elige la fuente: TextFile (por defecto), Sqlite o Excel (solo lectura).
/// </summary>
[Route("api/entidades-gubernamentales")]
public sealed class EntidadesGubernamentalesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GovernmentEntityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetPaged([FromQuery] GetGovernmentEntitiesQuery query, CancellationToken cancellationToken)
        => SendAsync(query, Ok, cancellationToken);

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GovernmentEntityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(int id, [FromQuery] GovernmentEntityDataSource? source, CancellationToken cancellationToken)
        => SendAsync(new GetGovernmentEntityByIdQuery(id, source), Ok, cancellationToken);

    /// <summary>Fuentes de datos disponibles e indicador de solo lectura.</summary>
    [HttpGet("fuentes")]
    [ProducesResponseType(typeof(IReadOnlyList<GovernmentEntityDataSourceResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSources(CancellationToken cancellationToken)
        => SendAsync(new GetGovernmentEntityDataSourcesQuery(), Ok, cancellationToken);

    /// <summary>Valores distintos de categoría, poder del Estado y sector para los filtros.</summary>
    [HttpGet("valores-filtro")]
    [ProducesResponseType(typeof(GovernmentEntityFilterValuesResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterValues([FromQuery] GovernmentEntityDataSource? source, CancellationToken cancellationToken)
        => SendAsync(new GetGovernmentEntityFilterValuesQuery(source), Ok, cancellationToken);

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(typeof(GovernmentEntityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Create(
        GovernmentEntityRequest request,
        [FromQuery] GovernmentEntityDataSource? source,
        CancellationToken cancellationToken)
        => SendAsync(
            new CreateGovernmentEntityCommand(request.Name, request.Category, request.StateBranch, request.Sector, source),
            created => CreatedAtAction(nameof(GetById), new { id = created.Id, source }, created),
            cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Update(
        int id,
        GovernmentEntityRequest request,
        [FromQuery] GovernmentEntityDataSource? source,
        CancellationToken cancellationToken)
        => SendForNoContentAsync(
            new UpdateGovernmentEntityCommand(id, request.Name, request.Category, request.StateBranch, request.Sector, source),
            cancellationToken);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Delete(int id, [FromQuery] GovernmentEntityDataSource? source, CancellationToken cancellationToken)
        => SendForNoContentAsync(new DeleteGovernmentEntityCommand(id, source), cancellationToken);

    /// <summary>Reemplaza el contenido de la fuente destino con el de la fuente origen (por defecto, el Excel).</summary>
    [HttpPost("importaciones")]
    [Authorize(Policy = AuthorizationPolicies.ADMINISTRATOR)]
    [ProducesResponseType(typeof(GovernmentEntityImportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Import(
        [FromQuery] GovernmentEntityDataSource destination,
        [FromQuery] GovernmentEntityDataSource source = GovernmentEntityDataSource.Excel,
        CancellationToken cancellationToken = default)
        => SendAsync(new ImportGovernmentEntitiesCommand(source, destination), Ok, cancellationToken);
}

public sealed record GovernmentEntityRequest(string Name, string Category, string StateBranch, string Sector);
