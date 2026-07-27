using System.Data.Common;
using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.Metadata;
using Dynamic.Json.EfCore.Querying;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.ProviderContracts;

/// <summary>
/// Reusable real-provider contracts for JSON persistence and scalar query behavior.
/// </summary>
public abstract class ScalarProviderContractTests
{
    [Fact]
    public async Task JsonObjectPersistence_RoundTripsPopulatedEmptyAndDatabaseNull()
    {
        Guid populatedId = Guid.NewGuid();
        Guid emptyId = Guid.NewGuid();
        Guid nullId = Guid.NewGuid();
        Guid[] testIds = [populatedId, emptyId, nullId];
        JsonObject populatedValues = new()
        {
            ["text"] = "present",
            ["number"] = 12.5m,
            ["date"] = "2026-07-27",
            ["nested"] = new JsonObject { ["enabled"] = true }
        };

        string connectionString;
        await using (ContractDbContext context = CreateContext())
        {
            connectionString = context.Database.GetConnectionString()!;
            await context.Database.EnsureCreatedAsync();
            context.Records.AddRange(
                new ContractRecord { Id = populatedId, Values = populatedValues },
                new ContractRecord { Id = emptyId, Values = new JsonObject() },
                new ContractRecord { Id = nullId, Values = null! });
            await context.SaveChangesAsync();

            (await ReadStoreTypeAsync(context)).Should().Be(ExpectedStoreType);
            string populatedJson = (string)(await ReadStoredValueAsync(context, populatedId))!;
            JsonNode.DeepEquals(JsonNode.Parse(populatedJson), populatedValues).Should().BeTrue();
            (await ReadStoredValueAsync(context, emptyId)).Should().Be("{}");
            (await ReadStoredValueAsync(context, nullId)).Should().Be(DBNull.Value);
        }

        await using ContractDbContext reloadContext = CreateContext(connectionString);
        ContractRecord[] records = await reloadContext.Records
            .AsNoTracking()
            .Where(record => testIds.Contains(record.Id))
            .ToArrayAsync();

        records.Should().HaveCount(3);
        JsonNode.DeepEquals(
            records.Single(record => record.Id == populatedId).Values,
            populatedValues).Should().BeTrue();
        records.Single(record => record.Id == emptyId).Values.Should().BeEmpty();
        records.Single(record => record.Id == nullId).Values.Should().BeNull();
    }

    [Fact]
    public async Task ScalarQueries_ReturnPortableResultsForValidMissingNullAndInvalidValues()
    {
        Guid validId = Guid.NewGuid();
        Guid missingId = Guid.NewGuid();
        Guid jsonNullId = Guid.NewGuid();
        Guid databaseNullId = Guid.NewGuid();
        Guid invalidId = Guid.NewGuid();
        Guid[] testIds = [validId, missingId, jsonNullId, databaseNullId, invalidId];

        string connectionString;
        await using (ContractDbContext context = CreateContext())
        {
            connectionString = context.Database.GetConnectionString()!;
            await context.Database.EnsureCreatedAsync();
            context.Records.AddRange(
                new ContractRecord
                {
                    Id = validId,
                    Values = new JsonObject
                    {
                        ["text"] = "present",
                        ["number"] = "12.5",
                        ["date"] = "2026-07-27"
                    }
                },
                new ContractRecord { Id = missingId, Values = new JsonObject() },
                new ContractRecord
                {
                    Id = jsonNullId,
                    Values = new JsonObject
                    {
                        ["text"] = null,
                        ["number"] = null,
                        ["date"] = null
                    }
                },
                new ContractRecord { Id = databaseNullId, Values = null! },
                new ContractRecord
                {
                    Id = invalidId,
                    Values = new JsonObject
                    {
                        ["number"] = "not-a-decimal",
                        ["date"] = "not-a-date"
                    }
                });
            await context.SaveChangesAsync();
        }

        await using ContractDbContext queryContext = CreateContext(connectionString);
        ScalarValues[] values = await queryContext.Records
            .Where(record => testIds.Contains(record.Id))
            .Select(record => new ScalarValues(
                record.Id,
                DynamicJsonFunctions.Value(record.Values, "$.text"),
                DynamicJsonFunctions.ValueDecimal(record.Values, "$.number"),
                DynamicJsonFunctions.ValueDate(record.Values, "$.date")))
            .ToArrayAsync();

        values.Single(value => value.Id == validId).Should().Be(
            new ScalarValues(validId, "present", 12.5m, new DateOnly(2026, 7, 27)));
        values.Single(value => value.Id == missingId).Should().Be(
            new ScalarValues(missingId, null, null, null));
        values.Single(value => value.Id == jsonNullId).Should().Be(
            new ScalarValues(jsonNullId, null, null, null));
        values.Single(value => value.Id == databaseNullId).Should().Be(
            new ScalarValues(databaseNullId, null, null, null));
        values.Single(value => value.Id == invalidId).Should().Be(
            new ScalarValues(invalidId, null, null, null));
    }

    protected abstract string ExpectedStoreType { get; }

    protected abstract ContractDbContext CreateContext(string? connectionString = null);

    protected abstract Task<string> ReadStoreTypeAsync(ContractDbContext context);

    protected abstract Task<object?> ReadStoredValueAsync(ContractDbContext context, Guid id);

    protected sealed class ContractDbContext : DbContext
    {
        private readonly string _jsonColumnType;

        public ContractDbContext(DbContextOptions<ContractDbContext> options, string jsonColumnType)
            : base(options)
        {
            _jsonColumnType = jsonColumnType;
        }

        public DbSet<ContractRecord> Records => Set<ContractRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<ContractRecord> entity = modelBuilder.Entity<ContractRecord>();
            entity.ToTable("ScalarProviderContractRecords");
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Values)
                .HasColumnType(_jsonColumnType)
                .HasJsonConversion()
                .IsRequired(false);
        }
    }

    protected sealed class ContractRecord
    {
        public Guid Id { get; set; }

        public JsonObject Values { get; set; } = new();
    }

    private sealed record ScalarValues(Guid Id, string? Text, decimal? Number, DateOnly? Date);
}
