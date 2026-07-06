using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.SqlServer;

public class SqlServerIntegrationTestsPlaceholder
{
    [Fact]
    public void Placeholder_IntegrationTestProjectIsDiscoverable()
    {
        true.Should().BeTrue();
    }
}
