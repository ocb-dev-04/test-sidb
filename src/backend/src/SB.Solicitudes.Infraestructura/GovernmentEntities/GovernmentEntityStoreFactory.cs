using Microsoft.Extensions.DependencyInjection;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Infraestructura.GovernmentEntities;

/// <summary>
/// Factory sobre keyed services de .NET 8: agregar una fuente es registrar una implementación con su clave.
/// </summary>
internal sealed class GovernmentEntityStoreFactory(IServiceProvider serviceProvider) : IGovernmentEntityStoreFactory
{
    public bool IsWritable(GovernmentEntityDataSource source)
        => serviceProvider.GetKeyedService<IGovernmentEntityWriter>(source) is not null;

    public Result<IGovernmentEntityReader> GetReader(GovernmentEntityDataSource source)
        => serviceProvider.GetKeyedService<IGovernmentEntityReader>(source) is { } reader
            ? Result.Success(reader)
            : Result.Failure<IGovernmentEntityReader>(GovernmentEntityErrors.SOURCE_NOT_SUPPORTED);

    public Result<IGovernmentEntityWriter> GetWriter(GovernmentEntityDataSource source)
    {
        if (serviceProvider.GetKeyedService<IGovernmentEntityWriter>(source) is { } writer)
        {
            return Result.Success(writer);
        }

        Error error = serviceProvider.GetKeyedService<IGovernmentEntityReader>(source) is null
            ? GovernmentEntityErrors.SOURCE_NOT_SUPPORTED
            : GovernmentEntityErrors.SOURCE_READ_ONLY;
        return Result.Failure<IGovernmentEntityWriter>(error);
    }
}
