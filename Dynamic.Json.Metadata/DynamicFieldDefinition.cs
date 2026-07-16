namespace Dynamic.Json.Metadata;

/// <summary>
/// Defines the runtime metadata for one dynamic JSON field.
/// </summary>
/// <param name="Name">The JSON property name.</param>
/// <param name="FieldType">The field's JSON value shape.</param>
/// <param name="Required">Whether the field must have a value.</param>
/// <param name="ElementType">The scalar item type for a <see cref="DynamicFieldType.JsonArray" /> field.</param>
/// <param name="Options">Allowed values for select fields or arrays of select values.</param>
public sealed record DynamicFieldDefinition(
    string? Name,
    DynamicFieldType FieldType,
    bool Required = false,
    DynamicFieldType? ElementType = null,
    IReadOnlyCollection<string?>? Options = null);
