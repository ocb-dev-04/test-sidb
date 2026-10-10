using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SB.Solicitudes.Aplicacion.Abstractions.Events;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Infraestructura.Persistence;

/// <summary>
/// Solo para `dotnet ef`: genera migraciones sin levantar la API ni conectarse a la base de datos.
/// </summary>
public sealed class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string DESIGN_TIME_CONNECTION_STRING = "Server=localhost;Database=SolicitudesDesignTime;Integrated Security=True;TrustServerCertificate=True";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(DESIGN_TIME_CONNECTION_STRING)
            .Options;

        return new ApplicationDbContext(options, new NoDomainEventDispatcher());
    }

    private sealed class NoDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
