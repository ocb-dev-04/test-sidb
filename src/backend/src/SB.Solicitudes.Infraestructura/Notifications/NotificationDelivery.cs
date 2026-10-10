using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Dominio.Notifications;

namespace SB.Solicitudes.Infraestructura.Notifications;

public sealed record NotificationEnvelope(
    Guid NotificationId,
    string RecipientName,
    string RecipientEmail,
    string Subject,
    string Message,
    DateTime CreatedOnUtc);

/// <summary>
/// Punto de extensión: un canal nuevo (cola, SMS, Teams) es una clase más que implementa esta interfaz.
/// </summary>
public interface INotificationDeliveryChannel
{
    NotificationChannel Channel { get; }

    Task DeliverAsync(NotificationEnvelope envelope, CancellationToken cancellationToken);
}

internal sealed class InAppNotificationChannel : INotificationDeliveryChannel
{
    public NotificationChannel Channel => NotificationChannel.InApp;

    // La notificación en la aplicación ya quedó persistida; el usuario la consulta desde la bandeja.
    public Task DeliverAsync(NotificationEnvelope envelope, CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class SimulatedEmailNotificationChannel(
    IOptions<NotificationDispatchOptions> options,
    TimeProvider timeProvider,
    ILogger<SimulatedEmailNotificationChannel> logger)
    : INotificationDeliveryChannel
{
    private const string FILE_TIMESTAMP_FORMAT = "yyyyMMddHHmmssfff";
    private const string EMAIL_FILE_EXTENSION = ".eml";
    private const string SENDER_ADDRESS = "no-reply@solicitudes.local";
    private const char AT_SIGN = '@';
    private const string MASK = "***";

    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task DeliverAsync(NotificationEnvelope envelope, CancellationToken cancellationToken)
    {
        string? pickupDirectory = options.Value.EmailPickupDirectory;
        if (!string.IsNullOrWhiteSpace(pickupDirectory))
        {
            Directory.CreateDirectory(pickupDirectory);
            string fileName = string.Concat(
                timeProvider.GetUtcNow().ToString(FILE_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture),
                "-",
                envelope.NotificationId.ToString("N"),
                EMAIL_FILE_EXTENSION);

            string content = string.Join(
                Environment.NewLine,
                $"From: {SENDER_ADDRESS}",
                $"To: {envelope.RecipientName} <{envelope.RecipientEmail}>",
                $"Subject: {envelope.Subject}",
                "Content-Type: text/plain; charset=utf-8",
                string.Empty,
                envelope.Message);

            await File.WriteAllTextAsync(Path.Combine(pickupDirectory, fileName), content, cancellationToken);
        }

        InfrastructureLogMessages.SimulatedEmailSent(logger, envelope.Subject, MaskEmail(envelope.RecipientEmail));
    }

    // Nunca se registra el correo completo del destinatario.
    public static string MaskEmail(string email)
    {
        int atIndex = email.IndexOf(AT_SIGN, StringComparison.Ordinal);
        return atIndex <= 0 ? MASK : string.Concat(email.AsSpan(0, 1), MASK, email.AsSpan(atIndex));
    }
}
