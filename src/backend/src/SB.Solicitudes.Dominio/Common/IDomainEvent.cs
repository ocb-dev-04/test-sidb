namespace SB.Solicitudes.Dominio.Common;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
