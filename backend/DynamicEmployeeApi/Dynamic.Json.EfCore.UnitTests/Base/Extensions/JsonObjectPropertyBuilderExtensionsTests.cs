using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.ChangeTracking;
using Dynamic.Json.EfCore.Metadata;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;

namespace Dynamic.Json.EfCore.UnitTests.Base.Extensions;

public class JsonObjectPropertyBuilderExtensionsTests
{
    [Fact]
    public void HasJsonConversion_DefaultMode_UsesSemanticComparer()
    {
        IMutableModel model = CreateModel(JsonObjectComparisonMode.Semantic);

        model.FindEntityType(typeof(TestRecord))!
            .FindProperty(nameof(TestRecord.Values))!
            .GetValueComparer()
            .Should().BeOfType<JsonObjectValueComparer>();
    }

    [Fact]
    public void HasJsonConversion_SerializedMode_UsesSerializedComparer()
    {
        IMutableModel model = CreateModel(JsonObjectComparisonMode.Serialized);

        model.FindEntityType(typeof(TestRecord))!
            .FindProperty(nameof(TestRecord.Values))!
            .GetValueComparer()
            .Should().BeOfType<SerializedJsonObjectValueComparer>();
    }

    private static IMutableModel CreateModel(JsonObjectComparisonMode comparisonMode)
    {
        ModelBuilder modelBuilder = new(new ConventionSet());

        modelBuilder.Entity<TestRecord>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Values).HasJsonConversion(comparisonMode);
        });

        return modelBuilder.Model;
    }

    private class TestRecord
    {
        public Guid Id { get; set; }
        public JsonObject Values { get; set; } = new();
    }
}
