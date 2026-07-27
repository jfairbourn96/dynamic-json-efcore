using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Dynamic.Json.EfCore.PostgreSql;

/// <summary>
/// PostgreSQL method-call translator plugin for Dynamic.Json.EfCore query functions.
/// </summary>
public sealed class DynamicJsonPostgreSqlMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    /// <summary>Initializes the plugin with PostgreSQL scalar JSON translations.</summary>
    public DynamicJsonPostgreSqlMethodCallTranslatorPlugin(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        Translators =
        [
            new DynamicJsonPostgreSqlFunctionsTranslator(
                (NpgsqlSqlExpressionFactory)sqlExpressionFactory,
                typeMappingSource)
        ];
    }

    /// <inheritdoc />
    public IEnumerable<IMethodCallTranslator> Translators { get; }
}
