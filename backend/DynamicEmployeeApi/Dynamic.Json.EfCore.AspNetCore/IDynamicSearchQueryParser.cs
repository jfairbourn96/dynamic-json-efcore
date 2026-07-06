using Dynamic.Json.EfCore.Search;
using Microsoft.AspNetCore.Http;

namespace Dynamic.Json.EfCore.AspNetCore;

/// <summary>
/// Parses ASP.NET Core query-string parameters into dynamic search filters.
/// </summary>
public interface IDynamicSearchQueryParser
{
    /// <summary>
    /// Determines whether the query string contains any non-ignored dynamic search parameters.
    /// </summary>
    /// <param name="parameters">The query-string parameters to inspect.</param>
    /// <param name="options">Optional parser settings for ignored keys and prefixes.</param>
    /// <returns><see langword="true" /> when at least one non-empty dynamic search parameter is present.</returns>
    bool HasDynamicSearchParameters(
        IQueryCollection parameters,
        DynamicSearchQueryParserOptions? options = null);

    /// <summary>
    /// Parses query-string parameters into validated dynamic search filters.
    /// </summary>
    /// <param name="parameters">The query-string parameters to parse.</param>
    /// <param name="fields">The searchable field definitions used for validation.</param>
    /// <param name="options">Optional parser settings for ignored keys and prefixes.</param>
    /// <returns>A parse result containing valid filters and validation errors.</returns>
    DynamicSearchFilterParseResult Parse(
        IQueryCollection parameters,
        IEnumerable<DynamicSearchField> fields,
        DynamicSearchQueryParserOptions? options = null);
}
