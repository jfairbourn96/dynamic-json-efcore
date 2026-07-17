using System.Text.Json.Nodes;
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
}
