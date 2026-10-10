using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Servicios.Common;

/// <summary>
/// Último recurso: los errores esperados viajan como Result; aquí solo llegan fallas técnicas.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int statusCode, string title, string? code) = exception switch
        {
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict", ServiceRequestErrors.CONCURRENCY_CONFLICT.Code),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Bad request", null),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error", null),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            ApiLogMessages.UnhandledException(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            ApiLogMessages.HandledException(logger, exception.GetType().Name, statusCode);
        }

        ProblemDetails problemDetails = new()
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status409Conflict
                ? ServiceRequestErrors.CONCURRENCY_CONFLICT.Description
                : null,
        };
        if (code is not null)
        {
            problemDetails.Extensions[ApiControllerBase.ERROR_CODE_EXTENSION] = code;
        }

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        });
    }
}

internal static partial class ApiLogMessages
{
    [LoggerMessage(EventId = 3000, Level = LogLevel.Error, Message = "Unhandled exception processing {Method} {Path}")]
    public static partial void UnhandledException(ILogger logger, Exception exception, string method, string path);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Warning, Message = "{ExceptionType} translated to HTTP {StatusCode}")]
    public static partial void HandledException(ILogger logger, string exceptionType, int statusCode);
}
