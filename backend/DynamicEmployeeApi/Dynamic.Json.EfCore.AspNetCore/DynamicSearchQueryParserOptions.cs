namespace Dynamic.Json.EfCore.AspNetCore;

/// <summary>
/// Configures how dynamic search query parsing identifies parameters to ignore.
/// </summary>
public sealed class DynamicSearchQueryParserOptions
{
    /// <summary>
    /// Exact query-string keys that should be ignored by the dynamic search parser.
    /// </summary>
    public ISet<string> IgnoredKeys { get; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Query-string key prefixes that should be ignored by the dynamic search parser.
    /// </summary>
    public ISet<string> IgnoredKeyPrefixes { get; } = new HashSet<string>(StringComparer.Ordinal);
}
