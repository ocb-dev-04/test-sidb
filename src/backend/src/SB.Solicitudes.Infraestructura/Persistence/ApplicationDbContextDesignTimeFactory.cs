using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SB.Solicitudes.Aplicacion.Abstractions.Events;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Infraestructura.Persistence;

/// <summary>
/// Solo para `dotnet ef`: genera migraciones sin levantar la API. `database update` usa la variable SB_SOLICITUDES_CONNECTION_STRING o la base local de desarrollo.
/// </summary>
public sealed class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public const string CONNECTION_STRING_VARIABLE = "SB_SOLICITUDES_CONNECTION_STRING";
    private const string DEFAULT_CONNECTION_STRING = "Server=localhost,1433;Database=SBSolicitudes;User Id=sa;Password=SbSolicitudes#Dev2026;TrustServerCertificate=True";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(CONNECTION_STRING_VARIABLE) ?? DEFAULT_CONNECTION_STRING)
            .Options;

        return new ApplicationDbContext(options, new NoDomainEventDispatcher());
    }

    private sealed class NoDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
