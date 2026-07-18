using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Dynamic.Json.EfCore.PostgreSql;

/// <summary>
/// Extension methods for enabling PostgreSQL support for Dynamic.Json.EfCore query functions.
/// </summary>
public static class DynamicJsonPostgreSqlDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds the Dynamic.Json PostgreSQL options extension to the context configuration.
    /// </summary>
    /// <param name="builder">The EF Core options builder to configure.</param>
    /// <returns>The same options builder so configuration calls can be chained.</returns>
    /// <remarks>
    /// This registration establishes the PostgreSQL provider boundary and adds its translator
    /// plugin to EF Core. Individual scalar translations are introduced separately.
    /// </remarks>
    public static DbContextOptionsBuilder UseDynamicJsonPostgreSql(this DbContextOptionsBuilder builder)
    {
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(
            new DynamicJsonPostgreSqlOptionsExtension());

        return builder;
    }
}
