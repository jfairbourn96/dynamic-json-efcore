using Microsoft.EntityFrameworkCore.Query;

namespace Dynamic.Json.EfCore.SqlServer;

/// <summary>
/// SQL Server method-call translator plugin that adds translations for Dynamic.Json.EfCore functions.
/// </summary>
public sealed class DynamicJsonSqlServerMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    /// <summary>
    /// Creates a translator plugin for Dynamic.Json.EfCore SQL Server JSON functions.
    /// </summary>
    public DynamicJsonSqlServerMethodCallTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory)
    {
        Translators = [new DynamicJsonFunctionsTranslator(sqlExpressionFactory)];
    }

    /// <inheritdoc />
    public IEnumerable<IMethodCallTranslator> Translators { get; }
}
