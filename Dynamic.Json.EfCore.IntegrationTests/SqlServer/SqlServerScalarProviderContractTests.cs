using System.Data.Common;
using Dynamic.Json.EfCore.IntegrationTests.ProviderContracts;
using Dynamic.Json.EfCore.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.SqlServer;

[Collection(SqlServerContainerCollection.Name)]
public sealed class SqlServerScalarProviderContractTests : ScalarProviderContractTests
{
    private readonly IScalarProviderFixture _fixture;

    public SqlServerScalarProviderContractTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
    }

    protected override string ExpectedStoreType => "nvarchar";

    protected override ContractDbContext CreateContext(string? connectionString = null)
    {
        DbContextOptionsBuilder<ContractDbContext> builder =
            new DbContextOptionsBuilder<ContractDbContext>()
                .UseSqlServer(connectionString ?? CreateDatabaseConnectionString());
        builder.UseDynamicJsonSqlServer();

        return new ContractDbContext(builder.Options, "nvarchar(max)");
    }

    protected override async Task<string> ReadStoreTypeAsync(ContractDbContext context)
    {
        await context.Database.OpenConnectionAsync();
        await using DbCommand command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT DATA_TYPE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = 'dbo'
              AND TABLE_NAME = 'ScalarProviderContractRecords'
              AND COLUMN_NAME = 'Values'
            """;

        return (string)(await command.ExecuteScalarAsync())!;
    }

    protected override async Task<object?> ReadStoredValueAsync(ContractDbContext context, Guid id)
    {
        await context.Database.OpenConnectionAsync();
        await using DbCommand command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText =
            "SELECT [Values] FROM [ScalarProviderContractRecords] WHERE [Id] = @id";
        command.Parameters.Add(new SqlParameter("id", id));

        return await command.ExecuteScalarAsync();
    }

    private string CreateDatabaseConnectionString()
    {
        SqlConnectionStringBuilder builder = new(_fixture.ConnectionString)
        {
            InitialCatalog = $"DynamicJsonEfCoreContracts_{Guid.NewGuid():N}",
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }
}
