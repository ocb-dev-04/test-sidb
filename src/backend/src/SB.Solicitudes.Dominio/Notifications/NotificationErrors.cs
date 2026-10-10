using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.Notifications;

public static class NotificationErrors
{
    public static readonly Error NOT_FOUND = Error.NotFound("notification.notFound", "The notification was not found.");
}
