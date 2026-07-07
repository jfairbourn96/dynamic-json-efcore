using Dynamic.Json.Search;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.UnitTests.Base.Search;

public class DynamicSearchFieldTests
{
    [Fact]
    public void Constructor_WithoutOptions_UsesEmptyOptionsCollection()
    {
        DynamicSearchField field = new("name", DynamicSearchFieldType.Text);

        field.Name.Should().Be("name");
        field.FieldType.Should().Be(DynamicSearchFieldType.Text);
        field.Options.Should().NotBeNull();
        field.Options.Should().BeEmpty();
    }
}
