using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.Metadata;
using Dynamic.Json.EfCore.Querying;
using Dynamic.Json.EfCore.SqlServer;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.SqlServer;

[Collection(SqlServerContainerCollection.Name)]
public sealed class SqlServerJsonObjectIntegrationTests
{
    private readonly SqlServerContainerFixture _fixture;

    public SqlServerJsonObjectIntegrationTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task JsonObjectValues_PersistAndReloadCorrectly()
    {
        await using TestJsonDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        Guid id = Guid.NewGuid();
        context.Records.Add(new TestJsonRecord
        {
            Id = id,
            Values = new JsonObject
            {
                ["name"] = "Bluey",
                ["score"] = 42.75m,
                ["active"] = true,
                ["metadata"] = new JsonObject
                {
                    ["tier"] = "heeler",
                    ["visits"] = 3
                }
            }
        });

        await context.SaveChangesAsync();

        await using TestJsonDbContext reloadContext = CreateContext(context.Database.GetConnectionString()!);
        TestJsonRecord record = await reloadContext.Records.SingleAsync(r => r.Id == id);

        record.Values["name"]!.GetValue<string>().Should().Be("Bluey");
        record.Values["score"]!.GetValue<decimal>().Should().Be(42.75m);
        record.Values["active"]!.GetValue<bool>().Should().BeTrue();
        record.Values["metadata"]!["tier"]!.GetValue<string>().Should().Be("heeler");
        record.Values["metadata"]!["visits"]!.GetValue<int>().Should().Be(3);
    }

    [Fact]
    public async Task Value_StringQuery_FiltersAgainstRealSqlServer()
    {
        await using TestJsonDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);

        List<string> names = await context.Records
            .Where(r => DynamicJsonFunctions.Value(r.Values, "$.department") == "Engineering")
            .Select(r => DynamicJsonFunctions.Value(r.Values, "$.name")!)
            .OrderBy(name => name)
            .ToListAsync();

        names.Should().Equal("Bluey", "Bingo");
    }

    [Fact]
    public async Task ValueDecimal_NumericQuery_FiltersAgainstRealSqlServer()
    {
        await using TestJsonDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);

        List<string> names = await context.Records
            .Where(r => DynamicJsonFunctions.ValueDecimal(r.Values, "$.score") >= 90m)
            .Select(r => DynamicJsonFunctions.Value(r.Values, "$.name")!)
            .OrderBy(name => name)
            .ToListAsync();

        names.Should().Equal("Bluey", "Bingo");
    }

    [Fact]
    public async Task ValueDate_DateQuery_FiltersAgainstRealSqlServer()
    {
        await using TestJsonDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);

        List<string> names = await context.Records
            .Where(r => DynamicJsonFunctions.ValueDate(r.Values, "$.startDate") >= new DateOnly(2024, 1, 1))
            .Select(r => DynamicJsonFunctions.Value(r.Values, "$.name")!)
            .OrderBy(name => name)
            .ToListAsync();

        names.Should().Equal("Bluey", "Chilli");
    }

    private TestJsonDbContext CreateContext()
        => CreateContext(CreateDatabaseConnectionString());

    private TestJsonDbContext CreateContext(string connectionString)
    {
        DbContextOptionsBuilder<TestJsonDbContext> builder = new DbContextOptionsBuilder<TestJsonDbContext>()
            .UseSqlServer(connectionString);

        builder.UseDynamicJsonSqlServer();

        return new TestJsonDbContext(builder.Options);
    }

    private string CreateDatabaseConnectionString()
    {
        SqlConnectionStringBuilder builder = new(_fixture.ConnectionString)
        {
            InitialCatalog = $"DynamicJsonEfCoreTests_{Guid.NewGuid():N}",
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }

    private static async Task SeedAsync(TestJsonDbContext context)
    {
        context.Records.AddRange(
            CreateRecord("Bluey", "HeelerHouse", 98.50m, "2024-01-15"),
            CreateRecord("Bingo", "HeelerHouse", 91m, "2023-12-01"),
            CreateRecord("Chilli", "GrownUps", 85.25m, "2024-06-30"));

        await context.SaveChangesAsync();
    }

    private static TestJsonRecord CreateRecord(string name, string department, decimal score, string startDate)
        => new()
        {
            Id = Guid.NewGuid(),
            Values = new JsonObject
            {
                ["name"] = name,
                ["department"] = department,
                ["score"] = score,
                ["startDate"] = startDate
            }
        };

    internal sealed class TestJsonDbContext : DbContext
    {
        public TestJsonDbContext(DbContextOptions<TestJsonDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestJsonRecord> Records => Set<TestJsonRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestJsonRecord>(entity =>
            {
                entity.ToTable("Records");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Values)
                    .HasColumnType("nvarchar(max)")
                    .HasJsonConversion();
            });
        }
    }

    internal sealed class TestJsonRecord
    {
        public Guid Id { get; set; }

        public JsonObject Values { get; set; } = new();
    }
}
