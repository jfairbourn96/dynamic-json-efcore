namespace Dynamic.Json.Metadata;

/// <summary>
/// Represents an explicit request to convert an invalid metadata result into an exception.
/// </summary>
public sealed class DynamicMetadataValidationException : Exception
{
    internal DynamicMetadataValidationException(DynamicMetadataValidationResult validationResult)
        : base($"Runtime metadata validation failed with {validationResult.Errors.Count} error(s).")
    {
        ValidationResult = validationResult;
    }

    /// <summary>The complete validation result that caused this exception.</summary>
    public DynamicMetadataValidationResult ValidationResult { get; }
}
