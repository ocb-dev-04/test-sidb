using System.Reflection;
using NetArchTest.Rules;
using Shouldly;

namespace SB.Solicitudes.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string ENTITY_FRAMEWORK_NAMESPACE = "Microsoft.EntityFrameworkCore";
    private const string ASP_NET_CORE_NAMESPACE = "Microsoft.AspNetCore";

    public static TheoryData<string, string> ForbiddenDependencies => new()
    {
        { LayerAssemblies.DOMINIO, LayerAssemblies.APLICACION },
        { LayerAssemblies.DOMINIO, LayerAssemblies.INFRAESTRUCTURA },
        { LayerAssemblies.DOMINIO, LayerAssemblies.SERVICIOS },
        { LayerAssemblies.DOMINIO, ENTITY_FRAMEWORK_NAMESPACE },
        { LayerAssemblies.DOMINIO, ASP_NET_CORE_NAMESPACE },
        { LayerAssemblies.APLICACION, LayerAssemblies.INFRAESTRUCTURA },
        { LayerAssemblies.APLICACION, LayerAssemblies.SERVICIOS },
        { LayerAssemblies.APLICACION, ENTITY_FRAMEWORK_NAMESPACE },
        { LayerAssemblies.APLICACION, ASP_NET_CORE_NAMESPACE },
        { LayerAssemblies.INFRAESTRUCTURA, LayerAssemblies.SERVICIOS },
    };

    [Theory]
    [MemberData(nameof(ForbiddenDependencies))]
    public void LayerShouldNotDependOnForbiddenNamespace(string layerAssemblyName, string forbiddenNamespace)
    {
        TestResult result = Types
            .InAssembly(Assembly.Load(layerAssemblyName))
            .ShouldNot()
            .HaveDependencyOn(forbiddenNamespace)
            .GetResult();

        (result.FailingTypeNames ?? []).ShouldBeEmpty(
            $"{layerAssemblyName} must not depend on {forbiddenNamespace}");
    }
}
