using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SB.Solicitudes.Aplicacion.Abstractions.Behaviors;
using SB.Solicitudes.Aplicacion.Abstractions.Events;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;

namespace SB.Solicitudes.Aplicacion;

public static class DependencyInjection
{
    private static readonly Type[] HANDLER_CONTRACTS = [typeof(IRequestHandler<,>), typeof(IDomainEventSubscriber<>)];

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // El orden de registro define el orden del pipeline: el primero envuelve a los demás.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton, includeInternalTypes: true);
        services.AddHandlersFromAssembly();

        return services;
    }

    private static void AddHandlersFromAssembly(this IServiceCollection services)
    {
        IEnumerable<Type> concreteTypes = typeof(DependencyInjection).Assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false });

        foreach (Type implementationType in concreteTypes)
        {
            IEnumerable<Type> contracts = implementationType
                .GetInterfaces()
                .Where(contract => contract.IsGenericType && HANDLER_CONTRACTS.Contains(contract.GetGenericTypeDefinition()));

            foreach (Type contract in contracts)
            {
                services.AddScoped(contract, implementationType);
            }
        }
    }
}
