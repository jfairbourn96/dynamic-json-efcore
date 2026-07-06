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

    public static TheoryData<string, string, DynamicSearchParseError> InvalidFilterParameters => new()
    {
        {
            "_gt",
            "7",
            new DynamicSearchParseError(DynamicSearchParseErrorCode.UnsupportedSearchParameter, "_gt", Value: "7")
        },
        {
            "ice-power",
            "true",
            new DynamicSearchParseError(DynamicSearchParseErrorCode.InvalidFieldName, "ice-power", "ice-power", SearchOperator.Exact, "true")
        },
        {
            "snowmanName",
            "Olaf",
            new DynamicSearchParseError(DynamicSearchParseErrorCode.UnknownField, "snowmanName", "snowmanName", SearchOperator.Exact, "Olaf")
        },
        {
            "favoriteSongName_gt",
            "Let It Go",
            new DynamicSearchParseError(DynamicSearchParseErrorCode.InvalidOperatorForFieldType, "favoriteSongName_gt", "favoriteSongName", SearchOperator.GreaterThan, "Let It Go")
        },
        {
            "numberOfSongs_gte",
            "seven",
            new DynamicSearchParseError(DynamicSearchParseErrorCode.InvalidNumberValue, "numberOfSongs_gte", "numberOfSongs", SearchOperator.GreaterThanOrEqual, "seven")
        },
        {
            "coronationDate_startDate",
            "someday",
            new DynamicSearchParseError(DynamicSearchParseErrorCode.InvalidDateValue, "coronationDate_startDate", "coronationDate", SearchOperator.StartDate, "someday")
        },
        {
            "hasIcePowers",
            "onlyInWinter",
            new DynamicSearchParseError(DynamicSearchParseErrorCode.InvalidBooleanValue, "hasIcePowers", "hasIcePowers", SearchOperator.Exact, "onlyInWinter")
        },
        {
            "kingdom",
            "Southern Isles",
            new DynamicSearchParseError(DynamicSearchParseErrorCode.InvalidSelectOptionValue, "kingdom", "kingdom", SearchOperator.Exact, "Southern Isles")
        },
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
            ["numberOfSongs_gte"] = "seven",
        });

        DynamicSearchFilterParseResult result = DynamicSearchQueryParser.Parse(
            parameters,
            [new DynamicSearchField("numberOfSongs", DynamicSearchFieldType.Number)]);

        result.Filters.Should().BeEmpty();
        result.Errors.Should().ContainSingle().Which.Should().Be(
            new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidNumberValue,
                "numberOfSongs_gte",
                "numberOfSongs",
                SearchOperator.GreaterThanOrEqual,
                "seven"));
    }

    [Theory]
    [MemberData(nameof(InvalidFilterParameters))]
    public void Parse_InvalidParameter_ReturnsDynamicSearchParseError(
        string queryParamName,
        string value,
        DynamicSearchParseError expectedError)
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            [queryParamName] = value
        });

        DynamicSearchFilterParseResult result = DynamicSearchQueryParser.Parse(
            parameters,
            GetSearchFields());

        result.Filters.Should().BeEmpty();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
    }

    private static DynamicSearchField[] GetSearchFields()
    {
        return
        [
            new DynamicSearchField("favoriteSongName", DynamicSearchFieldType.Text),
            new DynamicSearchField("numberOfSongs", DynamicSearchFieldType.Number),
            new DynamicSearchField("coronationDate", DynamicSearchFieldType.Date),
            new DynamicSearchField("hasIcePowers", DynamicSearchFieldType.Boolean),
            new DynamicSearchField("kingdom", DynamicSearchFieldType.Select, ["Arendelle", "Northuldra"]),
        ];
    }
}
