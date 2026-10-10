using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SB.Solicitudes.Infraestructura.Persistence;

internal sealed class SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger, IOptions<DatabaseOptions> options)
    : DbCommandInterceptor
{
    private const int MAXIMUM_LOGGED_SQL_LENGTH = 500;

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    // Solo se registra el texto SQL, nunca los valores de los parámetros (pueden contener datos personales).
    private void LogIfSlow(DbCommand command, CommandExecutedEventData eventData)
    {
        if (eventData.Duration.TotalMilliseconds < options.Value.SlowQueryThresholdMilliseconds)
        {
            return;
        }

        string sql = command.CommandText.Length > MAXIMUM_LOGGED_SQL_LENGTH
            ? command.CommandText[..MAXIMUM_LOGGED_SQL_LENGTH]
            : command.CommandText;

        InfrastructureLogMessages.SlowQuery(logger, eventData.Duration.TotalMilliseconds, sql);
    }
}
