namespace SB.Solicitudes.Infraestructura.Persistence;

public sealed class DatabaseOptions
{
    public const string SECTION_NAME = "Database";
    public const string CONNECTION_STRING_NAME = "SolicitudesDatabase";

    public const int DEFAULT_COMMAND_TIMEOUT_SECONDS = 30;
    public const int DEFAULT_MAXIMUM_RETRY_COUNT = 3;
    public const int DEFAULT_SLOW_QUERY_THRESHOLD_MILLISECONDS = 500;

    public bool ApplyMigrationsOnStartup { get; set; }

    public int CommandTimeoutSeconds { get; set; } = DEFAULT_COMMAND_TIMEOUT_SECONDS;

    public int MaximumRetryCount { get; set; } = DEFAULT_MAXIMUM_RETRY_COUNT;

    public int SlowQueryThresholdMilliseconds { get; set; } = DEFAULT_SLOW_QUERY_THRESHOLD_MILLISECONDS;
}
