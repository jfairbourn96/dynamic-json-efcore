using System.Data.Common;
using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.Metadata;
using Dynamic.Json.EfCore.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.PostgreSql;

[Collection(PostgreSqlContainerCollection.Name)]
public sealed class PostgreSqlJsonObjectIntegrationTests
{
    private readonly PostgreSqlContainerFixture _fixture;

    public PostgreSqlJsonObjectIntegrationTests(PostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task JsonObjectValues_PersistAsJsonb_AndMaterializePopulatedEmptyAndNull()
    {
        Guid populatedId = Guid.NewGuid();
        Guid emptyId = Guid.NewGuid();
        Guid nullId = Guid.NewGuid();
        JsonObject populatedValues = new()
        {
            ["name"] = "Rumi",
            ["rank"] = 1,
            ["active"] = true,
            ["metadata"] = new JsonObject
            {
                ["group"] = "Huntrix"
            }
        };

        await using (TestJsonDbContext context = CreateContext())
        {
            await context.Database.EnsureCreatedAsync();
            context.Records.AddRange(
                new TestJsonRecord { Id = populatedId, Values = populatedValues },
                new TestJsonRecord { Id = emptyId, Values = new JsonObject() },
                new TestJsonRecord { Id = nullId, Values = null! });

            await context.SaveChangesAsync();

            (string dataType, string udtName) = await ReadColumnTypeAsync(context);
            dataType.Should().Be("jsonb");
            udtName.Should().Be("jsonb");

            string populatedJson = (string)(await ReadStoredValueAsync(context, populatedId))!;
            JsonNode.DeepEquals(JsonNode.Parse(populatedJson), populatedValues).Should().BeTrue();
            (await ReadStoredValueAsync(context, emptyId)).Should().Be("{}");
            (await ReadStoredValueAsync(context, nullId)).Should().Be(DBNull.Value);
        }

        await using TestJsonDbContext reloadContext = CreateContext();
        TestJsonRecord[] records = await reloadContext.Records
            .AsNoTracking()
            .ToArrayAsync();

        TestJsonRecord populated = records.Single(record => record.Id == populatedId);
        populated.Values["name"]!.GetValue<string>().Should().Be("Rumi");
        populated.Values["rank"]!.GetValue<int>().Should().Be(1);
        populated.Values["active"]!.GetValue<bool>().Should().BeTrue();
        populated.Values["metadata"]!["group"]!.GetValue<string>().Should().Be("Huntrix");
        records.Single(record => record.Id == emptyId).Values.Should().BeEmpty();
        records.Single(record => record.Id == nullId).Values.Should().BeNull();
    }

    private TestJsonDbContext CreateContext()
    {
        DbContextOptionsBuilder<TestJsonDbContext> builder =
            new DbContextOptionsBuilder<TestJsonDbContext>()
                .UseNpgsql(_fixture.ConnectionString);
        builder.UseDynamicJsonPostgreSql();

        return new TestJsonDbContext(builder.Options);
    }

    private static async Task<(string DataType, string UdtName)> ReadColumnTypeAsync(
        TestJsonDbContext context)
    {
        await context.Database.OpenConnectionAsync();
        await using DbCommand command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT data_type, udt_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'Records'
              AND column_name = 'Values'
            """;
        await using DbDataReader reader = await command.ExecuteReaderAsync();
        (await reader.ReadAsync()).Should().BeTrue();

        return (reader.GetString(0), reader.GetString(1));
    }

    private static async Task<object?> ReadStoredValueAsync(TestJsonDbContext context, Guid id)
    {
        await context.Database.OpenConnectionAsync();
        await using DbCommand command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT \"Values\"::text FROM \"Records\" WHERE \"Id\" = @id";
        command.Parameters.Add(new NpgsqlParameter<Guid>("id", id));

        return await command.ExecuteScalarAsync();
    }

    private sealed class TestJsonDbContext : DbContext
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
                entity.HasKey(record => record.Id);
                entity.Property(record => record.Values)
                    .HasColumnType("jsonb")
                    .HasJsonConversion()
                    .IsRequired(false);
            });
        }
    }

    private sealed class TestJsonRecord
    {
        public Guid Id { get; set; }

        public JsonObject Values { get; set; } = new();
    }
}
