using Dynamic.Json.EfCore.Querying;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.UnitTests.Base.Querying;

public sealed class DynamicJsonPathTests
{
    [Theory]
    [InlineData("$", "$", new string[0])]
    [InlineData("$.stageName", "$.stageName", new[] { "stageName" })]
    [InlineData("$.huntrix.leader", "$.huntrix.leader", new[] { "huntrix", "leader" })]
    [InlineData("$.\"stage.name\"", "$.\"stage.name\"", new[] { "stage.name" })]
    [InlineData("$.\"demon\\\"hunter\"", "$.\"demon\\\"hunter\"", new[] { "demon\"hunter" })]
    [InlineData("$.\"saja\\\\boys\"", "$.\"saja\\\\boys\"", new[] { "saja\\boys" })]
    [InlineData("$.\"\"", "$.\"\"", new[] { "" })]
    [InlineData("$.\"stageNa\\u006de\"", "$.stageName", new[] { "stageName" })]
    public void Normalize_SupportedPropertyPath_ReturnsCanonicalPath(
        string path,
        string expected,
        string[] expectedProperties)
    {
        DynamicJsonPath.Normalize(path).Should().Be(expected);
        DynamicJsonPath.ParseProperties(path).Should().Equal(expectedProperties);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("name")]
    [InlineData("$.")]
    [InlineData("$..name")]
    [InlineData("$.1name")]
    [InlineData("$[0]")]
    [InlineData("$.items[0]")]
    [InlineData("$.*")]
    [InlineData("$..*")]
    [InlineData("$?(@.active)")]
    [InlineData("strict $.name")]
    [InlineData("$.\"unterminated")]
    [InlineData("$.\"bad\\qescape\"")]
    [InlineData("$.name ")]
    public void Normalize_UnsupportedOrMalformedPath_Throws(string? path)
    {
        Action act = () => DynamicJsonPath.Normalize(path!);

        act.Should().Throw<DynamicJsonPathException>()
            .Which.ParamName.Should().Be("path");
    }

    [Theory]
    [InlineData("stageName", "$.stageName")]
    [InlineData("stage.name", "$.\"stage.name\"")]
    [InlineData("demon\"hunter", "$.\"demon\\\"hunter\"")]
    [InlineData("saja\\boys", "$.\"saja\\\\boys\"")]
    [InlineData("", "$.\"\"")]
    [InlineData("名字", "$.\"名字\"")]
    [InlineData("items[0]", "$.\"items[0]\"")]
    [InlineData("*", "$.\"*\"")]
    [InlineData("strict $.secret", "$.\"strict $.secret\"")]
    [InlineData("stage' OR 1=1--", "$.\"stage' OR 1=1--\"")]
    public void FromProperty_ExactPropertyName_ReturnsEscapedPath(string propertyName, string expected)
    {
        DynamicJsonPath.FromProperty(propertyName).Should().Be(expected);
        DynamicJsonPath.ParseProperties(expected).Should().Equal(propertyName);
    }

    [Fact]
    public void FromProperties_NestedPropertyNames_ReturnsPortablePath()
    {
        DynamicJsonPath.FromProperties("huntrix", "demon.level", "idol-awards")
            .Should().Be("$.huntrix.\"demon.level\".\"idol-awards\"");
    }

    [Fact]
    public void AppendProperty_UnsupportedBasePath_Throws()
    {
        Action act = () => DynamicJsonPath.AppendProperty("$.items[0]", "name");

        act.Should().Throw<DynamicJsonPathException>();
    }
}
