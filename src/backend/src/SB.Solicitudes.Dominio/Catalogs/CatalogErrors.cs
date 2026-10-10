using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.Catalogs;

public static class CatalogErrors
{
    public static readonly Error AREA_NOT_FOUND = Error.NotFound("area.notFound", "The area was not found.");

    public static readonly Error AREA_INACTIVE = Error.Validation("area.inactive", "The area is inactive.");

    public static readonly Error AREA_NAME_IN_USE = Error.Conflict("area.nameInUse", "Another area already uses that name.");

    public static readonly Error REQUEST_TYPE_NOT_FOUND = Error.NotFound("requestType.notFound", "The request type was not found.");

    public static readonly Error REQUEST_TYPE_INACTIVE = Error.Validation("requestType.inactive", "The request type is inactive.");

    public static readonly Error REQUEST_TYPE_NAME_IN_USE = Error.Conflict("requestType.nameInUse", "Another request type already uses that name.");
}
