using System.Reflection;
using Dynamic.Json.EfCore.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Dynamic.Json.EfCore.PostgreSql;

/// <summary>
/// Translates provider-neutral scalar JSON marker methods into PostgreSQL expressions.
/// </summary>
internal sealed class DynamicJsonPostgreSqlFunctionsTranslator : IMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _jsonPathMapping;
    private readonly RelationalTypeMapping _textMapping;
    private readonly RelationalTypeMapping _decimalMapping;
    private readonly RelationalTypeMapping _dateMapping;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicJsonPostgreSqlFunctionsTranslator" /> class.
    /// </summary>
    /// <param name="sqlExpressionFactory">The Npgsql SQL expression factory.</param>
    /// <param name="typeMappingSource">The provider type-mapping source.</param>
    public DynamicJsonPostgreSqlFunctionsTranslator(
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _jsonPathMapping = GetMapping(typeMappingSource, "jsonpath");
        _textMapping = GetMapping(typeMappingSource, "text");
        _decimalMapping = GetMapping(typeMappingSource, "numeric");
        _dateMapping = GetMapping(typeMappingSource, "date");
    }

    /// <inheritdoc />
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == DynamicJsonScalarMethods.Value)
        {
            return JsonValue(arguments);
        }

        if (method == DynamicJsonScalarMethods.ValueDecimal)
        {
            return SafeConvert(JsonValue(arguments), _decimalMapping, typeof(decimal?));
        }

        if (method == DynamicJsonScalarMethods.ValueDate)
        {
            return SafeConvert(JsonValue(arguments), _dateMapping, typeof(DateOnly?));
        }

        return null;
    }

    /// <summary>Extracts a nullable scalar as text using the portable JSON path.</summary>
    private SqlExpression JsonValue(IReadOnlyList<SqlExpression> arguments)
    {
        SqlExpression path = arguments[1];
        if (path is SqlConstantExpression { Value: string constantPath })
        {
            path = _sqlExpressionFactory.Constant(DynamicJsonPath.Normalize(constantPath));
        }

        SqlExpression json = _sqlExpressionFactory.Function(
            "jsonb_path_query_first",
            [arguments[0], _sqlExpressionFactory.Convert(path, typeof(string), _jsonPathMapping)],
            nullable: true,
            argumentsPropagateNullability: [true, true],
            typeof(string),
            arguments[0].TypeMapping);

        return _sqlExpressionFactory.JsonTraversal(
            json,
            Array.Empty<SqlExpression>(),
            returnsText: true,
            typeof(string),
            _textMapping);
    }

    /// <summary>Converts text only when PostgreSQL reports that the input is valid.</summary>
    private SqlExpression SafeConvert(
        SqlExpression value,
        RelationalTypeMapping targetMapping,
        Type returnType)
    {
        SqlExpression isValid = _sqlExpressionFactory.Function(
            "pg_input_is_valid",
            [value, _sqlExpressionFactory.Constant(targetMapping.StoreType)],
            nullable: true,
            argumentsPropagateNullability: [true, false],
            typeof(bool));
        SqlExpression converted = _sqlExpressionFactory.Convert(value, returnType, targetMapping);

        return _sqlExpressionFactory.Case(
            [new CaseWhenClause(isValid, converted)],
            elseResult: null);
    }

    /// <summary>Gets a required PostgreSQL store-type mapping.</summary>
    private static RelationalTypeMapping GetMapping(
        IRelationalTypeMappingSource typeMappingSource,
        string storeType)
        => typeMappingSource.FindMapping(storeType)
            ?? throw new InvalidOperationException($"PostgreSQL type mapping '{storeType}' was not found.");
}
