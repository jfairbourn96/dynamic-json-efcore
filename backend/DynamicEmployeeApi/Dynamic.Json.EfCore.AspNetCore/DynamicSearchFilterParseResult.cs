using Dynamic.Json.EfCore.Search;

namespace Dynamic.Json.EfCore.AspNetCore;

/// <summary>
/// Contains the filters and validation errors produced by dynamic search query parsing.
/// </summary>
/// <param name="Filters">The successfully parsed dynamic search filters.</param>
/// <param name="Errors">Structured validation errors for unsupported or invalid query parameters.</param>
public sealed record DynamicSearchFilterParseResult(
    IReadOnlyList<DynamicSearchFilter> Filters,
    IReadOnlyList<DynamicSearchParseError> Errors);
