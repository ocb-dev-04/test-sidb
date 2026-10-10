using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Infraestructura.Persistence;

namespace SB.Solicitudes.Infraestructura.Notifications;

public sealed class NotificationDispatchOptions
{
    public const string SECTION_NAME = "Notifications:Dispatch";
    public const int DEFAULT_POLLING_INTERVAL_SECONDS = 5;
    public const int DEFAULT_BATCH_SIZE = 20;

    public bool Enabled { get; set; } = true;

    public int PollingIntervalSeconds { get; set; } = DEFAULT_POLLING_INTERVAL_SECONDS;

    public int BatchSize { get; set; } = DEFAULT_BATCH_SIZE;

    public string? EmailPickupDirectory { get; set; }
}

internal sealed class NotificationDispatcher(
    ApplicationDbContext context,
    IEnumerable<INotificationDeliveryChannel> channels,
    IOptions<NotificationDispatchOptions> options,
    TimeProvider timeProvider,
    ILogger<NotificationDispatcher> logger)
{
    public async Task<int> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        List<PendingNotification> pending = await context.Notifications
            .Where(notification => notification.Status == NotificationStatus.Pending)
            .OrderBy(notification => notification.CreatedOnUtc)
            .Take(options.Value.BatchSize)
            .Join(
                context.Users,
                notification => notification.RecipientUserId,
                user => user.Id,
                (notification, user) => new PendingNotification(notification, user.FullName, user.Email))
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return 0;
        }

        Dictionary<NotificationChannel, INotificationDeliveryChannel> channelsByType = channels.ToDictionary(channel => channel.Channel);
        int deliveredCount = 0;

        foreach (PendingNotification item in pending)
        {
            Notification notification = item.Notification;
            try
            {
                if (!channelsByType.TryGetValue(notification.Channel, out INotificationDeliveryChannel? channel))
                {
                    throw new InvalidOperationException($"No delivery channel registered for {notification.Channel}.");
                }

                NotificationEnvelope envelope = new(
                    notification.Id, item.FullName, item.Email, notification.Subject, notification.Message, notification.CreatedOnUtc);
                await channel.DeliverAsync(envelope, cancellationToken);

                notification.MarkSent(timeProvider.GetUtcNow().UtcDateTime);
                deliveredCount++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                InfrastructureLogMessages.NotificationDeliveryFailed(logger, exception, notification.Id, notification.Channel);
                notification.MarkDeliveryFailed(exception.Message);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        InfrastructureLogMessages.NotificationsDispatched(logger, deliveredCount, pending.Count);
        return deliveredCount;
    }

    private sealed record PendingNotification(Notification Notification, string FullName, string Email);
}

/// <summary>
/// Patrón outbox simplificado: las notificaciones se guardan junto al cambio de negocio y este worker las entrega después.
/// </summary>
internal sealed class NotificationDispatchWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<NotificationDispatchOptions> options,
    ILogger<NotificationDispatchWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(options.Value.PollingIntervalSeconds));
        do
        {
            try
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                NotificationDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<NotificationDispatcher>();
                await dispatcher.DispatchPendingAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                InfrastructureLogMessages.NotificationDispatchCycleFailed(logger, exception);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
