using SB.Solicitudes.Dominio.Notifications;

namespace SB.Solicitudes.Dominio.Tests.Notifications;

public sealed class NotificationTests
{
    private const string SUBJECT = "Solicitud registrada";
    private const string MESSAGE = "Su solicitud SOL-2026-0001 fue registrada.";
    private const string DELIVERY_ERROR = "SMTP unavailable";

    private static readonly DateTime CREATED_ON_UTC = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void MarkDeliveryFailedShouldKeepPendingUntilMaximumAttempts()
    {
        Notification notification = CreatePendingEmail();

        for (int attempt = 1; attempt < Notification.MAXIMUM_DELIVERY_ATTEMPTS; attempt++)
        {
            notification.MarkDeliveryFailed(DELIVERY_ERROR);
            notification.Status.ShouldBe(NotificationStatus.Pending);
        }

        notification.MarkDeliveryFailed(DELIVERY_ERROR);

        notification.Status.ShouldBe(NotificationStatus.Failed);
        notification.DeliveryAttempts.ShouldBe(Notification.MAXIMUM_DELIVERY_ATTEMPTS);
        notification.LastError.ShouldBe(DELIVERY_ERROR);
    }

    [Fact]
    public void MarkSentShouldClearLastError()
    {
        Notification notification = CreatePendingEmail();
        notification.MarkDeliveryFailed(DELIVERY_ERROR);

        notification.MarkSent(CREATED_ON_UTC);

        notification.Status.ShouldBe(NotificationStatus.Sent);
        notification.LastError.ShouldBeNull();
        notification.SentOnUtc.ShouldBe(CREATED_ON_UTC);
    }

    private static Notification CreatePendingEmail()
        => Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationChannel.Email,
            NotificationType.ServiceRequestCreated,
            SUBJECT,
            MESSAGE,
            CREATED_ON_UTC);
}
