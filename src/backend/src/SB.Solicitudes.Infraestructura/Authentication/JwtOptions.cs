using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SB.Solicitudes.Infraestructura.Authentication;

public sealed class JwtOptions
{
    public const string SECTION_NAME = "Jwt";
    public const int MINIMUM_SIGNING_KEY_LENGTH = 32;
    public const int DEFAULT_EXPIRATION_MINUTES = 60;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = DEFAULT_EXPIRATION_MINUTES;
}

internal sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Jwt:Audience is required.");
        }

        if (options.SigningKey.Length < JwtOptions.MINIMUM_SIGNING_KEY_LENGTH)
        {
            failures.Add($"Jwt:SigningKey must have at least {JwtOptions.MINIMUM_SIGNING_KEY_LENGTH} characters.");
        }

        if (options.ExpirationMinutes <= 0)
        {
            failures.Add("Jwt:ExpirationMinutes must be positive.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}

public static class JwtSigningKey
{
    public static SymmetricSecurityKey Create(string signingKey) => new(Encoding.UTF8.GetBytes(signingKey));
}

public static class ApplicationClaimTypes
{
    public const string USER_ID = "sub";
    public const string EMAIL = "email";
    public const string NAME = "name";
    public const string ROLE = "role";
    public const string TOKEN_ID = "jti";
}
