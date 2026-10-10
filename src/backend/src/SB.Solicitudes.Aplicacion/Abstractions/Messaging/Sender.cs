using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Aplicacion.Abstractions.Messaging;

internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, object> DISPATCHERS = new();

    public Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        RequestDispatcher<TResponse> dispatcher = (RequestDispatcher<TResponse>)DISPATCHERS.GetOrAdd(
            request.GetType(),
            requestType => Activator.CreateInstance(
                typeof(RequestDispatcher<,>).MakeGenericType(requestType, typeof(TResponse)))!);

        return dispatcher.DispatchAsync(request, serviceProvider, cancellationToken);
    }
}

internal abstract class RequestDispatcher<TResponse>
{
    public abstract Task<Result<TResponse>> DispatchAsync(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal sealed class RequestDispatcher<TRequest, TResponse> : RequestDispatcher<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<Result<TResponse>> DispatchAsync(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        TRequest typedRequest = (TRequest)request;
        IRequestHandler<TRequest, TResponse> handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        RequestPipelineNext<TResponse> pipeline = () => handler.HandleAsync(typedRequest, cancellationToken);
        foreach (IPipelineBehavior<TRequest, TResponse> behavior in serviceProvider
                     .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                     .Reverse())
        {
            RequestPipelineNext<TResponse> next = pipeline;
            pipeline = () => behavior.HandleAsync(typedRequest, next, cancellationToken);
        }

        return pipeline();
    }
}
