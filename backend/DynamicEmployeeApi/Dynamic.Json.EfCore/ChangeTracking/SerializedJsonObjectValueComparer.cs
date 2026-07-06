using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dynamic.Json.EfCore.ChangeTracking;

/// <summary>
/// Compares and snapshots <see cref="JsonObject" /> values by serialized JSON text.
/// </summary>
/// <remarks>
/// This comparer is optimized for simple text comparison. It is property-order-sensitive,
/// so two JSON objects with the same properties in different orders are treated as different values.
/// </remarks>
public class SerializedJsonObjectValueComparer() : ValueComparer<JsonObject>(
    (a, b) => Serialize(a) == Serialize(b),
    v => v == null ? 0 : Serialize(v).GetHashCode(StringComparison.Ordinal),
    v => Clone(v))
{
    private static string Serialize(JsonObject? obj)
        => obj is null ? "null" : JsonSerializer.Serialize(obj);

    private static JsonObject Clone(JsonObject obj)
        => JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(obj))!;
}
