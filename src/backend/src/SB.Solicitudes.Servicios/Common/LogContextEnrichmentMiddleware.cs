using System.Diagnostics;
using Serilog.Context;
using SB.Solicitudes.Infraestructura.Authentication;

namespace SB.Solicitudes.Servicios.Common;

/// <summary>
/// Agrega a cada log de la petición el usuario y el traceId; nunca el token ni datos personales.
/// </summary>
internal sealed class LogContextEnrichmentMiddleware(RequestDelegate next)
{
    private const string USER_ID_PROPERTY = "UserId";
    private const string TRACE_ID_PROPERTY = "TraceId";
    private const string ANONYMOUS = "anonymous";

    public async Task InvokeAsync(HttpContext httpContext)
    {
        string userId = httpContext.User.FindFirst(ApplicationClaimTypes.USER_ID)?.Value ?? ANONYMOUS;
        string traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        using (LogContext.PushProperty(USER_ID_PROPERTY, userId))
        using (LogContext.PushProperty(TRACE_ID_PROPERTY, traceId))
        {
            await next(httpContext);
        }
    }
}
