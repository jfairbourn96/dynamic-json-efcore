using Testcontainers.PostgreSql;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.PostgreSql;

public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync()
        => _container.StartAsync();

    public Task DisposeAsync()
        => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public sealed class PostgreSqlContainerCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
    public const string Name = "PostgreSqlContainer";
}
