using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Abstractions;
using SB.Solicitudes.Aplicacion.Abstractions.Events;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.ServiceRequests.Events;

namespace SB.Solicitudes.Aplicacion.Features.Notifications;

/// <summary>
/// Traduce eventos de dominio en notificaciones pendientes; el canal de entrega lo resuelve Infraestructura.
/// </summary>
internal sealed class ServiceRequestNotificationHandler(
    IServiceRequestRepository serviceRequestRepository,
    INotificationRepository notificationRepository,
    IOptions<NotificationOptions> options,
    ILogger<ServiceRequestNotificationHandler> logger)
    : IDomainEventSubscriber<ServiceRequestCreatedDomainEvent>,
      IDomainEventSubscriber<ServiceRequestAssignedDomainEvent>,
      IDomainEventSubscriber<ServiceRequestStatusChangedDomainEvent>,
      IDomainEventSubscriber<ServiceRequestCommentAddedDomainEvent>
{
    public async Task HandleAsync(ServiceRequestCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ServiceRequest? serviceRequest = await serviceRequestRepository.GetByIdAsync(domainEvent.ServiceRequestId, cancellationToken);
        if (serviceRequest is null)
        {
            return;
        }

        Queue(
            serviceRequest,
            [serviceRequest.RequesterId],
            excludedUserId: null,
            NotificationType.ServiceRequestCreated,
            $"Solicitud {serviceRequest.Code} registrada",
            $"Su solicitud \"{serviceRequest.Title}\" fue registrada con el código {serviceRequest.Code}.",
            domainEvent.OccurredOnUtc);
    }

    public async Task HandleAsync(ServiceRequestAssignedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ServiceRequest? serviceRequest = await serviceRequestRepository.GetByIdAsync(domainEvent.ServiceRequestId, cancellationToken);
        if (serviceRequest is null)
        {
            return;
        }

        Queue(
            serviceRequest,
            [domainEvent.AssigneeId, serviceRequest.RequesterId],
            domainEvent.AssignedById,
            NotificationType.ServiceRequestAssigned,
            $"Solicitud {serviceRequest.Code} asignada",
            $"La solicitud {serviceRequest.Code} \"{serviceRequest.Title}\" tiene un nuevo responsable.",
            domainEvent.OccurredOnUtc);
    }

    public async Task HandleAsync(ServiceRequestStatusChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ServiceRequest? serviceRequest = await serviceRequestRepository.GetByIdAsync(domainEvent.ServiceRequestId, cancellationToken);
        if (serviceRequest is null)
        {
            return;
        }

        bool isClosing = domainEvent.NewStatus == ServiceRequestStatus.Closed;
        string newStatusName = ServiceRequestStatusNames.From(domainEvent.NewStatus);

        Queue(
            serviceRequest,
            [serviceRequest.RequesterId, serviceRequest.AssigneeId],
            domainEvent.ChangedById,
            isClosing ? NotificationType.ServiceRequestClosed : NotificationType.ServiceRequestStatusChanged,
            isClosing ? $"Solicitud {serviceRequest.Code} cerrada" : $"Solicitud {serviceRequest.Code}: {newStatusName}",
            $"La solicitud {serviceRequest.Code} pasó de \"{ServiceRequestStatusNames.From(domainEvent.PreviousStatus)}\" a \"{newStatusName}\".",
            domainEvent.OccurredOnUtc);
    }

    public async Task HandleAsync(ServiceRequestCommentAddedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ServiceRequest? serviceRequest = await serviceRequestRepository.GetByIdAsync(domainEvent.ServiceRequestId, cancellationToken);
        if (serviceRequest is null)
        {
            return;
        }

        // Los comentarios internos nunca deben llegar al solicitante.
        Guid?[] recipients = domainEvent.Visibility == CommentVisibility.Internal
            ? [serviceRequest.AssigneeId]
            : [serviceRequest.RequesterId, serviceRequest.AssigneeId];

        Queue(
            serviceRequest,
            recipients,
            domainEvent.AuthorId,
            NotificationType.ServiceRequestCommented,
            $"Nuevo comentario en {serviceRequest.Code}",
            $"Se agregó un comentario a la solicitud {serviceRequest.Code} \"{serviceRequest.Title}\".",
            domainEvent.OccurredOnUtc);
    }

    private void Queue(
        ServiceRequest serviceRequest,
        IEnumerable<Guid?> candidateRecipients,
        Guid? excludedUserId,
        NotificationType type,
        string subject,
        string message,
        DateTime utcNow)
    {
        Guid[] recipients = candidateRecipients
            .OfType<Guid>()
            .Where(recipientId => recipientId != excludedUserId)
            .Distinct()
            .ToArray();

        Notification[] notifications = recipients
            .SelectMany(recipientId => options.Value.Channels.Select(channel =>
                Notification.Create(recipientId, serviceRequest.Id, channel, type, subject, message, utcNow)))
            .ToArray();

        if (notifications.Length == 0)
        {
            return;
        }

        notificationRepository.AddRange(notifications);
        ApplicationLogMessages.NotificationsQueued(logger, notifications.Length, type, serviceRequest.Id);
    }
}
