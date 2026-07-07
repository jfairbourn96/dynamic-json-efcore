namespace Dynamic.Json.Search;

/// <summary>
/// Represents a validated search condition against a dynamic JSON field.
/// </summary>
/// <param name="FieldName">The JSON field name to search.</param>
/// <param name="FieldType">The field's search value type.</param>
/// <param name="Operator">The comparison operation to apply.</param>
/// <param name="Value">The raw search value supplied by the caller.</param>
public sealed record DynamicSearchFilter(
    string FieldName,
    DynamicSearchFieldType FieldType,
    SearchOperator Operator,
    string Value);
