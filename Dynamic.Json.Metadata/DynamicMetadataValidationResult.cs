namespace Dynamic.Json.Metadata;

/// <summary>
/// Contains all failures found while validating a runtime metadata definition.
/// </summary>
public sealed class DynamicMetadataValidationResult
{
    internal DynamicMetadataValidationResult(IEnumerable<DynamicMetadataValidationError> errors)
    {
        Errors = errors.ToArray();
    }

    /// <summary>Whether the metadata passed every validation rule.</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>All validation failures in metadata order.</summary>
    public IReadOnlyCollection<DynamicMetadataValidationError> Errors { get; }
}
