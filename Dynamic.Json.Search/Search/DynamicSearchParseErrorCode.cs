namespace Dynamic.Json.Search;

/// <summary>
/// Identifies the reason a dynamic search query parameter could not be parsed.
/// </summary>
public enum DynamicSearchParseErrorCode
{
    /// <summary>
    /// The query parameter could not be interpreted as a supported dynamic search parameter.
    /// </summary>
    UnsupportedSearchParameter,

    /// <summary>
    /// The parsed dynamic field name is not a valid field name.
    /// </summary>
    InvalidFieldName,

    /// <summary>
    /// The parsed dynamic field name does not exist in the searchable field definitions.
    /// </summary>
    UnknownField,

    /// <summary>
    /// The parsed operator is not valid for the dynamic field's type.
    /// </summary>
    InvalidOperatorForFieldType,

    /// <summary>
    /// The supplied value could not be parsed as a number.
    /// </summary>
    InvalidNumberValue,

    /// <summary>
    /// The supplied value could not be parsed as a date.
    /// </summary>
    InvalidDateValue,

    /// <summary>
    /// The supplied value could not be parsed as a boolean.
    /// </summary>
    InvalidBooleanValue,

    /// <summary>
    /// The supplied value is not one of the field's allowed options.
    /// </summary>
    InvalidSelectOptionValue,
}
