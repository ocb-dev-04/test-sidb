using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.Users;

public static class UserErrors
{
    public static readonly Error NOT_FOUND = Error.NotFound("user.notFound", "The user was not found.");

    public static readonly Error EMAIL_ALREADY_IN_USE = Error.Conflict("user.emailAlreadyInUse", "The email is already registered.");

    public static readonly Error INVALID_CREDENTIALS = Error.Unauthorized("user.invalidCredentials", "The email or password is incorrect.");

    public static readonly Error INACTIVE = Error.Forbidden("user.inactive", "The user is inactive.");

    public static readonly Error CANNOT_DEACTIVATE_SELF = Error.Conflict("user.cannotDeactivateSelf", "Administrators cannot deactivate or demote themselves.");
}
