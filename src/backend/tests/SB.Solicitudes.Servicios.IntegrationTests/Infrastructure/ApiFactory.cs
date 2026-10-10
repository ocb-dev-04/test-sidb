using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SB.Solicitudes.Aplicacion.Features.Authentication;
using Testcontainers.MsSql;

namespace SB.Solicitudes.Servicios.IntegrationTests.Infrastructure;

/// <summary>
/// Levanta la API completa contra un SQL Server real en contenedor; las migraciones y la semilla se aplican al arrancar.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string DEMO_PASSWORD = "Demo2026*";
    public const string ADMINISTRATOR_EMAIL = "admin@solicitudes.local";
    public const string FIRST_ANALYST_EMAIL = "analista1@solicitudes.local";
    public const string FIRST_REQUESTER_EMAIL = "solicitante1@solicitudes.local";
    public const string SECOND_REQUESTER_EMAIL = "solicitante2@solicitudes.local";

    private const string SQL_SERVER_IMAGE = "mcr.microsoft.com/mssql/server:2022-latest";
    private const string TEXT_FILE_NAME = "entidades-gubernamentales.txt";
    private const string SQLITE_FILE_NAME = "entidades-gubernamentales.db";
    private const string EXCEL_FILE_NAME = "ListaEntidadesGubernamentales.xlsx";
    private const string TEST_LOGIN_PERMITS = "1000";

    public static readonly JsonSerializerOptions JSON_OPTIONS = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder(SQL_SERVER_IMAGE).Build();
    private readonly string _dataDirectory = Path.Combine(Path.GetTempPath(), "sb-solicitudes-tests", Guid.NewGuid().ToString("N"));

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_dataDirectory);
        string projectDataDirectory = Path.Combine(FindProjectDirectory(), "Data");
        File.Copy(Path.Combine(projectDataDirectory, TEXT_FILE_NAME), Path.Combine(_dataDirectory, TEXT_FILE_NAME));
        File.Copy(Path.Combine(projectDataDirectory, EXCEL_FILE_NAME), Path.Combine(_dataDirectory, EXCEL_FILE_NAME));

        await _sqlContainer.StartAsync();
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new LoginCommand(email, DEMO_PASSWORD), JSON_OPTIONS);
        response.EnsureSuccessStatusCode();

        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>(JSON_OPTIONS);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _sqlContainer.DisposeAsync();
        if (Directory.Exists(_dataDirectory))
        {
            Directory.Delete(_dataDirectory, recursive: true);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:SolicitudesDatabase"] = _sqlContainer.GetConnectionString(),
            ["ConnectionStrings:GovernmentEntitiesSqlite"] = $"Data Source={Path.Combine(_dataDirectory, SQLITE_FILE_NAME)}",
            ["Database:ApplyMigrationsOnStartup"] = "true",
            ["GovernmentEntities:TextFilePath"] = Path.Combine(_dataDirectory, TEXT_FILE_NAME),
            ["GovernmentEntities:ExcelFilePath"] = Path.Combine(_dataDirectory, EXCEL_FILE_NAME),
            ["Notifications:Dispatch:Enabled"] = "false",
            ["RateLimiting:LoginPermitsPerWindow"] = TEST_LOGIN_PERMITS,
            ["Notifications:Dispatch:EmailPickupDirectory"] = string.Empty,
        }));
    }

    private static string FindProjectDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "SB.Solicitudes.Servicios")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new DirectoryNotFoundException("Could not locate the SB.Solicitudes.Servicios project.")
            : Path.Combine(directory.FullName, "src", "SB.Solicitudes.Servicios");
    }
}

[CollectionDefinition(NAME)]
public sealed class ApiTestGroup : ICollectionFixture<ApiFactory>
{
    public const string NAME = "Api";
}
