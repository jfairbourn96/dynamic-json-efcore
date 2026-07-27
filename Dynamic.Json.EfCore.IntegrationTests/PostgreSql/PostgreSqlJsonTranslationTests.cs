using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.Metadata;
using Dynamic.Json.EfCore.PostgreSql;
using Dynamic.Json.EfCore.Querying;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.PostgreSql;

public sealed class PostgreSqlJsonTranslationTests
{
    [Fact]
    public void Value_GeneratesJsonPathTextExtractionSql()
    {
        using TestJsonDbContext context = CreateContext();

        string sql = context.Records
            .Where(record => DynamicJsonFunctions.Value(record.Values, "$.color") == "orange")
            .ToQueryString();

        sql.Should().Contain("jsonb_path_query_first(r.\"Values\", '$.color'::jsonpath) #>> '{}'");
    }

    [Fact]
    public void ValueDecimal_GeneratesGuardedNumericConversionSql()
    {
        using TestJsonDbContext context = CreateContext();

        string sql = context.Records
            .Where(record => DynamicJsonFunctions.ValueDecimal(record.Values, "$.age") >= 7m)
            .ToQueryString();

        sql.Should().Contain("pg_input_is_valid");
        sql.Should().Contain("AS numeric");
    }

    [Fact]
    public void ValueDate_GeneratesGuardedDateConversionSql()
    {
        using TestJsonDbContext context = CreateContext();

        string sql = context.Records
            .Where(record => DynamicJsonFunctions.ValueDate(record.Values, "$.birthday") >= new DateOnly(2018, 1, 1))
            .ToQueryString();

        sql.Should().Contain("pg_input_is_valid");
        sql.Should().Contain("AS date");
    }

    [Theory]
    [InlineData("$.huntrix.leader")]
    [InlineData("$.\"stage.name\"")]
    [InlineData("$.\"demon\\\"hunter\"")]
    public void Value_PortablePropertyPath_PreservesCanonicalPathInSql(string path)
    {
        using TestJsonDbContext context = CreateContext();

        string sql = CreateConstantPathQuery(context, path).ToQueryString();

        sql.Should().Contain(path);
    }

    [Fact]
    public void Value_PathCreatedFromRuntimePropertyName_UsesJsonPathParameter()
    {
        using TestJsonDbContext context = CreateContext();
        string path = DynamicJsonPath.FromProperty("stage.name");

        string sql = context.Records
            .Where(record => DynamicJsonFunctions.Value(record.Values, path) == "expected")
            .ToQueryString();

        sql.Should().Contain("$.\"stage.name\"");
        sql.Should().MatchRegex(@"jsonb_path_query_first\([^,]+, @[A-Za-z0-9_]+::jsonpath\)");
    }

    [Fact]
    public void ScalarQueries_CapturedPathsAndComparisonValues_RemainParameterized()
    {
        using TestJsonDbContext context = CreateContext();
        string textPath = DynamicJsonPath.FromProperty("stage.name");
        string expectedText = "Rumi";
        string decimalPath = DynamicJsonPath.FromProperty("score");
        decimal minimumDecimal = 7.5m;
        string datePath = DynamicJsonPath.FromProperty("debutDate");
        DateOnly minimumDate = new(2024, 6, 14);

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
        using TestJsonDbContext context = CreateContext();

        Action act = () => CreateConstantPathQuery(context, path).ToQueryString();

        act.Should().Throw<DynamicJsonPathException>();
    }

    [Fact]
    public void ScalarQuery_WithoutDynamicJsonRegistration_FailsTranslation()
    {
        using TestJsonDbContext context = CreateContext(registerDynamicJson: false);

        Action act = () => context.Records
            .Where(record => DynamicJsonFunctions.Value(record.Values, "$.color") == "orange")
            .ToQueryString();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*could not be translated*");
    }

    private static IQueryable<TestJsonRecord> CreateConstantPathQuery(
        TestJsonDbContext context,
        string path)
    {
        ParameterExpression record = Expression.Parameter(typeof(TestJsonRecord), "record");
        MemberExpression values = Expression.Property(record, nameof(TestJsonRecord.Values));
        MethodCallExpression value = Expression.Call(
            DynamicJsonScalarMethods.Value,
            values,
            Expression.Constant(path));
        BinaryExpression predicate = Expression.Equal(value, Expression.Constant("expected"));
        Expression<Func<TestJsonRecord, bool>> lambda =
            Expression.Lambda<Func<TestJsonRecord, bool>>(predicate, record);

        return context.Records.Where(lambda);
    }

    private static void AssertCapturedParameters(string sql, string comparisonOperator)
    {
        sql.Should().MatchRegex(
            @"jsonb_path_query_first\([^,]+, @[A-Za-z0-9_]+::jsonpath\)");
        sql.Should().MatchRegex(
            $@"{System.Text.RegularExpressions.Regex.Escape(comparisonOperator)} @[A-Za-z0-9_]+");
    }

    private static TestJsonDbContext CreateContext(bool registerDynamicJson = true)
    {
        DbContextOptionsBuilder<TestJsonDbContext> builder = new DbContextOptionsBuilder<TestJsonDbContext>()
            .UseNpgsql("Host=localhost;Database=DynamicJsonEfCoreSqlGeneration");
        if (registerDynamicJson)
        {
            builder.UseDynamicJsonPostgreSql();
        }

        return new TestJsonDbContext(builder.Options);
    }

    private sealed class TestJsonDbContext : DbContext
    {
        public TestJsonDbContext(DbContextOptions<TestJsonDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestJsonRecord> Records => Set<TestJsonRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TestJsonRecord>().Property(record => record.Values)
                .HasColumnType("jsonb")
                .HasJsonConversion();
    }

    private sealed class TestJsonRecord
    {
        public Guid Id { get; set; }

        public JsonObject Values { get; set; } = new();
    }
}
