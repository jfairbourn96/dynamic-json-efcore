using Dynamic.Json.Search;
using Microsoft.AspNetCore.Http;

namespace Dynamic.Json.AspNetCore;

/// <summary>
/// Extension methods for parsing ASP.NET Core query collections with Dynamic.Json.Search parsers.
/// </summary>
public static class DynamicSearchQueryParserQueryCollectionExtensions
{
    /// <summary>
    /// Determines whether the query string contains any non-ignored dynamic search parameters.
    /// </summary>
    public static bool HasDynamicSearchParameters(
        this IDynamicSearchQueryParser parser,
        IQueryCollection parameters,
        DynamicSearchQueryParserOptions? options = null)
    {
        return parser.HasDynamicSearchParameters(ToParameterDictionary(parameters), options);
    }

    /// <summary>
    /// Parses query-string parameters into validated dynamic search filters.
    /// </summary>
    public static DynamicSearchFilterParseResult Parse(
        this IDynamicSearchQueryParser parser,
        IQueryCollection parameters,
        IEnumerable<DynamicSearchField> fields,
        DynamicSearchQueryParserOptions? options = null)
    {
        return parser.Parse(ToParameterDictionary(parameters), fields, options);
    }

    private static IReadOnlyDictionary<string, string?> ToParameterDictionary(IQueryCollection parameters)
    {
        return parameters.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.FirstOrDefault(),
            StringComparer.OrdinalIgnoreCase);
    }
}