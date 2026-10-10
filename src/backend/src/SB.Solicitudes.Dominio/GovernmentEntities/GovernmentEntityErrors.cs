using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.GovernmentEntities;

public static class GovernmentEntityErrors
{
    public static readonly Error NOT_FOUND = Error.NotFound("governmentEntity.notFound", "The government entity was not found.");

    public static readonly Error NAME_IN_USE = Error.Conflict("governmentEntity.nameInUse", "Another government entity already uses that name.");

    public static readonly Error SOURCE_READ_ONLY = Error.Validation("governmentEntity.sourceReadOnly", "The selected data source is read-only.");

    public static readonly Error SOURCE_NOT_SUPPORTED = Error.Validation("governmentEntity.sourceNotSupported", "The selected data source is not supported.");

    public static readonly Error SOURCE_AND_DESTINATION_EQUAL = Error.Validation("governmentEntity.sameSourceAndDestination", "The import source and destination must be different.");
}
