using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dynamic.Json.EfCore.SqlServer;

#pragma warning disable EF1001
internal sealed class DynamicJsonSqlServerQuerySqlGeneratorFactory(
    QuerySqlGeneratorDependencies dependencies,
    IRelationalTypeMappingSource typeMappingSource,
    ISqlServerSingletonOptions sqlServerSingletonOptions)
    : IQuerySqlGeneratorFactory
{
    public QuerySqlGenerator Create()
        => new DynamicJsonSqlServerQuerySqlGenerator(
            dependencies,
            typeMappingSource,
            sqlServerSingletonOptions);
}

internal sealed class DynamicJsonSqlServerQuerySqlGenerator(
    QuerySqlGeneratorDependencies dependencies,
    IRelationalTypeMappingSource typeMappingSource,
    ISqlServerSingletonOptions sqlServerSingletonOptions)
    : SqlServerQuerySqlGenerator(dependencies, typeMappingSource, sqlServerSingletonOptions)
{
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case SqlFunctionExpression
                {
                    Name: "__DYNAMIC_JSON_FRAGMENT",
                    Arguments: [SqlConstantExpression { Value: int fragmentScope }]
                }:
                Sql.Append("[json_")
                    .Append(fragmentScope.ToString())
                    .Append("].[value]");
                return extensionExpression;

            case SqlFunctionExpression
                {
                    Name: "__DYNAMIC_JSON_ARRAY_ANY",
                    Arguments:
                    [
                        SqlExpression document,
                        SqlExpression path,
                        SqlConstantExpression { Value: int arrayScope },
                        SqlExpression predicate
                    ]
                }:
                Sql.Append("EXISTS (")
                    .AppendLine()
                    .IncrementIndent()
                    .Append("SELECT 1")
                    .AppendLine()
                    .Append("FROM OPENJSON(");
                Visit(document);
                Sql.Append(", ");
                Visit(path);
                Sql.Append(") AS [json_")
                    .Append(arrayScope.ToString())
                    .Append("]")
                    .AppendLine()
                    .Append("WHERE ");
                Visit(predicate);
                Sql.DecrementIndent()
                    .AppendLine()
                    .Append(")");
                return extensionExpression;

            default:
                return base.VisitExtension(extensionExpression);
        }
    }
}
#pragma warning restore EF1001
