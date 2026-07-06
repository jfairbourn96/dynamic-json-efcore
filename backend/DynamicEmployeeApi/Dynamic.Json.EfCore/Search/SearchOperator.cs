namespace Dynamic.Json.EfCore.Search;

/// <summary>
/// Defines the supported comparison operations for dynamic search filters.
/// </summary>
public enum SearchOperator
{
    /// <summary>
    /// Matches text that contains the supplied value.
    /// </summary>
    Contains,

    /// <summary>
    /// Matches text that begins with the supplied value.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Matches values that equal the supplied value.
    /// </summary>
    Exact,

    /// <summary>
    /// Matches numeric values less than the supplied value.
    /// </summary>
    LessThan,

    /// <summary>
    /// Matches numeric values less than or equal to the supplied value.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Matches numeric values greater than the supplied value.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Matches numeric values greater than or equal to the supplied value.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Matches date values on or after the supplied value.
    /// </summary>
    StartDate,

    /// <summary>
    /// Matches date values on or before the supplied value.
    /// </summary>
    EndDate,
}
