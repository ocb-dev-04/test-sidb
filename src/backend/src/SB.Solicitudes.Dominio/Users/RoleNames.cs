namespace SB.Solicitudes.Dominio.Users;

public static class RoleNames
{
    public const string ADMINISTRATOR = "Administrador";
    public const string ANALYST = "Analista";
    public const string REQUESTER = "Solicitante";
    public const string MANAGEMENT = ADMINISTRATOR + "," + ANALYST;

    public static string From(UserRole role) => role switch
    {
        UserRole.Administrator => ADMINISTRATOR,
        UserRole.Analyst => ANALYST,
        UserRole.Requester => REQUESTER,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown role."),
    };

    public static bool TryParse(string? roleName, out UserRole role)
    {
        role = roleName switch
        {
            ADMINISTRATOR => UserRole.Administrator,
            ANALYST => UserRole.Analyst,
            REQUESTER => UserRole.Requester,
            _ => default,
        };

        return role != default;
    }
}
