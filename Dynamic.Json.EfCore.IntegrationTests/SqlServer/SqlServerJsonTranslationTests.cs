using Dynamic.Json.EfCore.Querying;
using Dynamic.Json.EfCore.SqlServer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.SqlServer;

public sealed class SqlServerJsonTranslationTests
{
    [Fact]
    public void Queries_GenerateExpectedSqlServerJsonTranslation()
    {
        DbContextOptionsBuilder<SqlServerJsonObjectIntegrationTests.TestJsonDbContext> builder =
            new DbContextOptionsBuilder<SqlServerJsonObjectIntegrationTests.TestJsonDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DynamicJsonEfCoreSqlGeneration");

        builder.UseDynamicJsonSqlServer();

        using SqlServerJsonObjectIntegrationTests.TestJsonDbContext context = new(builder.Options);

        string sql = context.Records
            .Where(r =>
                DynamicJsonFunctions.Value(r.Values, "$.department") == "HeelerHouse" &&
                DynamicJsonFunctions.ValueDecimal(r.Values, "$.score") >= 90m &&
                DynamicJsonFunctions.ValueDate(r.Values, "$.startDate") >= new DateOnly(2024, 1, 1))
            .ToQueryString();

        sql.Should().Contain("JSON_VALUE");
        sql.Should().Contain("TRY_CONVERT(decimal(18, 4), JSON_VALUE");
        sql.Should().Contain("TRY_CONVERT(date, JSON_VALUE");
    }
}
