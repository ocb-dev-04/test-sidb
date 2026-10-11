using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.AddComment;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.AssignServiceRequest;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.ChangeServiceRequestStatus;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.CreateServiceRequest;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.GetServiceRequestById;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.GetServiceRequests;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.UpdateServiceRequest;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Servicios.Common;

namespace SB.Solicitudes.Servicios.Controllers;

[Route("api/solicitudes")]
public sealed class SolicitudesController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Consulta paginada con filtros; cada rol solo ve las solicitudes que le corresponden.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ServiceRequestSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetPaged([FromQuery] GetServiceRequestsQuery query, CancellationToken cancellationToken)
        => SendAsync(query, Ok, cancellationToken);

    /// <summary>Detalle con historial de estados y comentarios.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServiceRequestDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => SendAsync(new GetServiceRequestByIdQuery(id), Ok, cancellationToken);

    /// <summary>Registra una solicitud a nombre del usuario autenticado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceRequestCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create(CreateServiceRequestCommand command, CancellationToken cancellationToken)
        => SendAsync(command, created => CreatedAtAction(nameof(GetById), new { id = created.Id }, created), cancellationToken);

    /// <summary>Edición parcial: solo se modifican los campos enviados.</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Update(Guid id, UpdateServiceRequestRequest request, CancellationToken cancellationToken)
        => SendForNoContentAsync(
            new UpdateServiceRequestCommand(id, request.Title, request.Description, request.Priority, request.RequestTypeId, request.AreaId, request.EvidenceReference),
            cancellationToken);

    /// <summary>Cambia el estado; resolver, cerrar o reabrir exige comentario.</summary>
    [HttpPatch("{id:guid}/estado")]
    [Authorize(Policy = AuthorizationPolicies.MANAGEMENT)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> ChangeStatus(Guid id, ChangeStatusRequest request, CancellationToken cancellationToken)
        => SendForNoContentAsync(new ChangeServiceRequestStatusCommand(id, request.NewStatus, request.Comment), cancellationToken);

    /// <summary>Asigna o reasigna el responsable técnico.</summary>
    [HttpPatch("{id:guid}/asignacion")]
    [Authorize(Policy = AuthorizationPolicies.MANAGEMENT)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Assign(Guid id, AssignRequest request, CancellationToken cancellationToken)
        => SendForNoContentAsync(new AssignServiceRequestCommand(id, request.AssigneeId), cancellationToken);

    /// <summary>Agrega un comentario público o interno (los internos no los ve el solicitante).</summary>
    [HttpPost("{id:guid}/comentarios")]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> AddComment(Guid id, AddCommentRequest request, CancellationToken cancellationToken)
        => SendAsync(
            new AddServiceRequestCommentCommand(id, request.Text, request.Visibility),
            comment => CreatedAtAction(nameof(GetById), new { id }, comment),
            cancellationToken);
}

/// <summary>Campos opcionales: los nulos no se modifican.</summary>
public sealed record UpdateServiceRequestRequest(
    string? Title,
    string? Description,
    ServiceRequestPriority? Priority,
    int? RequestTypeId,
    int? AreaId,
    string? EvidenceReference);

public sealed record ChangeStatusRequest(ServiceRequestStatus NewStatus, string? Comment);

public sealed record AssignRequest(Guid AssigneeId);

public sealed record AddCommentRequest(string Text, CommentVisibility Visibility);
