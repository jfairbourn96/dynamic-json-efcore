namespace Dynamic.Json.Metadata;

/// <summary>
/// Identifies a runtime metadata validation failure without requiring consumers to parse messages.
/// </summary>
public enum DynamicMetadataValidationErrorCode
{
    FieldsRequired,
    NullField,
    FieldNameRequired,
    UnsupportedFieldType,
    ArrayElementTypeRequired,
    UnsupportedArrayElementType,
    NestedArrayNotSupported,
    ElementTypeNotAllowed,
    SelectOptionsRequired,
    OptionsNotAllowed,
    OptionRequired,
    DuplicateOption,
    DuplicateFieldName,
}
