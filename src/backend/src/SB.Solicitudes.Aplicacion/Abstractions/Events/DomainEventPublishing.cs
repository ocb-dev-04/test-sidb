using MediatR;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Aplicacion.Abstractions.Events;

// El dominio no conoce MediatR: sus eventos viajan envueltos en esta notificación.
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;

public interface IDomainEventSubscriber<TDomainEvent> : INotificationHandler<DomainEventNotification<TDomainEvent>>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);

    Task INotificationHandler<DomainEventNotification<TDomainEvent>>.Handle(
        DomainEventNotification<TDomainEvent> notification,
        CancellationToken cancellationToken)
        => HandleAsync(notification.DomainEvent, cancellationToken);
}

public interface IDomainEventPublisher
{
    Task PublishAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}

internal sealed class DomainEventPublisher(IPublisher publisher) : IDomainEventPublisher
{
    private static readonly Type NOTIFICATION_TYPE = typeof(DomainEventNotification<>);

    public async Task PublishAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            object notification = Activator.CreateInstance(
                NOTIFICATION_TYPE.MakeGenericType(domainEvent.GetType()),
                domainEvent)!;

            await publisher.Publish(notification, cancellationToken);
        }
    }
}
