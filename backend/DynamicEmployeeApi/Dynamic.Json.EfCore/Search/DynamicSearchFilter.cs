namespace Dynamic.Json.EfCore.Search;

public sealed record DynamicSearchFilter(
    string FieldName,
    DynamicSearchFieldType FieldType,
    SearchOperator Operator,
    string Value);
