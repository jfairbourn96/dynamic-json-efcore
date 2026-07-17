using System.Reflection;
using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dynamic.Json.EfCore.SqlServer;

/// <summary>
/// SQL Server method-call translator plugin that adds translations for Dynamic.Json.EfCore functions.
/// </summary>
public sealed class DynamicJsonSqlServerMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    /// <summary>
    /// Creates a translator plugin for Dynamic.Json.EfCore SQL Server JSON functions.
    /// </summary>
    public DynamicJsonSqlServerMethodCallTranslatorPlugin(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        Translators = [new DynamicJsonFunctionsTranslator(sqlExpressionFactory, typeMappingSource)];
    }

    /// <inheritdoc />
    public IEnumerable<IMethodCallTranslator> Translators { get; }
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

    private static readonly MethodInfo FragmentValueMethod = typeof(DynamicJsonFunctions).GetMethod(
        nameof(DynamicJsonFunctions.Value),
        [typeof(JsonFragment), typeof(string)])!;

    private static readonly MethodInfo FragmentValueDecimalMethod = typeof(DynamicJsonFunctions).GetMethod(
        nameof(DynamicJsonFunctions.ValueDecimal),
        [typeof(JsonFragment), typeof(string)])!;

    private static readonly MethodInfo FragmentValueDateMethod = typeof(DynamicJsonFunctions).GetMethod(
        nameof(DynamicJsonFunctions.ValueDate),
        [typeof(JsonFragment), typeof(string)])!;

    private static readonly MethodInfo FragmentMethod = typeof(DynamicJsonFunctions).GetMethod(
        "Fragment",
        BindingFlags.Static | BindingFlags.Public)!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _jsonFragmentTypeMapping;

    public DynamicJsonFunctionsTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _jsonFragmentTypeMapping = typeMappingSource.FindMapping(typeof(string))!;
    }

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == ValueMethod || method == FragmentValueMethod)
        {
            return JsonValue(arguments);
        }

        if (method == ValueDecimalMethod || method == FragmentValueDecimalMethod)
        {
            return TryConvert("decimal(18, 4)", JsonValue(arguments), typeof(decimal?));
        }

        if (method == ValueDateMethod || method == FragmentValueDateMethod)
        {
            return TryConvert("date", JsonValue(arguments), typeof(DateOnly?));
        }

        if (method == FragmentMethod && arguments[0] is SqlConstantExpression { Value: int scope })
        {
            return new JsonFragmentSqlExpression(
                _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(scope)),
                _jsonFragmentTypeMapping);
        }

        if (method.DeclaringType == typeof(DynamicJsonFunctions) &&
            method.Name == "TranslatedArrayAny" &&
            arguments[2] is SqlConstantExpression { Value: int arrayScope })
        {
            return new JsonArrayAnySqlExpression(
                arguments[0],
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(arrayScope)),
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[3]));
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
