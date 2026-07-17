using Dynamic.Json.EfCore.Querying;
using Dynamic.Json.EfCore.SqlServer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

        sql.Should().Contain("JSON_VALUE");
    }

    [Fact]
    public void ValueDecimal_GeneratesTryConvertDecimalSql()
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();

        string sql = context.Records
            .Where(r => DynamicJsonFunctions.ValueDecimal(r.Values, "$.age") >= 7m)
            .ToQueryString();

        sql.Should().Contain("TRY_CONVERT(decimal(18, 4), JSON_VALUE");
    }

    [Fact]
    public void ValueDate_GeneratesTryConvertDateSql()
    {
        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = CreateContext();

        string sql = context.Records
            .Where(r => DynamicJsonFunctions.ValueDate(r.Values, "$.birthday") >= new DateOnly(2018, 1, 1))
            .ToQueryString();

        sql.Should().Contain("TRY_CONVERT(date, JSON_VALUE");
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
