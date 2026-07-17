using System.Linq.Expressions;
using System.Reflection;
using Dynamic.Json.EfCore.Querying;
using Microsoft.EntityFrameworkCore.Query;

namespace Dynamic.Json.EfCore.SqlServer;

internal sealed class DynamicJsonQueryTranslationPreprocessorFactory(
    QueryTranslationPreprocessorDependencies dependencies,
    RelationalQueryTranslationPreprocessorDependencies relationalDependencies)
    : IQueryTranslationPreprocessorFactory
{
    public QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
        => new DynamicJsonQueryTranslationPreprocessor(
            dependencies,
            relationalDependencies,
            queryCompilationContext);
}

internal sealed class DynamicJsonQueryTranslationPreprocessor(
    QueryTranslationPreprocessorDependencies dependencies,
    RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
    QueryCompilationContext queryCompilationContext)
    : RelationalQueryTranslationPreprocessor(dependencies, relationalDependencies, queryCompilationContext)
{
    public override Expression Process(Expression query)
        => base.Process(new ArrayAnyRewritingVisitor().Visit(query));
}

internal sealed class ArrayAnyRewritingVisitor : ExpressionVisitor
{
    private static readonly MethodInfo FragmentMethod = typeof(DynamicJsonFunctions)
        .GetMethod("Fragment", BindingFlags.Static | BindingFlags.Public)!;

    private int _nextScope;

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType != typeof(DynamicJsonFunctions) ||
            node.Method.Name != nameof(DynamicJsonFunctions.ArrayAny))
        {
            return base.VisitMethodCall(node);
        }

        int scope = _nextScope++;
        Expression document = Visit(node.Arguments[0]);
        Expression path;
        Expression predicateExpression;

        if (node.Arguments.Count == 2)
        {
            path = Expression.Constant("$");
            predicateExpression = node.Arguments[1];
        }
        else
        {
            path = Visit(node.Arguments[1]);
            predicateExpression = node.Arguments[2];
        }

        LambdaExpression predicate = UnwrapLambda(predicateExpression);
        Expression fragment = Expression.Call(FragmentMethod, Expression.Constant(scope));
        Expression predicateBody = new ParameterReplacingVisitor(predicate.Parameters[0], fragment)
            .Visit(predicate.Body);
        predicateBody = Visit(predicateBody);

        MethodInfo translatedMethod = typeof(DynamicJsonFunctions)
            .GetMethod(
                "TranslatedArrayAny",
                BindingFlags.Static | BindingFlags.Public,
                [document.Type, typeof(string), typeof(int), typeof(bool)])!;

        return Expression.Call(
            translatedMethod,
            document,
            path,
            Expression.Constant(scope),
            predicateBody);
    }

    private static LambdaExpression UnwrapLambda(Expression expression)
    {
        if (expression.NodeType == ExpressionType.Quote)
        {
            expression = ((UnaryExpression)expression).Operand;
        }

        if (expression is ConstantExpression { Value: LambdaExpression constantLambda })
        {
            return constantLambda;
        }

        return (LambdaExpression)expression;
    }

    private sealed class ParameterReplacingVisitor(ParameterExpression parameter, Expression replacement)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == parameter ? replacement : base.VisitParameter(node);
    }
}
