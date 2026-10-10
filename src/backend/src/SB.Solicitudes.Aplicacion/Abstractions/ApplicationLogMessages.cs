using Microsoft.Extensions.Logging;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Aplicacion.Abstractions;

internal static partial class ApplicationLogMessages
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Request {RequestName} handled in {ElapsedMilliseconds:0.0} ms")]
    public static partial void RequestSucceeded(ILogger logger, string requestName, double elapsedMilliseconds);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "Request {RequestName} failed with {ErrorCode} ({ErrorType}) in {ElapsedMilliseconds:0.0} ms")]
    public static partial void RequestFailed(ILogger logger, string requestName, string errorCode, ErrorType errorType, double elapsedMilliseconds);

    [LoggerMessage(EventId = 1100, Level = LogLevel.Information, Message = "Service request {ServiceRequestCode} created by user {UserId}")]
    public static partial void ServiceRequestCreated(ILogger logger, string serviceRequestCode, Guid userId);

    [LoggerMessage(EventId = 1101, Level = LogLevel.Information, Message = "Service request {ServiceRequestId} moved to {NewStatus} by user {UserId}")]
    public static partial void ServiceRequestStatusChanged(ILogger logger, Guid serviceRequestId, ServiceRequestStatus newStatus, Guid userId);

    [LoggerMessage(EventId = 1102, Level = LogLevel.Information, Message = "Service request {ServiceRequestId} assigned to user {AssigneeId} by user {UserId}")]
    public static partial void ServiceRequestAssigned(ILogger logger, Guid serviceRequestId, Guid assigneeId, Guid userId);

    [LoggerMessage(EventId = 1200, Level = LogLevel.Information, Message = "{NotificationCount} {NotificationType} notifications queued for service request {ServiceRequestId}")]
    public static partial void NotificationsQueued(ILogger logger, int notificationCount, NotificationType notificationType, Guid serviceRequestId);

    [LoggerMessage(EventId = 1300, Level = LogLevel.Warning, Message = "Failed login attempt")]
    public static partial void LoginFailed(ILogger logger);

    [LoggerMessage(EventId = 1301, Level = LogLevel.Information, Message = "User {UserId} signed in")]
    public static partial void LoginSucceeded(ILogger logger, Guid userId);

    [LoggerMessage(EventId = 1400, Level = LogLevel.Information, Message = "{ImportedCount} government entities imported from {Source} into {Destination}")]
    public static partial void GovernmentEntitiesImported(ILogger logger, int importedCount, string source, string destination);
}
