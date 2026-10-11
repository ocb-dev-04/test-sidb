using MediatR;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Aplicacion.Abstractions.Messaging;

#pragma warning disable CA1040 // Interfaces marcadoras: distinguen comandos de consultas sobre MediatR
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
#pragma warning restore CA1040

// MediatR exige `Handle`; se expone `HandleAsync` para respetar el sufijo Async de la especificación.
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TCommand request, CancellationToken cancellationToken);

    Task<Result<TResponse>> IRequestHandler<TCommand, Result<TResponse>>.Handle(TCommand request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery request, CancellationToken cancellationToken);

    Task<Result<TResponse>> IRequestHandler<TQuery, Result<TResponse>>.Handle(TQuery request, CancellationToken cancellationToken)
        => HandleAsync(request, cancellationToken);
}
