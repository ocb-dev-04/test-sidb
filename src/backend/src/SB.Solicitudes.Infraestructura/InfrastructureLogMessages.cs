using Microsoft.Extensions.Logging;
using SB.Solicitudes.Dominio.Notifications;

namespace SB.Solicitudes.Infraestructura;

internal static partial class InfrastructureLogMessages
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Warning, Message = "Slow database command ({ElapsedMilliseconds:0} ms): {CommandText}")]
    public static partial void SlowQuery(ILogger logger, double elapsedMilliseconds, string commandText);

    [LoggerMessage(EventId = 2100, Level = LogLevel.Information, Message = "Simulated email '{Subject}' delivered to {MaskedRecipient}")]
    public static partial void SimulatedEmailSent(ILogger logger, string subject, string maskedRecipient);

    [LoggerMessage(EventId = 2101, Level = LogLevel.Warning, Message = "Notification {NotificationId} delivery through {Channel} failed")]
    public static partial void NotificationDeliveryFailed(ILogger logger, Exception exception, Guid notificationId, NotificationChannel channel);

    [LoggerMessage(EventId = 2102, Level = LogLevel.Information, Message = "{DeliveredCount} of {PendingCount} pending notifications delivered")]
    public static partial void NotificationsDispatched(ILogger logger, int deliveredCount, int pendingCount);

    [LoggerMessage(EventId = 2103, Level = LogLevel.Error, Message = "Notification dispatch cycle failed")]
    public static partial void NotificationDispatchCycleFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2200, Level = LogLevel.Information, Message = "Applying pending database migrations")]
    public static partial void ApplyingMigrations(ILogger logger);

    [LoggerMessage(EventId = 2300, Level = LogLevel.Information, Message = "Government entities store {Store} initialized with {Count} records from Excel")]
    public static partial void GovernmentEntitiesStoreSeeded(ILogger logger, string store, int count);

    [LoggerMessage(EventId = 2301, Level = LogLevel.Warning, Message = "Government entities store initialization skipped: {Reason}")]
    public static partial void GovernmentEntitiesInitializationSkipped(ILogger logger, string reason);
}
