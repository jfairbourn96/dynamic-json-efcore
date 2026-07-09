using Dynamic.Json.EfCore.Querying;
using Dynamic.Json.EfCore.SqlServer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.SqlServer;

[Collection(SqlServerContainerCollection.Name)]
public sealed class SqlServerJsonTranslationTests
{
    private readonly SqlServerContainerFixture _fixture;

    public SqlServerJsonTranslationTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Queries_GenerateExpectedSqlServerJsonTranslation()
    {
        DbContextOptionsBuilder<SqlServerJsonObjectIntegrationTests.TestJsonDbContext> builder =
            new DbContextOptionsBuilder<SqlServerJsonObjectIntegrationTests.TestJsonDbContext>()
                .UseSqlServer(_fixture.ConnectionString);

        builder.UseDynamicJsonSqlServer();

        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = new(builder.Options);

        string sql = context.Records
            .Where(r =>
                DynamicJsonFunctions.Value(r.Values, "$.color") == "orange" &&
                DynamicJsonFunctions.ValueDecimal(r.Values, "$.age") >= 7m &&
                DynamicJsonFunctions.ValueDate(r.Values, "$.birthday") >= new DateOnly(2018, 1, 1))
            .ToQueryString();

        sql.Should().Contain("JSON_VALUE");
        sql.Should().Contain("TRY_CONVERT(decimal(18, 4), JSON_VALUE");
        sql.Should().Contain("TRY_CONVERT(date, JSON_VALUE");
    }
}
