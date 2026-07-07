namespace Dynamic.Json.Search;

/// <summary>
/// Describes a searchable JSON field and the values allowed when the field is option-backed.
/// </summary>
/// <param name="Name">The JSON field name to search.</param>
/// <param name="FieldType">The field's search value type.</param>
/// <param name="Options">
/// The allowed values for option-backed fields. This is intentionally non-null; use an empty
/// collection for field types that do not constrain values to a known option set.
/// </param>
public sealed record DynamicSearchField(
    string Name,
    DynamicSearchFieldType FieldType,
    IReadOnlyCollection<string> Options)
{
    /// <summary>
    /// Creates a searchable field with no option constraints.
    /// </summary>
    /// <remarks>
    /// This overload keeps <see cref="Options" /> non-null while avoiding nullable checks or
    /// requiring callers to pass an empty collection for non-option field types.
    /// </remarks>
    public DynamicSearchField(string name, DynamicSearchFieldType fieldType)
        : this(name, fieldType, Array.Empty<string>())
    {
    }
}
