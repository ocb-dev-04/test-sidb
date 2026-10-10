using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Dominio.Users;
using SB.Solicitudes.Infraestructura.Authentication;
using SB.Solicitudes.Infraestructura.Notifications;

namespace SB.Solicitudes.Infraestructura.Tests.Authentication;

public sealed class AuthenticationTests
{
    private const string PASSWORD = "Demo2026*";
    private const string WRONG_PASSWORD = "demo2026*";
    private const string SEEDED_HASH = "PBKDF2-SHA256.100000.+9YkaJuGlrd4t4HrEysALQ==.I5f5qXFwU9IHlmzqYETatOxi5vYzLgqQ+UJ3xrVoaK4=";
    private const string ISSUER = "SB.Solicitudes";
    private const string AUDIENCE = "SB.Solicitudes.Web";
    private const string SIGNING_KEY = "test-signing-key-with-at-least-32-characters";
    private const int EXPIRATION_MINUTES = 30;
    private const string FULL_NAME = "María Fernández";
    private const string EMAIL = "analista1@solicitudes.local";

    private static readonly DateTimeOffset NOW = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private readonly Pbkdf2PasswordHasher _passwordHasher = new();

    [Fact]
    public void HashShouldVerifyOnlyTheOriginalPasswordAndUseRandomSalt()
    {
        string firstHash = _passwordHasher.Hash(PASSWORD);
        string secondHash = _passwordHasher.Hash(PASSWORD);

        firstHash.ShouldNotBe(secondHash);
        _passwordHasher.Verify(PASSWORD, firstHash).ShouldBeTrue();
        _passwordHasher.Verify(WRONG_PASSWORD, firstHash).ShouldBeFalse();
    }

    [Fact]
    public void VerifyShouldAcceptSeededHashesAndRejectMalformedOnes()
    {
        _passwordHasher.Verify(PASSWORD, SEEDED_HASH).ShouldBeTrue();
        _passwordHasher.Verify(PASSWORD, "not-a-hash").ShouldBeFalse();
    }

    [Fact]
    public void TokenShouldCarrySpanishRoleNameAndExpireAfterConfiguredMinutes()
    {
        JwtTokenProvider provider = new(
            Options.Create(new JwtOptions { Issuer = ISSUER, Audience = AUDIENCE, SigningKey = SIGNING_KEY, ExpirationMinutes = EXPIRATION_MINUTES }),
            new FixedTimeProvider(NOW));
        User user = User.Create(FULL_NAME, EMAIL, SEEDED_HASH, UserRole.Analyst, NOW.UtcDateTime);

        AccessToken accessToken = provider.Create(user);
        JsonWebToken token = new(accessToken.Value);

        accessToken.ExpiresOnUtc.ShouldBe(NOW.UtcDateTime.AddMinutes(EXPIRATION_MINUTES));
        token.GetClaim(ApplicationClaimTypes.ROLE).Value.ShouldBe(RoleNames.ANALYST);
        token.GetClaim(ApplicationClaimTypes.USER_ID).Value.ShouldBe(user.Id.ToString());
        token.Issuer.ShouldBe(ISSUER);
    }

    [Fact]
    public void JwtOptionsValidatorShouldRejectShortSigningKeys()
    {
        JwtOptionsValidator validator = new();

        ValidateOptionsResult result = validator.Validate(null, new JwtOptions { Issuer = ISSUER, Audience = AUDIENCE, SigningKey = "short" });

        result.Failed.ShouldBeTrue();
    }

    [Theory]
    [InlineData("analista1@solicitudes.local", "a***@solicitudes.local")]
    [InlineData("invalid", "***")]
    public void MaskEmailShouldHideTheLocalPart(string email, string expected)
        => SimulatedEmailNotificationChannel.MaskEmail(email).ShouldBe(expected);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
