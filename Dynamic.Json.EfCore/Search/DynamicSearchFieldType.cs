namespace Dynamic.Json.EfCore.Search;

/// <summary>
/// Describes the value shape used when validating and applying a dynamic search field.
/// </summary>
public enum DynamicSearchFieldType
{
    /// <summary>
    /// A string field that supports text search operators.
    /// </summary>
    Text,

    /// <summary>
    /// A numeric field that supports equality and range operators.
    /// </summary>
    Number,

    /// <summary>
    /// A date field that supports start and end date operators.
    /// </summary>
    Date,

    /// <summary>
    /// A boolean field that supports exact matching.
    /// </summary>
    Boolean,

    /// <summary>
    /// A field constrained to a known option set.
    /// </summary>
    Select,
}
