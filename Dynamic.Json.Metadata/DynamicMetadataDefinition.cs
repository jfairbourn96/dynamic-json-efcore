using System.Text.Json.Serialization;

namespace Dynamic.Json.Metadata;

/// <summary>
/// Defines and validates a collection of runtime JSON fields.
/// </summary>
public sealed record DynamicMetadataDefinition
{
    /// <summary>Creates a validated runtime metadata definition.</summary>
    [JsonConstructor]
    public DynamicMetadataDefinition(IReadOnlyCollection<DynamicFieldDefinition>? fields)
    {
        Fields = fields?.ToArray() ?? throw new ArgumentNullException(nameof(fields));

        if (Fields.Any(field => field is null))
        {
            throw new ArgumentException("Runtime metadata cannot contain a null field.", nameof(fields));
        }

        if (Fields.Select(field => field.Name).Distinct(StringComparer.Ordinal).Count() != Fields.Count)
        {
            throw new ArgumentException("Runtime field names must be unique.", nameof(fields));
        }
    }

    /// <summary>The runtime fields in this definition.</summary>
    public IReadOnlyCollection<DynamicFieldDefinition> Fields { get; }
}
