using System.Reflection;
using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace Dynamic.Json.EfCore.SqlServer;

public sealed class DynamicJsonSqlServerMethodCallTranslatorProvider : SqlServerMethodCallTranslatorProvider
{
    public DynamicJsonSqlServerMethodCallTranslatorProvider(
        RelationalMethodCallTranslatorProviderDependencies dependencies,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, sqlServerSingletonOptions)
    {
        AddTranslators([new DynamicJsonFunctionsTranslator(dependencies.SqlExpressionFactory)]);
    }
}

internal sealed class DynamicJsonFunctionsTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo ValueMethod = typeof(DynamicJsonFunctions).GetMethod(
        nameof(DynamicJsonFunctions.Value),
        [typeof(JsonObject), typeof(string)])!;

    private static readonly MethodInfo ValueDecimalMethod = typeof(DynamicJsonFunctions).GetMethod(
        nameof(DynamicJsonFunctions.ValueDecimal),
        [typeof(JsonObject), typeof(string)])!;

    private static readonly MethodInfo ValueDateMethod = typeof(DynamicJsonFunctions).GetMethod(
        nameof(DynamicJsonFunctions.ValueDate),
        [typeof(JsonObject), typeof(string)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public DynamicJsonFunctionsTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == ValueMethod)
        {
            return JsonValue(arguments);
        }

        if (method == ValueDecimalMethod)
        {
            return TryConvert("decimal(18, 4)", JsonValue(arguments), typeof(decimal?));
        }

        if (method == ValueDateMethod)
        {
            return TryConvert("date", JsonValue(arguments), typeof(DateOnly?));
        }

        return null;
    }

    private SqlExpression JsonValue(IReadOnlyList<SqlExpression> arguments)
    {
        return _sqlExpressionFactory.Function(
            "JSON_VALUE",
            arguments,
            nullable: true,
            argumentsPropagateNullability: [true, true],
            typeof(string));
    }

    private SqlExpression TryConvert(string storeType, SqlExpression value, Type returnType)
    {
        return _sqlExpressionFactory.Function(
            "TRY_CONVERT",
            [new SqlFragmentExpression(storeType), value],
            nullable: true,
            argumentsPropagateNullability: [false, true],
            returnType);
    }
}
