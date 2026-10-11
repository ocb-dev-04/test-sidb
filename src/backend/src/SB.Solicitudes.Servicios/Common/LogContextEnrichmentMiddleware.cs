using Serilog.Context;
using SB.Solicitudes.Infraestructura.Authentication;

namespace SB.Solicitudes.Servicios.Common;

/// <summary>
/// Agrega a cada log de la petición el usuario; nunca el token ni datos personales. El TraceId lo captura Serilog.
/// </summary>
internal sealed class LogContextEnrichmentMiddleware(RequestDelegate next)
{
    private const string USER_ID_PROPERTY = "UserId";
    private const string ANONYMOUS = "anonymous";

    public async Task InvokeAsync(HttpContext httpContext)
    {
        string userId = httpContext.User.FindFirst(ApplicationClaimTypes.USER_ID)?.Value ?? ANONYMOUS;

        using (LogContext.PushProperty(USER_ID_PROPERTY, userId))
        {
            await next(httpContext);
        }
    }
}
