using System.Text.Json.Serialization;

namespace Dynamic.Json.Metadata;

/// <summary>
/// Defines the runtime metadata for one dynamic JSON field.
/// </summary>
public sealed record DynamicFieldDefinition
{
    /// <summary>Creates and validates a runtime field definition.</summary>
    /// <param name="name">The JSON property name.</param>
    /// <param name="fieldType">The field's JSON value shape.</param>
    /// <param name="required">Whether the field must have a value.</param>
    /// <param name="elementType">The scalar item type for a <see cref="DynamicFieldType.JsonArray" /> field.</param>
    /// <param name="options">Allowed values for select fields or arrays of select values.</param>
    [JsonConstructor]
    public DynamicFieldDefinition(
        string name,
        DynamicFieldType fieldType,
        bool required = false,
        DynamicFieldType? elementType = null,
        IReadOnlyCollection<string>? options = null)
    {
        Name = name;
        FieldType = fieldType;
        Required = required;
        ElementType = elementType;
        Options = options?.ToArray() ?? Array.Empty<string>();

        Validate();
    }

    /// <summary>The JSON property name.</summary>
    public string Name { get; }

    /// <summary>The field's JSON value shape.</summary>
    public DynamicFieldType FieldType { get; }

    /// <summary>Whether the field must have a value.</summary>
    public bool Required { get; }

    /// <summary>The scalar item type for a JSON array; otherwise <see langword="null" />.</summary>
    public DynamicFieldType? ElementType { get; }

    /// <summary>Allowed values for select fields or arrays of select values.</summary>
    public IReadOnlyCollection<string> Options { get; }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("A runtime field name is required.", nameof(Name));
        }

        if (!Enum.IsDefined(FieldType))
        {
            throw new ArgumentOutOfRangeException(nameof(FieldType), FieldType, "The runtime field type is not supported.");
        }

        if (FieldType == DynamicFieldType.JsonArray)
        {
            if (ElementType is null)
            {
                throw new ArgumentException("A JsonArray field requires an element type.", nameof(ElementType));
            }

            if (!Enum.IsDefined(ElementType.Value) || ElementType == DynamicFieldType.JsonArray)
            {
                throw new ArgumentException("A JsonArray element type must be a supported scalar field type.", nameof(ElementType));
            }
        }
        else if (ElementType is not null)
        {
            throw new ArgumentException("Only a JsonArray field can declare an element type.", nameof(ElementType));
        }

        bool isSelect = FieldType == DynamicFieldType.Select ||
            FieldType == DynamicFieldType.JsonArray && ElementType == DynamicFieldType.Select;

        if (isSelect && Options.Count == 0)
        {
            throw new ArgumentException("A select field requires at least one option.", nameof(Options));
        }

        if (!isSelect && Options.Count > 0)
        {
            throw new ArgumentException("Options can only be declared for select fields.", nameof(Options));
        }

        if (Options.Any(string.IsNullOrWhiteSpace) || Options.Distinct(StringComparer.Ordinal).Count() != Options.Count)
        {
            throw new ArgumentException("Options must be non-blank and unique.", nameof(Options));
        }
    }
}
