using System.Data.Common;
using Dynamic.Json.EfCore.IntegrationTests.ProviderContracts;
using Dynamic.Json.EfCore.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.PostgreSql;

[Collection(PostgreSqlContainerCollection.Name)]
public sealed class PostgreSqlScalarProviderContractTests : ScalarProviderContractTests
{
    private readonly IScalarProviderFixture _fixture;

    public PostgreSqlScalarProviderContractTests(PostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;
    }

    protected override string ExpectedStoreType => "jsonb";

    protected override ContractDbContext CreateContext(string? connectionString = null)
    {
        DbContextOptionsBuilder<ContractDbContext> builder =
            new DbContextOptionsBuilder<ContractDbContext>()
                .UseNpgsql(connectionString ?? CreateDatabaseConnectionString());
        builder.UseDynamicJsonPostgreSql();

        return new ContractDbContext(builder.Options, "jsonb");
    }

    protected override async Task<string> ReadStoreTypeAsync(ContractDbContext context)
    {
        await context.Database.OpenConnectionAsync();
        await using DbCommand command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT udt_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'ScalarProviderContractRecords'
              AND column_name = 'Values'
            """;

        return (string)(await command.ExecuteScalarAsync())!;
    }

    protected override async Task<object?> ReadStoredValueAsync(ContractDbContext context, Guid id)
    {
        await context.Database.OpenConnectionAsync();
        await using DbCommand command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText =
            """SELECT "Values"::text FROM "ScalarProviderContractRecords" WHERE "Id" = @id""";
        command.Parameters.Add(new NpgsqlParameter<Guid>("id", id));

        return await command.ExecuteScalarAsync();
    }

    private string CreateDatabaseConnectionString()
    {
        NpgsqlConnectionStringBuilder builder = new(_fixture.ConnectionString)
        {
            Database = $"dynamic_json_efcore_contracts_{Guid.NewGuid():N}"
        };

        return builder.ConnectionString;
    }
}
