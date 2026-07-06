using Dynamic.Json.EfCore.Search;
using Dynamic.Json.EfCore.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dynamic.Json.EfCore.Tests.AspNetCore;

public class DynamicSearchQueryParserTests
{
    public static TheoryData<string, string, DynamicSearchFieldType, string, SearchOperator> ValidNumberFilterParameters => new()
    {
        { "numberOfSongs", "numberOfSongs_gt", DynamicSearchFieldType.Number, "7", SearchOperator.GreaterThan },
        { "numberOfSongs", "numberOfSongs_gte", DynamicSearchFieldType.Number, "7", SearchOperator.GreaterThanOrEqual },
        { "numberOfSongs", "numberOfSongs_lt", DynamicSearchFieldType.Number, "7", SearchOperator.LessThan },
        { "numberOfSongs", "numberOfSongs_lte", DynamicSearchFieldType.Number, "7", SearchOperator.LessThanOrEqual },
        { "numberOfSongs", "numberOfSongs", DynamicSearchFieldType.Number, "7", SearchOperator.Exact },
    };

    public static TheoryData<string, string, DynamicSearchFieldType, string, SearchOperator> ValidDateFilterParameters => new()
    {
        { "coronationDate", "coronationDate_startDate", DynamicSearchFieldType.Date, "2013-11-27", SearchOperator.StartDate },
        { "coronationDate", "coronationDate_endDate", DynamicSearchFieldType.Date, "2013-11-27", SearchOperator.EndDate },
    };

    public static TheoryData<string, string, DynamicSearchFieldType, string, SearchOperator> ValidBoolFilterParameters => new()
    {
        { "hasIcePowers", "hasIcePowers", DynamicSearchFieldType.Boolean, "true", SearchOperator.Exact },
        { "hasIcePowers", "hasIcePowers", DynamicSearchFieldType.Boolean, "false", SearchOperator.Exact },
    };

    public static TheoryData<string, string, DynamicSearchFieldType, string, SearchOperator> ValidTextFilterParameters => new()
    {
        { "favoriteSongName", "favoriteSongName", DynamicSearchFieldType.Text, "Let It Go", SearchOperator.Exact },
        { "favoriteSongName", "favoriteSongName_startsWith", DynamicSearchFieldType.Text, "Let", SearchOperator.StartsWith },
        { "favoriteSongName", "favoriteSongName_contains", DynamicSearchFieldType.Text, "It Go", SearchOperator.Contains },
    };

    [Theory]
    [MemberData(nameof(ValidNumberFilterParameters))]
    [MemberData(nameof(ValidDateFilterParameters))]
    [MemberData(nameof(ValidBoolFilterParameters))]
    [MemberData(nameof(ValidTextFilterParameters))]
    public void Parse_ValidParameters_ReturnsDynamicSearchFilters(
        string fieldName,
        string queryParamName,
        DynamicSearchFieldType fieldType,
        string value,
        SearchOperator expectedOperator)
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            [queryParamName] = value
        });

        DynamicSearchFilterParseResult result = DynamicSearchQueryParser.Parse(
            parameters,
            [new DynamicSearchField(fieldName, fieldType)]);

        result.Errors.Should().BeEmpty();
        result.Filters.Should().BeEquivalentTo([new DynamicSearchFilter(fieldName, fieldType, expectedOperator, value)]);
    }

    [Theory]
    [InlineData("kingdom", "kingdom", "Arendelle")]
    [InlineData("kingdom", "kingdom_exact", "Northuldra")]
    public void Parse_ValidSelectParameter_ReturnsDynamicSearchFilter(
        string fieldName,
        string queryParamName,
        string value)
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            [queryParamName] = value
        });

        DynamicSearchFilterParseResult result = DynamicSearchQueryParser.Parse(
            parameters,
            [new DynamicSearchField(fieldName, DynamicSearchFieldType.Select, ["Arendelle", "Northuldra"])]);

        result.Errors.Should().BeEmpty();
        result.Filters.Should().BeEquivalentTo(
        [
            new DynamicSearchFilter(fieldName, DynamicSearchFieldType.Select, SearchOperator.Exact, value)
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
