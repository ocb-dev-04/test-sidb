using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Aplicacion.Abstractions.Messaging;

#pragma warning disable CA1040 // Interfaces marcadoras: distinguen comandos de consultas en el pipeline
public interface IRequest<TResponse>;

public interface ICommand<TResponse> : IRequest<TResponse>;

public interface IQuery<TResponse> : IRequest<TResponse>;
#pragma warning restore CA1040

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;

public delegate Task<Result<TResponse>> RequestPipelineNext<TResponse>();

public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> HandleAsync(
        TRequest request,
        RequestPipelineNext<TResponse> next,
        CancellationToken cancellationToken);
}

public interface ISender
{
    Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

public readonly record struct Unit
{
    public static readonly Unit VALUE;
}
