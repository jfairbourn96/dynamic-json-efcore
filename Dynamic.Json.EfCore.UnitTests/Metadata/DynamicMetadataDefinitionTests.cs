using System.Text.Json;
using Dynamic.Json.Metadata;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.UnitTests.Metadata;

public sealed class DynamicMetadataDefinitionTests
{
    [Fact]
    public void Constructor_JsonArrayWithScalarElementType_CreatesDefinition()
    {
        DynamicFieldDefinition field = new("skills", DynamicFieldType.JsonArray, true, DynamicFieldType.Text);

        field.Name.Should().Be("skills");
        field.FieldType.Should().Be(DynamicFieldType.JsonArray);
        field.ElementType.Should().Be(DynamicFieldType.Text);
        field.Required.Should().BeTrue();
    }

    [Fact]
    public void JsonSerializer_JsonArrayMetadata_RoundTrips()
    {
        DynamicMetadataDefinition metadata = new([
            new("tags", DynamicFieldType.JsonArray, elementType: DynamicFieldType.Text),
            new("certifications", DynamicFieldType.JsonArray, elementType: DynamicFieldType.Select,
                options: ["azure", "aws"]),
        ]);

        string json = JsonSerializer.Serialize(metadata);
        DynamicMetadataDefinition? result = JsonSerializer.Deserialize<DynamicMetadataDefinition>(json);

        result.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void JsonSerializer_ExistingScalarMetadata_RoundTripsUnchanged()
    {
        DynamicMetadataDefinition metadata = new([
            new("name", DynamicFieldType.Text, true),
            new("yearsExperience", DynamicFieldType.Number),
            new("startDate", DynamicFieldType.Date),
            new("active", DynamicFieldType.Boolean),
            new("level", DynamicFieldType.Select, options: ["junior", "senior"]),
        ]);

        string json = JsonSerializer.Serialize(metadata);
        DynamicMetadataDefinition? result = JsonSerializer.Deserialize<DynamicMetadataDefinition>(json);

        result.Should().BeEquivalentTo(metadata);
    }

    [Theory]
    [InlineData(DynamicFieldType.JsonArray, null)]
    [InlineData(DynamicFieldType.JsonArray, DynamicFieldType.JsonArray)]
    [InlineData(DynamicFieldType.Text, DynamicFieldType.Text)]
    public void Constructor_InvalidElementConfiguration_Throws(DynamicFieldType fieldType, DynamicFieldType? elementType)
    {
        Action act = () => new DynamicFieldDefinition("field", fieldType, elementType: elementType);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_DuplicateFieldNames_Throws()
    {
        Action act = () => new DynamicMetadataDefinition([
            new("skills", DynamicFieldType.Text),
            new("skills", DynamicFieldType.Number),
        ]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void JsonSerializer_InvalidArrayMetadata_RejectsConfiguration()
    {
        const string json = """{"Fields":[{"Name":"skills","FieldType":5,"Required":false,"ElementType":null,"Options":[]}]}""";

        Action act = () => JsonSerializer.Deserialize<DynamicMetadataDefinition>(json);

        act.Should().Throw<ArgumentException>();
    }
}
