namespace Dynamic.Json.Search;

/// <summary>
/// Parses request-independent parameter collections into dynamic search filters.
/// </summary>
public interface IDynamicSearchQueryParser
{
    /// <summary>
    /// Determines whether the parameter collection contains any non-ignored dynamic search parameters.
    /// </summary>
    /// <param name="parameters">The parameters to inspect.</param>
    /// <param name="options">Optional parser settings for ignored keys and prefixes.</param>
    /// <returns><see langword="true" /> when at least one non-empty dynamic search parameter is present.</returns>
    bool HasDynamicSearchParameters(
        IReadOnlyDictionary<string, string?> parameters,
        DynamicSearchQueryParserOptions? options = null);

    /// <summary>
    /// Parses parameters into validated dynamic search filters.
    /// </summary>
    /// <param name="parameters">The parameters to parse.</param>
    /// <param name="fields">The searchable field definitions used for validation.</param>
    /// <param name="options">Optional parser settings for ignored keys and prefixes.</param>
    /// <returns>A parse result containing valid filters and validation errors.</returns>
    DynamicSearchFilterParseResult Parse(
        IReadOnlyDictionary<string, string?> parameters,
        IEnumerable<DynamicSearchField> fields,
        DynamicSearchQueryParserOptions? options = null);
}