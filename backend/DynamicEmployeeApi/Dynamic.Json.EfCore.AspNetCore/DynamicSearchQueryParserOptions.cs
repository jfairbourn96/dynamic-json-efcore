namespace Dynamic.Json.EfCore.AspNetCore;

public sealed class DynamicSearchQueryParserOptions
{
    public ISet<string> IgnoredKeys { get; } = new HashSet<string>(StringComparer.Ordinal);

    public ISet<string> IgnoredKeyPrefixes { get; } = new HashSet<string>(StringComparer.Ordinal);
}
