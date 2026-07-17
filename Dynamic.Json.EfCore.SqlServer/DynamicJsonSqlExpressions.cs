using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Dynamic.Json.EfCore.Querying;

namespace Dynamic.Json.EfCore.SqlServer;

internal sealed class JsonFragmentSqlExpression(SqlExpression scope, RelationalTypeMapping? typeMapping)
    : SqlFunctionExpression(
        "__DYNAMIC_JSON_FRAGMENT",
        [scope],
        nullable: true,
        argumentsPropagateNullability: [false],
        typeof(JsonFragment),
        typeMapping)
{
    public int Scope => (int)((SqlConstantExpression)Arguments![0]).Value!;
}

internal sealed class JsonArrayAnySqlExpression(
    SqlExpression document,
    SqlExpression path,
    SqlExpression scope,
    SqlExpression predicate)
    : SqlFunctionExpression(
        "__DYNAMIC_JSON_ARRAY_ANY",
        [document, path, scope, predicate],
        nullable: false,
        argumentsPropagateNullability: [false, false, false, false],
        typeof(bool),
        predicate.TypeMapping)
{
    public SqlExpression Document => Arguments![0];
    public SqlExpression Path => Arguments![1];
    public int Scope => (int)((SqlConstantExpression)Arguments![2]).Value!;
    public SqlExpression Predicate => Arguments![3];
}
