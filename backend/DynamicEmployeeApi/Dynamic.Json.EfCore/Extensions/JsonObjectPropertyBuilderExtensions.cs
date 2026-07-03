using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dynamic.Json.EfCore.Extensions;

public static class JsonObjectPropertyBuilderExtensions
{
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
