using System.Text.Json;
using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dynamic.Json.EfCore.Metadata;

/// <summary>
/// Extension methods for configuring <see cref="JsonObject" /> properties in EF Core models.
/// </summary>
public static class JsonObjectPropertyBuilderExtensions
{
    /// <summary>
    /// Stores a <see cref="JsonObject" /> property as serialized JSON and configures deep change tracking.
    /// </summary>
    /// <remarks>
    /// Use this from <c>DbContext.OnModelCreating</c> when a model property should be persisted
    /// as JSON text while still allowing EF Core to detect in-place mutations to the
    /// <see cref="JsonObject" /> instance.
    /// </remarks>
    /// <param name="builder">The property builder for the JSON object property being configured.</param>
    /// <param name="comparisonMode">
    /// The comparison strategy EF Core should use for change tracking. The default
    /// <see cref="JsonObjectComparisonMode.Semantic" /> mode ignores JSON object property order.
    /// Use <see cref="JsonObjectComparisonMode.Serialized" /> for faster, property-order-sensitive comparison.
    /// </param>
    /// <returns>The same property builder so configuration calls can be chained.</returns>
    public static PropertyBuilder<JsonObject> HasJsonConversion(
        this PropertyBuilder<JsonObject> builder,
        JsonObjectComparisonMode comparisonMode = JsonObjectComparisonMode.Semantic)
    {
        ValueComparer<JsonObject> comparer = comparisonMode switch
        {
            JsonObjectComparisonMode.Semantic => new JsonObjectValueComparer(),
            JsonObjectComparisonMode.Serialized => new SerializedJsonObjectValueComparer(),
            _ => throw new ArgumentOutOfRangeException(nameof(comparisonMode), comparisonMode, null)
        };

        builder
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<JsonObject>(v, (JsonSerializerOptions?)null) ?? new JsonObject(),
                comparer);

        return builder;
    }
}
