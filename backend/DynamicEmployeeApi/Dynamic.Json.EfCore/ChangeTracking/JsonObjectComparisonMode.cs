namespace Dynamic.Json.EfCore.ChangeTracking;

/// <summary>
/// Selects how <see cref="System.Text.Json.Nodes.JsonObject" /> values are compared for EF Core change tracking.
/// </summary>
public enum JsonObjectComparisonMode
{
    /// <summary>
    /// Compares JSON objects by their structured JSON values. Object property order is ignored, while array order is preserved.
    /// </summary>
    Semantic,

    /// <summary>
    /// Compares JSON objects by their serialized JSON text. This is faster, but object property order affects equality.
    /// </summary>
    Serialized,
}
