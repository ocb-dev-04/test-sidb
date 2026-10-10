using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NetArchTest.Rules;
using Shouldly;

namespace SB.Solicitudes.ArchitectureTests;

public sealed class NamingConventionTests
{
    private const string INTERFACE_PREFIX = "I";
    private const BindingFlags STATIC_FIELDS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly Regex UPPER_CASE_PATTERN = new("^[A-Z][A-Z0-9]*(_[A-Z0-9]+)*$", RegexOptions.Compiled);

    [Fact]
    public void InterfacesShouldStartWithPrefix()
    {
        TestResult result = Types
            .InAssemblies(LayerAssemblies.All)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith(INTERFACE_PREFIX)
            .GetResult();

        (result.FailingTypeNames ?? []).ShouldBeEmpty();
    }

    [Fact]
    public void ConstantsShouldBeUpperCase()
    {
        string[] violations = LayerAssemblies.All
            .SelectMany(assembly => assembly.GetTypes())
            .Where(IsHandWrittenType)
            .SelectMany(type => type.GetFields(STATIC_FIELDS))
            .Where(field => field.IsLiteral || field.IsInitOnly)
            .Where(field => !field.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .Where(field => !UPPER_CASE_PATTERN.IsMatch(field.Name))
            .Select(field => $"{field.DeclaringType!.FullName}.{field.Name}")
            .ToArray();

        violations.ShouldBeEmpty();
    }

    private static bool IsHandWrittenType(Type type)
        => !type.IsEnum
            && type.Namespace is not null
            && type.Namespace.StartsWith(LayerAssemblies.ROOT_NAMESPACE, StringComparison.Ordinal)
            && !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
            && !type.IsDefined(typeof(GeneratedCodeAttribute), inherit: false);
}
