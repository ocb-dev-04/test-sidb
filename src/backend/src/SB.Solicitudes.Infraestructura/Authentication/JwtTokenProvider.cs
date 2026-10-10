using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Infraestructura.Authentication;

internal sealed class JwtTokenProvider(IOptions<JwtOptions> options, TimeProvider timeProvider) : IJwtTokenProvider
{
    private static readonly JsonWebTokenHandler TOKEN_HANDLER = new();

    public AccessToken Create(User user)
    {
        JwtOptions jwtOptions = options.Value;
        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;
        DateTime expiresOnUtc = utcNow.AddMinutes(jwtOptions.ExpirationMinutes);

        SecurityTokenDescriptor descriptor = new()
        {
            Issuer = jwtOptions.Issuer,
            Audience = jwtOptions.Audience,
            IssuedAt = utcNow,
            NotBefore = utcNow,
            Expires = expiresOnUtc,
            SigningCredentials = CreateSigningCredentials(jwtOptions.SigningKey),
            Subject = new ClaimsIdentity(
            [
                new Claim(ApplicationClaimTypes.USER_ID, user.Id.ToString()),
                new Claim(ApplicationClaimTypes.EMAIL, user.Email),
                new Claim(ApplicationClaimTypes.NAME, user.FullName),
                new Claim(ApplicationClaimTypes.ROLE, RoleNames.From(user.Role)),
                new Claim(ApplicationClaimTypes.TOKEN_ID, Guid.NewGuid().ToString()),
            ]),
        };

        return new AccessToken(TOKEN_HANDLER.CreateToken(descriptor), expiresOnUtc);
    }

    public static SymmetricSecurityKey CreateSecurityKey(string signingKey) => new(Encoding.UTF8.GetBytes(signingKey));

    private static SigningCredentials CreateSigningCredentials(string signingKey)
        => new(CreateSecurityKey(signingKey), SecurityAlgorithms.HmacSha256);
}
