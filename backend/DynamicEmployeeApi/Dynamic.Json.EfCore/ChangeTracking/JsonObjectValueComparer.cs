using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dynamic.Json.EfCore.ChangeTracking;

/// <summary>
/// Compares and snapshots <see cref="JsonObject" /> values by structured JSON content.
/// </summary>
/// <remarks>
/// EF Core uses this comparer to detect in-place mutations inside a JSON object property.
/// Object property order is ignored, nested objects are compared recursively, and array
/// order remains significant.
/// </remarks>
public class JsonObjectValueComparer() : ValueComparer<JsonObject>((a, b) => AreEqual(a, b),
    v => GetJsonHashCode(v),
    v => Clone(v))
{
    private static bool AreEqual(JsonNode? left, JsonNode? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (left is JsonObject leftObject && right is JsonObject rightObject)
        {
            if (leftObject.Count != rightObject.Count)
            {
                return false;
            }

            foreach ((string key, JsonNode? leftValue) in leftObject)
            {
                if (!rightObject.TryGetPropertyValue(key, out JsonNode? rightValue) ||
                    !AreEqual(leftValue, rightValue))
                {
                    return false;
                }
            }

            return true;
        }

        if (left is JsonArray leftArray && right is JsonArray rightArray)
        {
            if (leftArray.Count != rightArray.Count)
            {
                return false;
            }

            for (int i = 0; i < leftArray.Count; i++)
            {
                if (!AreEqual(leftArray[i], rightArray[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return left.GetType() == right.GetType() &&
            StringComparer.Ordinal.Equals(left.ToJsonString(), right.ToJsonString());
    }

    private static int GetJsonHashCode(JsonNode? node)
    {
        if (node is null)
        {
            return 0;
        }

        HashCode hash = new();

        if (node is JsonObject jsonObject)
        {
            foreach ((string key, JsonNode? value) in jsonObject.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                hash.Add(key, StringComparer.Ordinal);
                hash.Add(GetJsonHashCode(value));
            }

            return hash.ToHashCode();
        }

        if (node is JsonArray jsonArray)
        {
            foreach (JsonNode? value in jsonArray)
            {
                hash.Add(GetJsonHashCode(value));
            }

            return hash.ToHashCode();
        }

        hash.Add(node.GetType());
        hash.Add(node.ToJsonString(), StringComparer.Ordinal);

        return hash.ToHashCode();
    }

    private static JsonObject Clone(JsonObject obj)
        => JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(obj))!;
}
