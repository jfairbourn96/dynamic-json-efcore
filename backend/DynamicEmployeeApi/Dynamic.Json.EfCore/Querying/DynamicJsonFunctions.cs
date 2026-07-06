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
    public static string? Value(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");

    public static decimal? ValueDecimal(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");

    public static DateOnly? ValueDate(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");
}
