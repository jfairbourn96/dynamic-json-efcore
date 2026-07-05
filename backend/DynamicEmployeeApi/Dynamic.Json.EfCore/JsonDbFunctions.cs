using System.Text.Json.Nodes;

namespace Dynamic.Json.EfCore;

public static class JsonDbFunctions
{
    public static string? JsonValue(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");

    public static decimal? JsonValueDecimal(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");

    public static DateOnly? JsonValueDate(JsonObject json, string path)
        => throw new NotSupportedException("Only for use in EF Core queries.");
}
