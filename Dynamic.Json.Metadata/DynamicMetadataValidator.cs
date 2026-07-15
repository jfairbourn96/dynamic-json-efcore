namespace Dynamic.Json.Metadata;

/// <summary>
/// Validates provider-neutral runtime metadata without using exceptions for expected invalid input.
/// </summary>
public static class DynamicMetadataValidator
{
    /// <summary>Validates a metadata definition and returns every discovered failure.</summary>
    public static DynamicMetadataValidationResult Validate(IReadOnlyCollection<DynamicFieldDefinition?>? fields)
    {
        List<DynamicMetadataValidationError> errors = [];

        if (fields is null)
        {
            Add(errors, DynamicMetadataValidationErrorCode.FieldsRequired, "$", "A metadata field collection is required.");
            return new(errors);
        }

        Dictionary<string, int> fieldNames = new(StringComparer.Ordinal);
        int index = 0;
        foreach (DynamicFieldDefinition? field in fields)
        {
            string fieldPath = $"fields[{index}]";
            if (field is null)
            {
                Add(errors, DynamicMetadataValidationErrorCode.NullField, fieldPath, "A metadata field cannot be null.");
                index++;
                continue;
            }

            ValidateField(field, fieldPath, errors);

            if (!string.IsNullOrWhiteSpace(field.Name))
            {
                if (fieldNames.TryGetValue(field.Name, out int firstIndex))
                {
                    Add(errors, DynamicMetadataValidationErrorCode.DuplicateFieldName, $"{fieldPath}.name",
                        $"Field name '{field.Name}' duplicates fields[{firstIndex}].name.");
                }
                else
                {
                    fieldNames.Add(field.Name, index);
                }
            }

            index++;
        }

        return new(errors);
    }

    /// <summary>
    /// Validates metadata and returns it when valid, or throws an exception containing all failures.
    /// </summary>
    public static IReadOnlyCollection<DynamicFieldDefinition?> ValidateAndThrow(
        IReadOnlyCollection<DynamicFieldDefinition?>? fields)
    {
        DynamicMetadataValidationResult result = Validate(fields);
        if (!result.IsValid)
        {
            throw new DynamicMetadataValidationException(result);
        }

        return fields!;
    }

    private static void ValidateField(
        DynamicFieldDefinition field,
        string path,
        ICollection<DynamicMetadataValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(field.Name))
        {
            Add(errors, DynamicMetadataValidationErrorCode.FieldNameRequired, $"{path}.name", "A runtime field name is required.");
        }

        if (!Enum.IsDefined(field.FieldType))
        {
            Add(errors, DynamicMetadataValidationErrorCode.UnsupportedFieldType, $"{path}.fieldType",
                $"Runtime field type '{field.FieldType}' is not supported.");
            return;
        }

        if (field.FieldType == DynamicFieldType.JsonArray)
        {
            ValidateArray(field, path, errors);
        }
        else
        {
            if (field.ElementType is not null)
            {
                Add(errors, DynamicMetadataValidationErrorCode.ElementTypeNotAllowed, $"{path}.elementType",
                    "Only a JsonArray field can declare an element type.");
            }

            ValidateOptions(field.FieldType == DynamicFieldType.Select, field.Options, path, errors);
        }
    }

    private static void ValidateArray(
        DynamicFieldDefinition field,
        string path,
        ICollection<DynamicMetadataValidationError> errors)
    {
        if (field.ElementType is null)
        {
            Add(errors, DynamicMetadataValidationErrorCode.ArrayElementTypeRequired, $"{path}.elementType",
                "A JsonArray field requires an element type.");
            ValidateOptions(false, field.Options, path, errors);
            return;
        }

        if (!Enum.IsDefined(field.ElementType.Value))
        {
            Add(errors, DynamicMetadataValidationErrorCode.UnsupportedArrayElementType, $"{path}.elementType",
                $"JsonArray element type '{field.ElementType}' is not supported.");
            ValidateOptions(false, field.Options, path, errors);
            return;
        }

        if (field.ElementType == DynamicFieldType.JsonArray)
        {
            Add(errors, DynamicMetadataValidationErrorCode.NestedArrayNotSupported, $"{path}.elementType",
                "Nested JsonArray fields are not supported.");
            ValidateOptions(false, field.Options, path, errors);
            return;
        }

        ValidateOptions(field.ElementType == DynamicFieldType.Select, field.Options, path, errors);
    }

    private static void ValidateOptions(
        bool isSelect,
        IReadOnlyCollection<string?>? options,
        string path,
        ICollection<DynamicMetadataValidationError> errors)
    {
        if (!isSelect)
        {
            if (options is { Count: > 0 })
            {
                Add(errors, DynamicMetadataValidationErrorCode.OptionsNotAllowed, $"{path}.options",
                    "Options can only be declared for select fields.");
            }

            return;
        }

        if (options is null || options.Count == 0)
        {
            Add(errors, DynamicMetadataValidationErrorCode.SelectOptionsRequired, $"{path}.options",
                "A select field requires at least one option.");
            return;
        }

        HashSet<string> values = new(StringComparer.Ordinal);
        int optionIndex = 0;
        foreach (string? option in options)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                Add(errors, DynamicMetadataValidationErrorCode.OptionRequired, $"{path}.options[{optionIndex}]",
                    "A select option cannot be blank.");
            }
            else if (!values.Add(option))
            {
                Add(errors, DynamicMetadataValidationErrorCode.DuplicateOption, $"{path}.options[{optionIndex}]",
                    $"Select option '{option}' is duplicated.");
            }

            optionIndex++;
        }
    }

    private static void Add(
        ICollection<DynamicMetadataValidationError> errors,
        DynamicMetadataValidationErrorCode code,
        string path,
        string message)
        => errors.Add(new(code, path, message));
}
