using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Dynamic.Json.EfCore.SqlServer;

/// <summary>
/// Extension methods for enabling SQL Server translations for Dynamic.Json.EfCore query functions.
/// </summary>
public static class DynamicJsonSqlServerDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Registers SQL Server method translators for provider-neutral dynamic JSON query functions.
    /// </summary>
    /// <param name="builder">The EF Core options builder to configure.</param>
    /// <returns>The same options builder so configuration calls can be chained.</returns>
    public static DbContextOptionsBuilder UseDynamicJsonSqlServer(this DbContextOptionsBuilder builder)
    {
        return builder.ReplaceService<IMethodCallTranslatorProvider, DynamicJsonSqlServerMethodCallTranslatorProvider>();
    }
}
