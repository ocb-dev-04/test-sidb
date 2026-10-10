namespace SB.Solicitudes.Servicios.Common;

public static class AuthorizationPolicies
{
    public const string ADMINISTRATOR = "Administrator";
    public const string MANAGEMENT = "Management";
}

public static class RateLimitPolicies
{
    public const string LOGIN = "Login";
}

public sealed class RateLimitingOptions
{
    public const string SECTION_NAME = "RateLimiting";
    public const int DEFAULT_LOGIN_PERMITS_PER_WINDOW = 5;
    public const int DEFAULT_LOGIN_WINDOW_SECONDS = 60;

    public int LoginPermitsPerWindow { get; set; } = DEFAULT_LOGIN_PERMITS_PER_WINDOW;

    public int LoginWindowSeconds { get; set; } = DEFAULT_LOGIN_WINDOW_SECONDS;
}
