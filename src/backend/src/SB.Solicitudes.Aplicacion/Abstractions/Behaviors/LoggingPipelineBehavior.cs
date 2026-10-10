using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Aplicacion.Abstractions.Behaviors;

internal sealed class LoggingPipelineBehavior<TRequest, TResponse>(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<Result<TResponse>> HandleAsync(
        TRequest request,
        RequestPipelineNext<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        long startTimestamp = Stopwatch.GetTimestamp();

        Result<TResponse> result = await next();

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
