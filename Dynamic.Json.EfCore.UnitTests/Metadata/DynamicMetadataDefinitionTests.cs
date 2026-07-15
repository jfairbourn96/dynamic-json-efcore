using System.Text.Json;
using Dynamic.Json.Metadata;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.UnitTests.Metadata;

public sealed class DynamicMetadataTests
{
    public static TheoryData<DynamicFieldDefinition, DynamicMetadataValidationErrorCode, string> InvalidFields => new()
    {
        { new(" ", DynamicFieldType.Text), DynamicMetadataValidationErrorCode.FieldNameRequired, "fields[0].name" },
        { new("demonAura", (DynamicFieldType)999), DynamicMetadataValidationErrorCode.UnsupportedFieldType, "fields[0].fieldType" },
        { new("huntrixMembers", DynamicFieldType.JsonArray), DynamicMetadataValidationErrorCode.ArrayElementTypeRequired, "fields[0].elementType" },
        { new("sajaPowers", DynamicFieldType.JsonArray, ElementType: (DynamicFieldType)999), DynamicMetadataValidationErrorCode.UnsupportedArrayElementType, "fields[0].elementType" },
        { new("demonSquads", DynamicFieldType.JsonArray, ElementType: DynamicFieldType.JsonArray), DynamicMetadataValidationErrorCode.NestedArrayNotSupported, "fields[0].elementType" },
        { new("stageName", DynamicFieldType.Text, ElementType: DynamicFieldType.Text), DynamicMetadataValidationErrorCode.ElementTypeNotAllowed, "fields[0].elementType" },
        { new("favoriteTrack", DynamicFieldType.Select), DynamicMetadataValidationErrorCode.SelectOptionsRequired, "fields[0].options" },
        { new("honmoonStatus", DynamicFieldType.Text, Options: ["Golden"]), DynamicMetadataValidationErrorCode.OptionsNotAllowed, "fields[0].options" },
        { new("favoriteTrack", DynamicFieldType.Select, Options: [" "]), DynamicMetadataValidationErrorCode.OptionRequired, "fields[0].options[0]" },
        { new("favoriteTrack", DynamicFieldType.Select, Options: ["Golden", "Golden"]), DynamicMetadataValidationErrorCode.DuplicateOption, "fields[0].options[1]" },
    };

    [Fact]
    public void Validate_JsonArrayWithScalarElementType_IsValid()
    {
        IReadOnlyCollection<DynamicFieldDefinition?> fields = [
            new("demonHuntingSkills", DynamicFieldType.JsonArray, true, DynamicFieldType.Text),
            new("huntrixMembers", DynamicFieldType.JsonArray, ElementType: DynamicFieldType.Select, Options: ["Rumi", "Mira", "Zoey"]),
        ];

        DynamicMetadataValidationResult result = DynamicMetadataValidator.Validate(fields);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void JsonSerializer_JsonArrayMetadata_RoundTrips()
    {
        DynamicFieldDefinition[] fields = [
            new("fanChants", DynamicFieldType.JsonArray, ElementType: DynamicFieldType.Text),
            new("favoriteTracks", DynamicFieldType.JsonArray, ElementType: DynamicFieldType.Select, Options: ["Golden", "Takedown"]),
        ];

        string json = JsonSerializer.Serialize(fields);
        DynamicFieldDefinition[]? result = JsonSerializer.Deserialize<DynamicFieldDefinition[]>(json);

        result.Should().BeEquivalentTo(fields);
    }

    [Fact]
    public void JsonSerializer_ExistingScalarMetadata_RoundTripsUnchanged()
    {
        DynamicFieldDefinition[] fields = [
            new("stageName", DynamicFieldType.Text, true),
            new("demonsDefeated", DynamicFieldType.Number),
            new("debutDate", DynamicFieldType.Date),
            new("honmoonRestored", DynamicFieldType.Boolean),
            new("favoriteTrack", DynamicFieldType.Select, Options: ["Golden", "How It's Done"]),
        ];

        string json = JsonSerializer.Serialize(fields);
        DynamicFieldDefinition[]? result = JsonSerializer.Deserialize<DynamicFieldDefinition[]>(json);

        result.Should().BeEquivalentTo(fields);
        DynamicMetadataValidator.Validate(result).IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidFields))]
    public void Validate_InvalidField_ReturnsDetailedError(
        DynamicFieldDefinition field,
        DynamicMetadataValidationErrorCode expectedCode,
        string expectedPath)
    {
        DynamicMetadataValidationResult result = DynamicMetadataValidator.Validate([field]);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Code == expectedCode && error.Path == expectedPath);
    }

    [Fact]
    public void Validate_NullFields_ReturnsFieldsRequired()
    {
        DynamicMetadataValidationResult result = DynamicMetadataValidator.Validate(null);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be(DynamicMetadataValidationErrorCode.FieldsRequired);
    }

    [Fact]
    public void Validate_NullField_ReturnsIndexedError()
    {
        DynamicMetadataValidationResult result = DynamicMetadataValidator.Validate([null]);

        result.Errors.Should().ContainSingle().Which.Should().Be(new DynamicMetadataValidationError(
            DynamicMetadataValidationErrorCode.NullField, "fields[0]", "A metadata field cannot be null."));
    }

    [Fact]
    public void Validate_DuplicateFieldNames_ReturnsReferenceToFirstField()
    {
        IReadOnlyCollection<DynamicFieldDefinition?> fields = [
            new("stageName", DynamicFieldType.Text),
            new("stageName", DynamicFieldType.Number),
        ];

        DynamicMetadataValidationResult result = DynamicMetadataValidator.Validate(fields);

        result.Errors.Should().ContainSingle().Which.Should().Be(new DynamicMetadataValidationError(
            DynamicMetadataValidationErrorCode.DuplicateFieldName,
            "fields[1].name",
            "Field name 'stageName' duplicates fields[0].name."));
    }

    [Fact]
    public void Validate_MultipleInvalidFields_ReturnsAllErrors()
    {
        IReadOnlyCollection<DynamicFieldDefinition?> fields = [
            new(" ", DynamicFieldType.Select),
            null,
            new("honmoonStatus", DynamicFieldType.Text, Options: ["Golden"]),
        ];

        DynamicMetadataValidationResult result = DynamicMetadataValidator.Validate(fields);

        result.Errors.Select(error => error.Code).Should().Equal(
            DynamicMetadataValidationErrorCode.FieldNameRequired,
            DynamicMetadataValidationErrorCode.SelectOptionsRequired,
            DynamicMetadataValidationErrorCode.NullField,
            DynamicMetadataValidationErrorCode.OptionsNotAllowed);
    }

    [Fact]
    public void JsonSerializer_InvalidMetadata_DeserializesForExplicitValidation()
    {
        const string json = """[{"Name":"huntrixMembers","FieldType":5,"Required":false,"ElementType":null,"Options":null}]""";

        DynamicFieldDefinition[]? fields = JsonSerializer.Deserialize<DynamicFieldDefinition[]>(json);
        DynamicMetadataValidationResult result = DynamicMetadataValidator.Validate(fields);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be(DynamicMetadataValidationErrorCode.ArrayElementTypeRequired);
    }

    [Fact]
    public void ValidateAndThrow_ValidMetadata_ReturnsSameDefinition()
    {
        IReadOnlyCollection<DynamicFieldDefinition?> fields = [new("stageName", DynamicFieldType.Text)];

        IReadOnlyCollection<DynamicFieldDefinition?> result = DynamicMetadataValidator.ValidateAndThrow(fields);

        result.Should().BeSameAs(fields);
    }

    [Fact]
    public void ValidateAndThrow_InvalidMetadata_ThrowsWithCompleteResult()
    {
        IReadOnlyCollection<DynamicFieldDefinition?> fields = [
            new(" ", DynamicFieldType.Select),
            new("honmoonStatus", DynamicFieldType.Text, Options: ["Golden"]),
        ];

        Action act = () => DynamicMetadataValidator.ValidateAndThrow(fields);

        DynamicMetadataValidationException exception = act.Should()
            .Throw<DynamicMetadataValidationException>()
            .WithMessage("Runtime metadata validation failed with 3 error(s).")
            .Which;
        exception.ValidationResult.Errors.Should().HaveCount(3);
    }
}
