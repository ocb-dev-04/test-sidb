using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.ServiceRequests;

public static class ServiceRequestErrors
{
    public static readonly Error NOT_FOUND = Error.NotFound("serviceRequest.notFound", "The service request was not found.");

    public static readonly Error CLOSED_CANNOT_BE_MODIFIED = Error.Conflict("serviceRequest.closed", "A closed service request cannot be modified.");

    public static readonly Error SAME_STATUS = Error.Validation("serviceRequest.sameStatus", "The service request already has that status.");

    public static readonly Error COMMENT_REQUIRED = Error.Validation("serviceRequest.commentRequired", "A comment is required to resolve, close or reopen a service request.");

    public static readonly Error REOPEN_NOT_ALLOWED = Error.Forbidden("serviceRequest.reopenNotAllowed", "Only administrators and analysts can reopen a closed service request.");

    public static readonly Error ALREADY_ASSIGNED = Error.Validation("serviceRequest.alreadyAssigned", "The service request is already assigned to that user.");

    public static readonly Error INVALID_ASSIGNEE = Error.Validation("serviceRequest.invalidAssignee", "The assignee must be an active administrator or analyst.");

    public static readonly Error EDIT_NOT_ALLOWED = Error.Forbidden("serviceRequest.editNotAllowed", "Requesters can only edit their own service requests while they are registered.");

    public static readonly Error INTERNAL_COMMENT_NOT_ALLOWED = Error.Forbidden("serviceRequest.internalCommentNotAllowed", "Requesters cannot add internal comments.");

    public static readonly Error MANAGEMENT_REQUIRED = Error.Forbidden("serviceRequest.managementRequired", "Only administrators and analysts can manage service requests.");

    public static readonly Error CONCURRENCY_CONFLICT = Error.Conflict("serviceRequest.concurrencyConflict", "The service request was modified by another user. Reload and try again.");

    public static Error InvalidTransition(ServiceRequestStatus from, ServiceRequestStatus to)
        => Error.Validation("serviceRequest.invalidTransition", $"Transition from {from} to {to} is not allowed.");
}
