using System.Reflection;

namespace SB.Solicitudes.ArchitectureTests;

internal static class LayerAssemblies
{
    public const string ROOT_NAMESPACE = "SB.Solicitudes";
    public const string DOMINIO = "SB.Solicitudes.Dominio";
    public const string APLICACION = "SB.Solicitudes.Aplicacion";
    public const string INFRAESTRUCTURA = "SB.Solicitudes.Infraestructura";
    public const string SERVICIOS = "SB.Solicitudes.Servicios";

    public static IReadOnlyList<Assembly> All =>
    [
        Assembly.Load(DOMINIO),
        Assembly.Load(APLICACION),
        Assembly.Load(INFRAESTRUCTURA),
        Assembly.Load(SERVICIOS),
    ];
}
