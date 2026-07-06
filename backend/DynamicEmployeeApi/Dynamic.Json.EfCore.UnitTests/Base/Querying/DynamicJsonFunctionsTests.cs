using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.Querying;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.UnitTests.Base.Querying;

public class DynamicJsonFunctionsTests
{
    [Fact]
    public void Value_DirectInvocation_ThrowsNotSupportedException()
    {
        Action act = () => DynamicJsonFunctions.Value(new JsonObject(), "$.name");

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Only for use in EF Core queries.");
    }

    [Fact]
    public void ValueDecimal_DirectInvocation_ThrowsNotSupportedException()
    {
        Action act = () => DynamicJsonFunctions.ValueDecimal(new JsonObject(), "$.powerLevel");

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Only for use in EF Core queries.");
    }

    [Fact]
    public void ValueDate_DirectInvocation_ThrowsNotSupportedException()
    {
        Action act = () => DynamicJsonFunctions.ValueDate(new JsonObject(), "$.coronationDate");

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Only for use in EF Core queries.");
    }
}
