using Dynamic.Json.EfCore.Search;

namespace Dynamic.Json.EfCore.AspNetCore;

/// <summary>
/// Describes a dynamic search query parsing or validation error.
/// </summary>
/// <param name="Code">The stable error code identifying the parse failure.</param>
/// <param name="QueryKey">The query-string key that produced the error.</param>
/// <param name="FieldName">The parsed dynamic field name, when available.</param>
/// <param name="Operator">The parsed search operator, when available.</param>
/// <param name="Value">The supplied query-string value, when available.</param>
public sealed record DynamicSearchParseError(
    DynamicSearchParseErrorCode Code,
    string QueryKey,
    string? FieldName = null,
    SearchOperator? Operator = null,
    string? Value = null);
