using Dynamic.Json.EfCore.Search;
using Dynamic.Json.EfCore.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dynamic.Json.EfCore.Tests.AspNetCore;

public class DynamicSearchQueryParserTests
{
    private readonly IDynamicSearchQueryParser _parser = new DynamicSearchQueryParser();

    public static TheoryData<string, string, DynamicSearchFilter> ValidNumberFilterParameters => new()
    {
        { "numberOfSongs_gt", "7", new DynamicSearchFilter("numberOfSongs", DynamicSearchFieldType.Number, SearchOperator.GreaterThan, "7") },
        { "numberOfSongs_gte", "7", new DynamicSearchFilter("numberOfSongs", DynamicSearchFieldType.Number, SearchOperator.GreaterThanOrEqual, "7") },
        { "numberOfSongs_lt", "7", new DynamicSearchFilter("numberOfSongs", DynamicSearchFieldType.Number, SearchOperator.LessThan, "7") },
        { "numberOfSongs_lte", "7", new DynamicSearchFilter("numberOfSongs", DynamicSearchFieldType.Number, SearchOperator.LessThanOrEqual, "7") },
        { "numberOfSongs", "7", new DynamicSearchFilter("numberOfSongs", DynamicSearchFieldType.Number, SearchOperator.Exact, "7") },
    };

    public static TheoryData<string, string, DynamicSearchFilter> ValidDateFilterParameters => new()
    {
        { "coronationDate_startDate", "2013-11-27", new DynamicSearchFilter("coronationDate", DynamicSearchFieldType.Date, SearchOperator.StartDate, "2013-11-27") },
        { "coronationDate_endDate", "2013-11-27", new DynamicSearchFilter("coronationDate", DynamicSearchFieldType.Date, SearchOperator.EndDate, "2013-11-27") },
    };

    public static TheoryData<string, string, DynamicSearchFilter> ValidBoolFilterParameters => new()
    {
        { "hasIcePowers", "true", new DynamicSearchFilter("hasIcePowers", DynamicSearchFieldType.Boolean, SearchOperator.Exact, "true") },
        { "hasIcePowers", "false", new DynamicSearchFilter("hasIcePowers", DynamicSearchFieldType.Boolean, SearchOperator.Exact, "false") },
    };

    public static TheoryData<string, string, DynamicSearchFilter> ValidTextFilterParameters => new()
    {
        { "favoriteSongName", "Let It Go", new DynamicSearchFilter("favoriteSongName", DynamicSearchFieldType.Text, SearchOperator.Exact, "Let It Go") },
        { "favoriteSongName_startsWith", "Let", new DynamicSearchFilter("favoriteSongName", DynamicSearchFieldType.Text, SearchOperator.StartsWith, "Let") },
        { "favoriteSongName_contains", "It Go", new DynamicSearchFilter("favoriteSongName", DynamicSearchFieldType.Text, SearchOperator.Contains, "It Go") },
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

    public static TheoryData<QueryCollection, DynamicSearchQueryParserOptions?, bool> DynamicSearchParameterPresence => new()
    {
        { new QueryCollection(), null, false },
        { CreateQueryCollection("favoriteSongName", "   "), null, false },
        { CreateQueryCollection("pageNumber", "1"), CreateParserOptions(ignoredKeys: ["pageNumber"]), false },
        { CreateQueryCollection("core_name", "Elsa"), CreateParserOptions(ignoredKeyPrefixes: ["core_"]), false },
        { CreateQueryCollection("favoriteSongName", "Into The Unknown"), null, true },
    };

    public static TheoryData<string, string, DynamicSearchField, DynamicSearchParseError> InvalidFieldTypeValues => new()
    {
        {
            "numberOfSongs_gte",
            "seven",
            new DynamicSearchField("numberOfSongs", DynamicSearchFieldType.Number),
            new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidNumberValue,
                "numberOfSongs_gte",
                "numberOfSongs",
                SearchOperator.GreaterThanOrEqual,
                "seven")
        },
        {
            "coronationDate_startDate",
            "someday",
            new DynamicSearchField("coronationDate", DynamicSearchFieldType.Date),
            new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidDateValue,
                "coronationDate_startDate",
                "coronationDate",
                SearchOperator.StartDate,
                "someday")
        },
        {
            "hasIcePowers",
            "concealDontFeel",
            new DynamicSearchField("hasIcePowers", DynamicSearchFieldType.Boolean),
            new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidBooleanValue,
                "hasIcePowers",
                "hasIcePowers",
                SearchOperator.Exact,
                "concealDontFeel")
        },
        {
            "kingdom",
            "Southern Isles",
            new DynamicSearchField("kingdom", DynamicSearchFieldType.Select, ["Arendelle", "Northuldra"]),
            new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidSelectOptionValue,
                "kingdom",
                "kingdom",
                SearchOperator.Exact,
                "Southern Isles")
        },
    };

    [Theory]
    [MemberData(nameof(ValidNumberFilterParameters))]
    [MemberData(nameof(ValidDateFilterParameters))]
    [MemberData(nameof(ValidBoolFilterParameters))]
    [MemberData(nameof(ValidTextFilterParameters))]
    public void Parse_ValidParameters_ReturnsDynamicSearchFilters(
        string queryParamName,
        string value,
        DynamicSearchFilter expectedFilter)
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            [queryParamName] = value
        });

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields());

        result.Errors.Should().BeEmpty();
        result.Filters.Should().BeEquivalentTo([expectedFilter]);
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

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields());

        result.Errors.Should().BeEmpty();
        result.Filters.Should().BeEquivalentTo(
        [
            new DynamicSearchFilter(fieldName, DynamicSearchFieldType.Select, SearchOperator.Exact, value)
        ]);
    }

    [Theory]
    [MemberData(nameof(InvalidFieldTypeValues))]
    public void Parse_InvalidFieldTypeValue_ReturnsValidationError(
        string queryParamName,
        string value,
        DynamicSearchField field,
        DynamicSearchParseError expectedError)
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            [queryParamName] = value,
        });

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            [field]);

        result.Filters.Should().BeEmpty();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
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

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields());

        result.Filters.Should().BeEmpty();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
    }

    [Fact]
    public void Parse_IgnoredParameters_DoesNotReturnFiltersOrErrors()
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            ["pageNumber"] = "1",
            ["core_name"] = "Elsa",
            ["favoriteSongName"] = "   ",
        });

        DynamicSearchQueryParserOptions options = CreateParserOptions(
            ignoredKeys: ["pageNumber"],
            ignoredKeyPrefixes: ["core_"]);

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields(),
            options);

        result.Filters.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_IgnoredParametersWithValidParameter_ReturnsOnlyDynamicSearchFilter()
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            ["pageNumber"] = "1",
            ["core_name"] = "Elsa",
            ["favoriteSongName"] = "Into The Unknown",
        });

        DynamicSearchQueryParserOptions options = CreateParserOptions(
            ignoredKeys: ["pageNumber"],
            ignoredKeyPrefixes: ["core_"]);

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields(),
            options);

        result.Errors.Should().BeEmpty();
        result.Filters.Should().BeEquivalentTo(
        [
            new DynamicSearchFilter("favoriteSongName", DynamicSearchFieldType.Text, SearchOperator.Exact, "Into The Unknown")
        ]);
    }

    [Fact]
    public void Parse_FieldLookup_IsCaseInsensitive()
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            ["FAVORITESONGNAME_contains"] = "Unknown",
        });

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields());

        result.Errors.Should().BeEmpty();
        result.Filters.Should().BeEquivalentTo(
        [
            new DynamicSearchFilter("favoriteSongName", DynamicSearchFieldType.Text, SearchOperator.Contains, "Unknown")
        ]);
    }

    [Fact]
    public void Parse_SelectOptionValidation_IsCaseSensitive()
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            ["kingdom"] = "arendelle",
        });

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields());

        result.Filters.Should().BeEmpty();
        result.Errors.Should().ContainSingle().Which.Should().Be(
            new DynamicSearchParseError(
                DynamicSearchParseErrorCode.InvalidSelectOptionValue,
                "kingdom",
                "kingdom",
                SearchOperator.Exact,
                "arendelle"));
    }

    [Fact]
    public void Parse_TrimmedValue_ReturnsTrimmedValueInFilter()
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            ["favoriteSongName"] = "  Let It Go  ",
        });

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields());

        result.Errors.Should().BeEmpty();
        result.Filters.Should().BeEquivalentTo(
        [
            new DynamicSearchFilter("favoriteSongName", DynamicSearchFieldType.Text, SearchOperator.Exact, "Let It Go")
        ]);
    }

    [Fact]
    public void Parse_MultipleValidParameters_ReturnsAllDynamicSearchFilters()
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            ["favoriteSongName_contains"] = "Go",
            ["numberOfSongs_gte"] = "7",
            ["hasIcePowers"] = "true",
        });

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields());

        result.Errors.Should().BeEmpty();
        result.Filters.Should().BeEquivalentTo(
        [
            new DynamicSearchFilter("favoriteSongName", DynamicSearchFieldType.Text, SearchOperator.Contains, "Go"),
            new DynamicSearchFilter("numberOfSongs", DynamicSearchFieldType.Number, SearchOperator.GreaterThanOrEqual, "7"),
            new DynamicSearchFilter("hasIcePowers", DynamicSearchFieldType.Boolean, SearchOperator.Exact, "true"),
        ]);
    }

    [Fact]
    public void Parse_ValidAndInvalidParameters_ReturnsFiltersAndErrors()
    {
        QueryCollection parameters = new(new Dictionary<string, StringValues>
        {
            ["favoriteSongName_contains"] = "Go",
            ["snowmanName"] = "Olaf",
        });

        DynamicSearchFilterParseResult result = _parser.Parse(
            parameters,
            GetSearchFields());

        result.Filters.Should().BeEquivalentTo(
        [
            new DynamicSearchFilter("favoriteSongName", DynamicSearchFieldType.Text, SearchOperator.Contains, "Go")
        ]);
        result.Errors.Should().ContainSingle().Which.Should().Be(
            new DynamicSearchParseError(
                DynamicSearchParseErrorCode.UnknownField,
                "snowmanName",
                "snowmanName",
                SearchOperator.Exact,
                "Olaf"));
    }

    [Fact]
    public void AddDynamicJsonEfCoreAspNetCore_RegistersDynamicSearchQueryParser()
    {
        ServiceCollection services = new();

        services.AddDynamicJsonEfCoreAspNetCore();

        using ServiceProvider provider = services.BuildServiceProvider();
        IDynamicSearchQueryParser parser = provider.GetRequiredService<IDynamicSearchQueryParser>();

        parser.Should().BeOfType<DynamicSearchQueryParser>();
    }

    [Theory]
    [MemberData(nameof(DynamicSearchParameterPresence))]
    public void HasDynamicSearchParameters_ReturnsWhetherQueryContainsNonIgnoredParameters(
        QueryCollection parameters,
        DynamicSearchQueryParserOptions? options,
        bool expectedResult)
    {
        bool result = _parser.HasDynamicSearchParameters(parameters, options);

        result.Should().Be(expectedResult);
    }

    private static QueryCollection CreateQueryCollection(string key, string value)
    {
        return new QueryCollection(new Dictionary<string, StringValues>
        {
            [key] = value
        });
    }

    private static DynamicSearchQueryParserOptions CreateParserOptions(
        string[]? ignoredKeys = null,
        string[]? ignoredKeyPrefixes = null)
    {
        DynamicSearchQueryParserOptions options = new();

        if (ignoredKeys is not null)
        {
            options.IgnoredKeys.UnionWith(ignoredKeys);
        }

        if (ignoredKeyPrefixes is not null)
        {
            options.IgnoredKeyPrefixes.UnionWith(ignoredKeyPrefixes);
        }

        return options;
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
