using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SB.Solicitudes.Aplicacion.Abstractions.Behaviors;
using SB.Solicitudes.Aplicacion.Abstractions.Events;

namespace SB.Solicitudes.Aplicacion;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // El orden de registro define el orden del pipeline: el primero envuelve a los demás.
            configuration.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton, includeInternalTypes: true);

        return services;
    }
}
