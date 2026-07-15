namespace Dynamic.Json.Metadata;

/// <summary>
/// Describes the JSON value shape of a runtime-defined field.
/// </summary>
public enum DynamicFieldType
{
    /// <summary>A JSON string value.</summary>
    Text,

    /// <summary>A JSON number value.</summary>
    Number,

    /// <summary>A date represented as a JSON string.</summary>
    Date,

    /// <summary>A JSON boolean value.</summary>
    Boolean,

    /// <summary>A JSON string constrained to a configured option set.</summary>
    Select,

    /// <summary>An ordered JSON array whose items have a configured scalar type.</summary>
    JsonArray,
}
