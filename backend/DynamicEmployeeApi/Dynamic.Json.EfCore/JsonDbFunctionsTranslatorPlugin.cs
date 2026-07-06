using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace Dynamic.Json.EfCore;

public sealed class DynamicJsonSqlServerMethodCallTranslatorProvider : SqlServerMethodCallTranslatorProvider
{
    public DynamicJsonSqlServerMethodCallTranslatorProvider(
        RelationalMethodCallTranslatorProviderDependencies dependencies,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, sqlServerSingletonOptions)
    {
        AddTranslators([new JsonDbFunctionsTranslator(dependencies.SqlExpressionFactory)]);
    }
}

internal sealed class JsonDbFunctionsTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo JsonValueMethod = typeof(JsonDbFunctions).GetMethod(
        nameof(JsonDbFunctions.JsonValue),
        [typeof(JsonObject), typeof(string)])!;

    private static readonly MethodInfo JsonValueDecimalMethod = typeof(JsonDbFunctions).GetMethod(
        nameof(JsonDbFunctions.JsonValueDecimal),
        [typeof(JsonObject), typeof(string)])!;

    private static readonly MethodInfo JsonValueDateMethod = typeof(JsonDbFunctions).GetMethod(
        nameof(JsonDbFunctions.JsonValueDate),
        [typeof(JsonObject), typeof(string)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public JsonDbFunctionsTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == JsonValueMethod)
        {
            return JsonValue(arguments);
        }

        if (method == JsonValueDecimalMethod)
        {
            return TryConvert("decimal(18, 4)", JsonValue(arguments), typeof(decimal?));
        }

        if (method == JsonValueDateMethod)
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
