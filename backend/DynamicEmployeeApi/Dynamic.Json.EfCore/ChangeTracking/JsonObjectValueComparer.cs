using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dynamic.Json.EfCore.ChangeTracking;

/// <summary>
/// Compares and snapshots <see cref="JsonObject" /> values by serialized JSON content.
/// </summary>
/// <remarks>
/// EF Core uses this comparer to detect in-place mutations inside a JSON object property.
/// </remarks>
public class JsonObjectValueComparer() : ValueComparer<JsonObject>((a, b) => Serialize(a) == Serialize(b),
    v => v == null ? 0 : Serialize(v).GetHashCode(),
    v => Clone(v))
{
    private static string Serialize(JsonObject? obj)
        => obj is null ? "null" : JsonSerializer.Serialize(obj);

    private static JsonObject Clone(JsonObject obj)
        => JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(obj))!;
}
