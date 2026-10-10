using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Dominio.GovernmentEntities;
using SB.Solicitudes.Infraestructura.GovernmentEntities;
using SB.Solicitudes.Infraestructura.Persistence;

namespace SB.Solicitudes.Infraestructura.Initialization;

internal sealed class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IOptions<DatabaseOptions> options,
    ILogger<DatabaseInitializer> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.ApplyMigrationsOnStartup)
        {
            return;
        }

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        InfrastructureLogMessages.ApplyingMigrations(logger);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// Garantiza que el archivo de texto y la base SQLite tengan datos: si están vacíos se cargan desde el Excel original.
/// </summary>
internal sealed class GovernmentEntitiesInitializer(
    IServiceScopeFactory scopeFactory,
    IOptions<GovernmentEntityStorageOptions> options,
    ILogger<GovernmentEntitiesInitializer> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.InitializeStoresOnStartup)
        {
            return;
        }

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;

        await services.GetRequiredService<GovernmentEntitiesDbContext>().Database.EnsureCreatedAsync(cancellationToken);

        IReadOnlyList<GovernmentEntity> excelEntities = await services
            .GetRequiredKeyedService<IGovernmentEntityReader>(GovernmentEntityDataSource.Excel)
            .GetAllAsync(cancellationToken);
        if (excelEntities.Count == 0)
        {
            InfrastructureLogMessages.GovernmentEntitiesInitializationSkipped(logger, "Excel source file is missing or empty");
            return;
        }

        GovernmentEntityDataSource[] writableSources = [GovernmentEntityDataSource.TextFile, GovernmentEntityDataSource.Sqlite];
        foreach (GovernmentEntityDataSource source in writableSources)
        {
            IReadOnlyList<GovernmentEntity> current = await services
                .GetRequiredKeyedService<IGovernmentEntityReader>(source)
                .GetAllAsync(cancellationToken);
            if (current.Count > 0)
            {
                continue;
            }

            await services.GetRequiredKeyedService<IGovernmentEntityWriter>(source).ReplaceAllAsync(excelEntities, cancellationToken);
            InfrastructureLogMessages.GovernmentEntitiesStoreSeeded(logger, source.ToString(), excelEntities.Count);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
