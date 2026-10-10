namespace SB.Solicitudes.Dominio.Notifications;

public enum NotificationChannel
{
    InApp = 1,
    Email = 2,
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
}

public enum NotificationType
{
    ServiceRequestCreated = 1,
    ServiceRequestAssigned = 2,
    ServiceRequestStatusChanged = 3,
    ServiceRequestClosed = 4,
    ServiceRequestCommented = 5,
}
