using System.Text.Json.Nodes;
using System.Reflection;
using Dynamic.Json.EfCore.Querying;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.UnitTests.Base.Querying;

public class DynamicJsonScalarMethodsTests
{
    [Fact]
    public void Descriptors_IdentifyEveryPublicScalarMarkerMethod()
    {
        DynamicJsonScalarMethods.Value.Should().BeSameAs(GetMethod(nameof(DynamicJsonFunctions.Value)));
        DynamicJsonScalarMethods.ValueDecimal.Should().BeSameAs(GetMethod(nameof(DynamicJsonFunctions.ValueDecimal)));
        DynamicJsonScalarMethods.ValueDate.Should().BeSameAs(GetMethod(nameof(DynamicJsonFunctions.ValueDate)));
    }

    [Fact]
    public void Descriptors_PreserveExpectedPublicMethodSignatures()
    {
        AssertDescriptor(DynamicJsonScalarMethods.Value, typeof(string));
        AssertDescriptor(DynamicJsonScalarMethods.ValueDecimal, typeof(decimal?));
        AssertDescriptor(DynamicJsonScalarMethods.ValueDate, typeof(DateOnly?));
    }

    [Fact]
    public void Descriptors_RepresentEveryPublicScalarMarkerMethod()
    {
        MethodInfo[] markerMethods = typeof(DynamicJsonFunctions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        MethodInfo[] descriptors = typeof(DynamicJsonScalarMethods)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(MethodInfo))
            .Select(property => (MethodInfo)property.GetValue(null)!)
            .ToArray();

        descriptors.Should().BeEquivalentTo(markerMethods);
    }

    [Fact]
    public void CoreAssembly_DoesNotReferenceDatabaseProviderOrRelationalAssemblies()
    {
        string[] references = typeof(DynamicJsonFunctions).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .ToArray();

        references.Should().NotContain(name =>
            name.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) ||
            name == "Microsoft.EntityFrameworkCore.Relational");
    }

    private static System.Reflection.MethodInfo GetMethod(string name)
        => typeof(DynamicJsonFunctions).GetMethod(name, [typeof(JsonObject), typeof(string)])!;

    private static void AssertDescriptor(MethodInfo descriptor, Type returnType)
    {
        descriptor.DeclaringType.Should().Be(typeof(DynamicJsonFunctions));
        descriptor.IsPublic.Should().BeTrue();
        descriptor.IsStatic.Should().BeTrue();
        descriptor.ReturnType.Should().Be(returnType);
        descriptor.GetParameters()
            .Select(parameter => parameter.ParameterType)
            .Should().Equal(typeof(JsonObject), typeof(string));
    }
}
