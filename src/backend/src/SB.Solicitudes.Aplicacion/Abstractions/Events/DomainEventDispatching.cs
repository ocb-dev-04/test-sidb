using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Aplicacion.Abstractions.Events;

public interface IDomainEventSubscriber<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}

internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, DomainEventInvoker> INVOKERS = new();

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            DomainEventInvoker invoker = INVOKERS.GetOrAdd(
                domainEvent.GetType(),
                eventType => (DomainEventInvoker)Activator.CreateInstance(
                    typeof(DomainEventInvoker<>).MakeGenericType(eventType))!);

            await invoker.InvokeAsync(domainEvent, serviceProvider, cancellationToken);
        }
    }
}

internal abstract class DomainEventInvoker
{
    public abstract Task InvokeAsync(IDomainEvent domainEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal sealed class DomainEventInvoker<TDomainEvent> : DomainEventInvoker
    where TDomainEvent : IDomainEvent
{
    public override async Task InvokeAsync(IDomainEvent domainEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        foreach (IDomainEventSubscriber<TDomainEvent> handler in serviceProvider.GetServices<IDomainEventSubscriber<TDomainEvent>>())
        {
            await handler.HandleAsync((TDomainEvent)domainEvent, cancellationToken);
        }
    }
}
