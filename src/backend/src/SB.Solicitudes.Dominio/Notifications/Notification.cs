using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.Notifications;

public sealed class Notification : Entity<Guid>
{
    public const int SUBJECT_MAXIMUM_LENGTH = 200;
    public const int MESSAGE_MAXIMUM_LENGTH = 2000;
    public const int LAST_ERROR_MAXIMUM_LENGTH = 1000;
    public const int MAXIMUM_DELIVERY_ATTEMPTS = 3;

    private Notification()
    {
    }

    private Notification(Guid id)
        : base(id)
    {
    }

    public Guid RecipientUserId { get; private set; }

    public Guid? ServiceRequestId { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public NotificationType Type { get; private set; }

    public string Subject { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public NotificationStatus Status { get; private set; }

    public int DeliveryAttempts { get; private set; }

    public string? LastError { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime? SentOnUtc { get; private set; }

    public DateTime? ReadOnUtc { get; private set; }

    public static Notification Create(
        Guid recipientUserId,
        Guid? serviceRequestId,
        NotificationChannel channel,
        NotificationType type,
        string subject,
        string message,
        DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Notification(Guid.NewGuid())
        {
            RecipientUserId = recipientUserId,
            ServiceRequestId = serviceRequestId,
            Channel = channel,
            Type = type,
            Subject = subject,
            Message = message,
            Status = NotificationStatus.Pending,
            CreatedOnUtc = utcNow,
        };
    }

    public void MarkSent(DateTime utcNow)
    {
        Status = NotificationStatus.Sent;
        SentOnUtc = utcNow;
        DeliveryAttempts++;
        LastError = null;
    }

    public void MarkDeliveryFailed(string error)
    {
        DeliveryAttempts++;
        LastError = error.Length > LAST_ERROR_MAXIMUM_LENGTH ? error[..LAST_ERROR_MAXIMUM_LENGTH] : error;
        Status = DeliveryAttempts >= MAXIMUM_DELIVERY_ATTEMPTS ? NotificationStatus.Failed : NotificationStatus.Pending;
    }

    public void MarkAsRead(DateTime utcNow) => ReadOnUtc ??= utcNow;
}
