using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Aplicacion.Abstractions.Events;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;
using SB.Solicitudes.Infraestructura.Persistence.Configurations;

namespace SB.Solicitudes.Infraestructura.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventPublisher domainEventPublisher)
    : DbContext(options), IUnitOfWork
{
    private const int MAXIMUM_PUBLISH_ROUNDS = 10;

    public DbSet<User> Users => Set<User>();

    public DbSet<Area> Areas => Set<Area>();

    public DbSet<RequestType> RequestTypes => Set<RequestType>();

    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    public DbSet<StatusHistory> StatusHistory => Set<StatusHistory>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Notification> Notifications => Set<Notification>();

    internal DbSet<ServiceRequestCounter> ServiceRequestCounters => Set<ServiceRequestCounter>();

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await PublishDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly,
            type => type.Namespace == typeof(ServiceRequestConfiguration).Namespace);

        modelBuilder.IgnoreDomainEvents();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        => configurationBuilder.UseUtcDateTimes();

    // Los eventos se publican antes de confirmar: las notificaciones que generan se guardan en la misma transacción.
    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        for (int round = 0; round < MAXIMUM_PUBLISH_ROUNDS; round++)
        {
            IDomainEvent[] domainEvents = ChangeTracker
                .Entries<IHasDomainEvents>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    IDomainEvent[] entityEvents = [.. entity.DomainEvents];
                    entity.ClearDomainEvents();
                    return entityEvents;
                })
                .ToArray();

            if (!domainEvents.Any())
            {
                return;
            }

            await domainEventPublisher.PublishAsync(domainEvents, cancellationToken);
        }

        throw new InvalidOperationException("Domain event publishing did not converge.");
    }
}
