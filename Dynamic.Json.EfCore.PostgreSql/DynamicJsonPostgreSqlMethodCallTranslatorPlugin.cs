using Microsoft.EntityFrameworkCore.Query;

namespace Dynamic.Json.EfCore.PostgreSql;

/// <summary>
/// PostgreSQL method-call translator plugin for Dynamic.Json.EfCore query functions.
/// </summary>
/// <remarks>
/// Individual scalar translators are added by their respective translation stories. Keeping the
/// plugin registered before those translators exist establishes the provider service boundary
/// without introducing function behavior prematurely.
/// </remarks>
public sealed class DynamicJsonPostgreSqlMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    /// <inheritdoc />
    public IEnumerable<IMethodCallTranslator> Translators { get; } = [];
}
