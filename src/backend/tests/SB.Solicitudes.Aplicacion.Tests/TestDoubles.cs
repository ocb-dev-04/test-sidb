using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Tests;

internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}

internal sealed class TestCurrentUser(Guid userId, UserRole role) : ICurrentUser
{
    public bool IsAuthenticated => true;

    public Guid UserId { get; } = userId;

    public UserRole Role { get; } = role;
}

internal static class TestData
{
    public const string CODE = "SOL-2026-0001";
    public const string TITLE = "Acceso a VPN";
    public const string DESCRIPTION = "No puedo conectarme a la VPN institucional.";
    public const string COMMENT = "Comentario de prueba";
    public const int AREA_ID = 1;
    public const int REQUEST_TYPE_ID = 1;

    public static readonly DateTime UTC_NOW = new(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc);
    public static readonly Guid REQUESTER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ANALYST_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OTHER_ANALYST_ID = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ADMINISTRATOR_ID = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static TimeProvider Clock => new FixedTimeProvider(new DateTimeOffset(UTC_NOW));

    public static ServiceRequest NewServiceRequest(Guid? requesterId = null)
        => ServiceRequest.Create(
            CODE,
            TITLE,
            DESCRIPTION,
            ServiceRequestPriority.Medium,
            REQUEST_TYPE_ID,
            AREA_ID,
            requesterId ?? REQUESTER_ID,
            null,
            UTC_NOW);
}
