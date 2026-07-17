using System.Reflection;
using Dynamic.Json.EfCore.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Dynamic.Json.EfCore.SqlServer;

/// <summary>
/// Translates provider-neutral scalar JSON marker methods into SQL Server expressions.
/// </summary>
/// <remarks>
/// Method identity comes from <see cref="DynamicJsonScalarMethods" />, while this class owns
/// SQL Server-specific function names, conversion store types, and null propagation metadata.
/// </remarks>
internal sealed class DynamicJsonFunctionsTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicJsonFunctionsTranslator" /> class.
    /// </summary>
    /// <param name="sqlExpressionFactory">
    /// The SQL Server expression factory used to construct translated query expressions.
    /// </param>
    public DynamicJsonFunctionsTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    /// Translates a recognized dynamic JSON scalar method call into its SQL Server equivalent.
    /// </summary>
    /// <param name="instance">The method-call instance, which is unused for static marker methods.</param>
    /// <param name="method">The method being considered for translation.</param>
    /// <param name="arguments">The translated JSON document and path arguments.</param>
    /// <param name="logger">The EF Core query diagnostics logger.</param>
    /// <returns>
    /// A SQL Server expression for a recognized scalar marker method; otherwise,
    /// <see langword="null" /> so another translator can handle the method.
    /// </returns>
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
            return TryConvert("decimal(18, 4)", JsonValue(arguments), typeof(decimal?));
        }

        if (method == DynamicJsonScalarMethods.ValueDate)
        {
            return TryConvert("date", JsonValue(arguments), typeof(DateOnly?));
        }

        return null;
    }

    /// <summary>
    /// Creates a nullable SQL Server <c>JSON_VALUE</c> expression.
    /// </summary>
    /// <param name="arguments">The JSON document and JSON path SQL expressions.</param>
    /// <returns>An expression that reads the selected scalar value as text.</returns>
    private SqlExpression JsonValue(IReadOnlyList<SqlExpression> arguments)
    {
        return _sqlExpressionFactory.Function(
            "JSON_VALUE",
            arguments,
            nullable: true,
            argumentsPropagateNullability: [true, true],
            typeof(string));
    }

    /// <summary>
    /// Creates a nullable SQL Server <c>TRY_CONVERT</c> expression around a scalar value.
    /// </summary>
    /// <param name="storeType">The SQL Server target store type, such as <c>date</c>.</param>
    /// <param name="value">The scalar SQL expression to convert.</param>
    /// <param name="returnType">The nullable CLR type produced by the conversion.</param>
    /// <returns>An expression that returns <see langword="null" /> when conversion fails.</returns>
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
