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
                ["color"] = "blue",
                ["age"] = 7,
                ["birthday"] = "2018-10-01",
                ["isHeeler"] = true,
                ["metadata"] = new JsonObject
                {
                    ["breed"] = "heeler",
                    ["visits"] = 3
                }
            }
        });

        await context.SaveChangesAsync();

        await using TestJsonDbContext reloadContext = CreateContext(context.Database.GetConnectionString()!);
        TestJsonRecord record = await reloadContext.Records.SingleAsync(r => r.Id == id);

        record.Values["name"]!.GetValue<string>().Should().Be("Bluey");
        record.Values["color"]!.GetValue<string>().Should().Be("blue");
        record.Values["age"]!.GetValue<int>().Should().Be(7);
        record.Values["birthday"]!.GetValue<string>().Should().Be("2018-10-01");
        record.Values["isHeeler"]!.GetValue<bool>().Should().BeTrue();
        record.Values["metadata"]!["breed"]!.GetValue<string>().Should().Be("heeler");
        record.Values["metadata"]!["visits"]!.GetValue<int>().Should().Be(3);
    }

    [Fact]
    public async Task Value_StringQuery_FiltersAgainstRealSqlServer()
    {
        await using TestJsonDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);

        List<string> names = await context.Records
            .Where(r => DynamicJsonFunctions.Value(r.Values, "$.color") == "orange")
            .Select(r => DynamicJsonFunctions.Value(r.Values, "$.name")!)
            .OrderBy(name => name)
            .ToListAsync();

        names.Should().Equal("Bingo", "Chilli");
    }

    [Fact]
    public async Task ValueDecimal_NumericQuery_FiltersAgainstRealSqlServer()
    {
        await using TestJsonDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);

        List<string> names = await context.Records
            .Where(r => DynamicJsonFunctions.ValueDecimal(r.Values, "$.age") >= 7m)
            .Select(r => DynamicJsonFunctions.Value(r.Values, "$.name")!)
            .OrderBy(name => name)
            .ToListAsync();

        names.Should().Equal("Bluey", "Chilli");
    }

    [Fact]
    public async Task ValueDate_DateQuery_FiltersAgainstRealSqlServer()
    {
        await using TestJsonDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);

        List<string> names = await context.Records
            .Where(r => DynamicJsonFunctions.ValueDate(r.Values, "$.birthday") >= new DateOnly(2018, 1, 1))
            .Select(r => DynamicJsonFunctions.Value(r.Values, "$.name")!)
            .OrderBy(name => name)
            .ToListAsync();

        names.Should().Equal("Bingo", "Bluey");
    }

    [Fact]
    public async Task ScalarValues_NullAndFailedConversions_FollowPortableContract()
    {
        await using TestJsonDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        Guid validId = Guid.NewGuid();
        Guid missingId = Guid.NewGuid();
        Guid jsonNullId = Guid.NewGuid();
        Guid invalidId = Guid.NewGuid();
        Guid databaseNullId = Guid.NewGuid();

        context.Records.AddRange(
            new TestJsonRecord
            {
                Id = validId,
                Values = new JsonObject
                {
                    ["text"] = "present",
                    ["number"] = "12.5",
                    ["date"] = "2026-07-17"
                }
            },
            new TestJsonRecord { Id = missingId, Values = new JsonObject() },
            new TestJsonRecord
            {
                Id = jsonNullId,
                Values = new JsonObject
                {
                    ["text"] = null,
                    ["number"] = null,
                    ["date"] = null
                }
            },
            new TestJsonRecord
            {
                Id = invalidId,
                Values = new JsonObject
                {
                    ["number"] = "not-a-decimal",
                    ["date"] = "not-a-date"
                }
            });

        await context.SaveChangesAsync();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [Records] ([Id], [Values]) VALUES ({databaseNullId}, NULL)");

        ScalarValues[] values = await context.Records
            .OrderBy(record => record.Id)
            .Select(record => new ScalarValues(
                record.Id,
                DynamicJsonFunctions.Value(record.Values, "$.text"),
                DynamicJsonFunctions.ValueDecimal(record.Values, "$.number"),
                DynamicJsonFunctions.ValueDate(record.Values, "$.date")))
            .ToArrayAsync();

        values.Single(value => value.Id == validId).Should().Be(
            new ScalarValues(validId, "present", 12.5m, new DateOnly(2026, 7, 17)));
        values.Single(value => value.Id == missingId).Should().Be(
            new ScalarValues(missingId, null, null, null));
        values.Single(value => value.Id == jsonNullId).Should().Be(
            new ScalarValues(jsonNullId, null, null, null));
        values.Single(value => value.Id == invalidId).Should().Be(
            new ScalarValues(invalidId, null, null, null));
        values.Single(value => value.Id == databaseNullId).Should().Be(
            new ScalarValues(databaseNullId, null, null, null));
    }

    [Fact]
    public async Task Value_EscapedAndNestedPropertyPaths_FilterAgainstRealSqlServer()
    {
        await using TestJsonDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        context.Records.Add(new TestJsonRecord
        {
            Id = Guid.NewGuid(),
            Values = new JsonObject
            {
                ["stage.name"] = "Rumi",
                ["huntrix"] = new JsonObject
                {
                    ["demon-rank"] = "golden"
                }
            }
        });
        await context.SaveChangesAsync();

        string escapedPropertyPath = DynamicJsonPath.FromProperty("stage.name");
        string nestedPropertyPath = DynamicJsonPath.FromProperties("huntrix", "demon-rank");

        TestJsonRecord[] matches = await context.Records
            .Where(record =>
                DynamicJsonFunctions.Value(record.Values, escapedPropertyPath) == "Rumi" &&
                DynamicJsonFunctions.Value(record.Values, nestedPropertyPath) == "golden")
            .ToArrayAsync();

        matches.Should().ContainSingle();
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
            CreateRecord("Bluey", "blue", 7, "2018-10-01"),
            CreateRecord("Bingo", "orange", 5, "2020-05-12"),
            CreateRecord("Chilli", "orange", 35, "1988-03-18"));

        await context.SaveChangesAsync();
    }

    private static TestJsonRecord CreateRecord(string name, string color, int age, string birthday)
        => new()
        {
            Id = Guid.NewGuid(),
            Values = new JsonObject
            {
                ["name"] = name,
                ["color"] = color,
                ["age"] = age,
                ["birthday"] = birthday
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
                    .HasJsonConversion()
                    .IsRequired(false);
            });
        }
    }

    internal sealed class TestJsonRecord
    {
        public Guid Id { get; set; }

        public JsonObject Values { get; set; } = new();
    }

    private sealed record ScalarValues(Guid Id, string? Text, decimal? Number, DateOnly? Date);
}
