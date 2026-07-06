using Dynamic.Json.EfCore.Search;
using Dynamic.Json.EfCore.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dynamic.Json.EfCore.Tests.AspNetCore;

public class DynamicSearchQueryParserTests
{
    [Fact]
    public void Parse_ValidParameters_ReturnsDynamicSearchFilters()
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            ["firstName_contains"] = "Jane",
            ["yearsOfExperience_gte"] = "5",
            ["primaryLanguage"] = "csharp",
        });

        DynamicSearchQueryParserOptions options = new();
        options.IgnoredKeyPrefixes.Add("firstName_");

        DynamicSearchFilterParseResult result = DynamicSearchQueryParser.Parse(
            parameters,
            [
                new DynamicSearchField("yearsOfExperience", DynamicSearchFieldType.Number),
                new DynamicSearchField("primaryLanguage", DynamicSearchFieldType.Select, ["csharp", "typescript"]),
            ],
            options);

        result.Errors.Should().BeEmpty();
        result.Filters.Should().BeEquivalentTo(
        [
            new DynamicSearchFilter("yearsOfExperience", DynamicSearchFieldType.Number, SearchOperator.GreaterThanOrEqual, "5"),
            new DynamicSearchFilter("primaryLanguage", DynamicSearchFieldType.Select, SearchOperator.Exact, "csharp"),
        ]);
    }

    [Fact]
    public void Parse_InvalidFieldTypeValue_ReturnsValidationError()
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            ["yearsOfExperience_gte"] = "five",
        });

        DynamicSearchFilterParseResult result = DynamicSearchQueryParser.Parse(
            parameters,
            [new DynamicSearchField("yearsOfExperience", DynamicSearchFieldType.Number)]);

        result.Filters.Should().BeEmpty();
        result.Errors.Should().ContainSingle("Dynamic field 'yearsOfExperience' must be a valid number.");
    }
}
