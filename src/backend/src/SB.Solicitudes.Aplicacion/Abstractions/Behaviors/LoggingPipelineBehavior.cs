using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Aplicacion.Abstractions.Behaviors;

internal sealed class LoggingPipelineBehavior<TRequest, TResponse>(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        long startTimestamp = Stopwatch.GetTimestamp();

        TResponse result = await next(cancellationToken);

        double elapsedMilliseconds = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
        if (result.IsSuccess)
        {
            ApplicationLogMessages.RequestSucceeded(logger, requestName, elapsedMilliseconds);
        }
        else
        {
            ApplicationLogMessages.RequestFailed(logger, requestName, result.Error.Code, result.Error.Type, elapsedMilliseconds);
        }

        return result;
    }
}
