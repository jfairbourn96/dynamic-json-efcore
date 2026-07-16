namespace Dynamic.Json.Metadata;

/// <summary>
/// Describes one runtime metadata validation failure.
/// </summary>
/// <param name="Code">The stable error code.</param>
/// <param name="Path">The path to the invalid metadata value.</param>
/// <param name="Message">A human-readable description of the failure.</param>
public sealed record DynamicMetadataValidationError(
    DynamicMetadataValidationErrorCode Code,
    string Path,
    string Message);
