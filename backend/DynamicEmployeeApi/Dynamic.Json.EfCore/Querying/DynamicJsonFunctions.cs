using System.Text.Json.Nodes;

namespace Dynamic.Json.EfCore.Querying;

/// <summary>
/// Provider-neutral JSON query functions for use in EF Core LINQ expressions.
/// </summary>
/// <remarks>
/// These methods are not evaluated in .NET. Database provider packages translate them into
/// provider-specific SQL, such as SQL Server JSON_VALUE or PostgreSQL JSON operators.
/// </remarks>
public static class DynamicJsonFunctions
{
    /// <summary>
    /// Reads a scalar JSON value as text from the supplied JSON path.
    /// </summary>
    /// <param name="json">The JSON document expression to read from.</param>
    /// <param name="path">The provider-specific JSON path expression.</param>
    /// <returns>The scalar JSON value as text, or <see langword="null" /> when no value exists.</returns>
    public static string? Value(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");

    /// <summary>
    /// Reads and converts a scalar JSON value to a nullable decimal.
    /// </summary>
    /// <param name="json">The JSON document expression to read from.</param>
    /// <param name="path">The provider-specific JSON path expression.</param>
    /// <returns>The converted decimal value, or <see langword="null" /> when conversion fails or no value exists.</returns>
    public static decimal? ValueDecimal(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");

    /// <summary>
    /// Reads and converts a scalar JSON value to a nullable date.
    /// </summary>
    /// <param name="json">The JSON document expression to read from.</param>
    /// <param name="path">The provider-specific JSON path expression.</param>
    /// <returns>The converted date value, or <see langword="null" /> when conversion fails or no value exists.</returns>
    public static DateOnly? ValueDate(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");
}
