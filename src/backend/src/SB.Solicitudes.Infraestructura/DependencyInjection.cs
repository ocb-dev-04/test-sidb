using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Aplicacion.Features.Notifications;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests;
using SB.Solicitudes.Aplicacion.Features.Users;
using SB.Solicitudes.Infraestructura.Authentication;
using SB.Solicitudes.Infraestructura.GovernmentEntities;
using SB.Solicitudes.Infraestructura.Initialization;
using SB.Solicitudes.Infraestructura.Notifications;
using SB.Solicitudes.Infraestructura.Persistence;
using SB.Solicitudes.Infraestructura.Persistence.Repositories;

namespace SB.Solicitudes.Infraestructura;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services
            .AddPersistence(configuration)
            .AddAuthenticationServices(configuration)
            .AddNotificationDelivery(configuration)
            .AddGovernmentEntityStores(configuration);

        // Orden de arranque: primero el esquema, luego los datos de entidades y por último el worker.
        services.AddHostedService<DatabaseInitializer>();
        services.AddHostedService<GovernmentEntitiesInitializer>();
        services.AddHostedService<NotificationDispatchWorker>();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection(DatabaseOptions.SECTION_NAME));
        services.AddSingleton<SlowQueryInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            DatabaseOptions databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            string connectionString = RequiredConnectionString(serviceProvider, DatabaseOptions.CONNECTION_STRING_NAME);
            options
                .UseSqlServer(connectionString, sqlServer =>
                {
                    sqlServer.EnableRetryOnFailure(databaseOptions.MaximumRetryCount);
                    sqlServer.CommandTimeout(databaseOptions.CommandTimeoutSeconds);
                })
                .AddInterceptors(serviceProvider.GetRequiredService<SlowQueryInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
        services.AddScoped<IServiceRequestReadRepository, ServiceRequestReadRepository>();
        services.AddScoped<IServiceRequestCodeGenerator, ServiceRequestCodeGenerator>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<IRequestTypeRepository, RequestTypeRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationReadRepository, NotificationReadRepository>();

        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();
        return services;
    }

    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SECTION_NAME)).ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        return services;
    }

    private static IServiceCollection AddNotificationDelivery(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NotificationOptions>().Bind(configuration.GetSection(NotificationOptions.SECTION_NAME));
        services.AddOptions<NotificationDispatchOptions>()
            .Bind(configuration.GetSection(NotificationDispatchOptions.SECTION_NAME))
            .PostConfigure<IHostEnvironment>((options, environment) =>
                options.EmailPickupDirectory = ResolvePath(options.EmailPickupDirectory, environment));

        services.AddSingleton<INotificationDeliveryChannel, InAppNotificationChannel>();
        services.AddSingleton<INotificationDeliveryChannel, SimulatedEmailNotificationChannel>();
        services.AddScoped<NotificationDispatcher>();
        return services;
    }

    private static IServiceCollection AddGovernmentEntityStores(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(GovernmentEntityStorageOptions.SECTION_NAME);
        services.AddOptions<GovernmentEntitiesOptions>().Bind(section);
        services.AddOptions<GovernmentEntityStorageOptions>()
            .Bind(section)
            .PostConfigure<IHostEnvironment>((options, environment) =>
            {
                options.TextFilePath = ResolvePath(options.TextFilePath, environment)!;
                options.ExcelFilePath = ResolvePath(options.ExcelFilePath, environment)!;
            });

        services.AddDbContext<GovernmentEntitiesDbContext>((serviceProvider, options) =>
            options.UseSqlite(ResolveSqliteConnectionString(
                RequiredConnectionString(serviceProvider, GovernmentEntityStorageOptions.SQLITE_CONNECTION_STRING_NAME),
                serviceProvider.GetRequiredService<IHostEnvironment>())));

        services.AddSingleton<TextFileGovernmentEntityStore>();
        services.AddKeyedSingleton<IGovernmentEntityReader>(GovernmentEntityDataSource.TextFile, (serviceProvider, _) => serviceProvider.GetRequiredService<TextFileGovernmentEntityStore>());
        services.AddKeyedSingleton<IGovernmentEntityWriter>(GovernmentEntityDataSource.TextFile, (serviceProvider, _) => serviceProvider.GetRequiredService<TextFileGovernmentEntityStore>());

        services.AddScoped<SqliteGovernmentEntityStore>();
        services.AddKeyedScoped<IGovernmentEntityReader>(GovernmentEntityDataSource.Sqlite, (serviceProvider, _) => serviceProvider.GetRequiredService<SqliteGovernmentEntityStore>());
        services.AddKeyedScoped<IGovernmentEntityWriter>(GovernmentEntityDataSource.Sqlite, (serviceProvider, _) => serviceProvider.GetRequiredService<SqliteGovernmentEntityStore>());

        services.AddKeyedSingleton<IGovernmentEntityReader, ExcelGovernmentEntityReader>(GovernmentEntityDataSource.Excel);

        services.AddScoped<IGovernmentEntityStoreFactory, GovernmentEntityStoreFactory>();
        return services;
    }

    // Se resuelve al crear el contexto (no al registrar) para respetar configuraciones aplicadas después, como en las pruebas.
    private static string RequiredConnectionString(IServiceProvider serviceProvider, string name)
        => serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' is not configured.");

    private static string? ResolvePath(string? path, IHostEnvironment environment)
        => string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, path));

    private static string ResolveSqliteConnectionString(string connectionString, IHostEnvironment environment)
    {
        SqliteConnectionStringBuilder builder = new(connectionString);
        string? dataSource = ResolvePath(builder.DataSource, environment);
        if (!string.IsNullOrWhiteSpace(dataSource))
        {
            string? directory = Path.GetDirectoryName(dataSource);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            builder.DataSource = dataSource;
        }

        return builder.ToString();
    }
}
