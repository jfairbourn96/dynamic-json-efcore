using System.Text.Json;
using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.ChangeTracking;
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
    /// <param name="builder">The property builder for the JSON object property.</param>
    /// <returns>The same property builder so configuration calls can be chained.</returns>
    public static PropertyBuilder<JsonObject> HasJsonConversion(
        this PropertyBuilder<JsonObject> builder)
    {
        builder
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<JsonObject>(v, (JsonSerializerOptions?)null) ?? new JsonObject(),
                new JsonObjectValueComparer());

        return builder;
    }
}
