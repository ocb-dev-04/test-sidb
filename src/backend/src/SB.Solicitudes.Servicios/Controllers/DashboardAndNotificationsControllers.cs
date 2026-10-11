using MediatR;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.Dashboard;
using SB.Solicitudes.Aplicacion.Features.Notifications;
using SB.Solicitudes.Servicios.Common;

namespace SB.Solicitudes.Servicios.Controllers;

[Route("api/dashboard")]
public sealed class DashboardController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Métricas por estado y prioridad, vencidas y últimas solicitudes, según la visibilidad del rol.</summary>
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        => SendAsync(new GetDashboardSummaryQuery(), Ok, cancellationToken);
}

[Route("api/notificaciones")]
public sealed class NotificacionesController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Bandeja de notificaciones dentro de la aplicación del usuario autenticado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMine([FromQuery] GetMyNotificationsQuery query, CancellationToken cancellationToken)
        => SendAsync(query, Ok, cancellationToken);

    [HttpGet("no-leidas/total")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public Task<IActionResult> CountUnread(CancellationToken cancellationToken)
        => SendAsync(new GetUnreadNotificationCountQuery(), count => Ok(count), cancellationToken);

    [HttpPatch("{id:guid}/leida")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        => SendForNoContentAsync(new MarkNotificationAsReadCommand(id), cancellationToken);
}
