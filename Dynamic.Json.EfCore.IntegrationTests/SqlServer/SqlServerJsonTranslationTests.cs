using Dynamic.Json.EfCore.Querying;
using Dynamic.Json.EfCore.SqlServer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.SqlServer;

public sealed class SqlServerJsonTranslationTests
{
    [Fact]
    public void Value_GeneratesJsonValueSql()
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();

        string sql = context.Records
            .Where(r => DynamicJsonFunctions.Value(r.Values, "$.color") == "orange")
            .ToQueryString();

        sql.Should().Contain("JSON_VALUE([r].[Values], N'$.color')");
    }

    [Fact]
    public void ValueDecimal_GeneratesTryConvertDecimalSql()
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();

        string sql = context.Records
            .Where(r => DynamicJsonFunctions.ValueDecimal(r.Values, "$.age") >= 7m)
            .ToQueryString();

        sql.Should().Contain(
            "TRY_CONVERT(decimal(18, 4), JSON_VALUE([r].[Values], N'$.age'))");
    }

    [Fact]
    public void ValueDate_GeneratesTryConvertDateSql()
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();

        string sql = context.Records
            .Where(r => DynamicJsonFunctions.ValueDate(r.Values, "$.birthday") >= new DateOnly(2018, 1, 1))
            .ToQueryString();

        sql.Should().Contain(
            "TRY_CONVERT(date, JSON_VALUE([r].[Values], N'$.birthday'))");
    }

    [Fact]
    public void ScalarQuery_WithoutDynamicJsonRegistration_FailsTranslation()
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext(registerDynamicJson: false);

        Action act = () => context.Records
            .Where(r => DynamicJsonFunctions.Value(r.Values, "$.color") == "orange")
            .ToQueryString();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*could not be translated*");
    }

    [Theory]
    [InlineData("$.huntrix.leader")]
    [InlineData("$.\"stage.name\"")]
    [InlineData("$.\"demon\\\"hunter\"")]
    public void Value_PortablePropertyPath_PreservesCanonicalPathInSql(string path)
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();

        string sql = CreateConstantPathQuery(context, path).ToQueryString();

        sql.Should().Contain(path);
    }

    [Fact]
    public void Value_PathCreatedFromRuntimePropertyName_PreservesEscapingInSqlParameter()
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();
        string path = DynamicJsonPath.FromProperty("stage.name");

        string sql = context.Records
            .Where(r => DynamicJsonFunctions.Value(r.Values, path) == "expected")
            .ToQueryString();

        sql.Should().Contain("$.\"stage.name\"");
    }

    [Fact]
    public void Value_UntrustedPropertyName_RemainsOneParameterizedPropertySegment()
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();
        string path = DynamicJsonPath.FromProperty("stage' OR 1=1--");

        string sql = context.Records
            .Where(r => DynamicJsonFunctions.Value(r.Values, path) == "Rumi")
            .ToQueryString();

        DynamicJsonPath.ParseProperties(path).Should().Equal("stage' OR 1=1--");
        sql.Should().Contain("$.\"stage'' OR 1=1--\"");
        sql.Should().MatchRegex(@"JSON_VALUE\([^,]+, @[A-Za-z0-9_]+\)");
    }

    [Fact]
    public void ScalarQueries_CapturedPathsAndComparisonValues_RemainParameterized()
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();
        string textPath = DynamicJsonPath.FromProperty("stage.name");
        string expectedText = "Rumi";
        string decimalPath = DynamicJsonPath.FromProperty("score");
        decimal minimumDecimal = 7.5m;
        string datePath = DynamicJsonPath.FromProperty("debutDate");
        DateOnly minimumDate = new(2026, 7, 27);

        string textSql = context.Records
            .Where(record => DynamicJsonFunctions.Value(record.Values, textPath) == expectedText)
            .ToQueryString();
        string decimalSql = context.Records
            .Where(record => DynamicJsonFunctions.ValueDecimal(record.Values, decimalPath) >= minimumDecimal)
            .ToQueryString();
        string dateSql = context.Records
            .Where(record => DynamicJsonFunctions.ValueDate(record.Values, datePath) >= minimumDate)
            .ToQueryString();

        AssertCapturedParameters(textSql, "=");
        AssertCapturedParameters(decimalSql, ">=");
        AssertCapturedParameters(dateSql, ">=");
    }

    [Theory]
    [InlineData("$.items[0]")]
    [InlineData("$.*")]
    [InlineData("strict $.name")]
    public void Value_UnsupportedConstantPath_ThrowsPortablePathException(string path)
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();

        Action act = () => CreateConstantPathQuery(context, path).ToQueryString();

        act.Should().Throw<DynamicJsonPathException>();
    }

    private static IQueryable<SqlServerJsonObjectIntegrationTests.TestJsonRecord> CreateConstantPathQuery(
        SqlServerJsonObjectIntegrationTests.TestJsonDbContext context,
        string path)
    {
        ParameterExpression record = Expression.Parameter(
            typeof(SqlServerJsonObjectIntegrationTests.TestJsonRecord),
            "record");
        MemberExpression values = Expression.Property(
            record,
            nameof(SqlServerJsonObjectIntegrationTests.TestJsonRecord.Values));
        MethodCallExpression value = Expression.Call(
            DynamicJsonScalarMethods.Value,
            values,
            Expression.Constant(path));
        BinaryExpression predicate = Expression.Equal(value, Expression.Constant("expected"));
        Expression<Func<SqlServerJsonObjectIntegrationTests.TestJsonRecord, bool>> lambda =
            Expression.Lambda<Func<SqlServerJsonObjectIntegrationTests.TestJsonRecord, bool>>(predicate, record);

        return context.Records.Where(lambda);
    }

    private static void AssertCapturedParameters(string sql, string comparisonOperator)
    {
        sql.Should().MatchRegex(@"JSON_VALUE\([^,]+, @[A-Za-z0-9_]+\)");
        sql.Should().MatchRegex(
            $@"{System.Text.RegularExpressions.Regex.Escape(comparisonOperator)} @[A-Za-z0-9_]+");
    }

    private static SqlServerJsonObjectIntegrationTests.TestJsonDbContext CreateContext(
        bool registerDynamicJson = true)
    {
        DbContextOptionsBuilder<SqlServerJsonObjectIntegrationTests.TestJsonDbContext> builder =
            new DbContextOptionsBuilder<SqlServerJsonObjectIntegrationTests.TestJsonDbContext>()
                .UseSqlServer("Server=(local);Database=DynamicJsonEfCoreSqlGeneration;Trusted_Connection=True;TrustServerCertificate=True");

        if (registerDynamicJson)
        {
            builder.UseDynamicJsonSqlServer();
        }

        return new SqlServerJsonObjectIntegrationTests.TestJsonDbContext(builder.Options);
    }
}
