using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.ChangeTracking;
using Dynamic.Json.EfCore.Metadata;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dynamic.Json.EfCore.Tests.Base.Extensions;

public class JsonObjectPropertyBuilderExtensionsTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public JsonObjectPropertyBuilderExtensionsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using TestDbContext ctx = CreateContext();
        ctx.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task HasJsonConversion_RoundTrip_PreservesAllValues()
    {
        Guid id = Guid.NewGuid();

        await using (TestDbContext write = CreateContext())
        {
            write.Records.Add(new TestRecord
            {
                Id = id,
                Values = new JsonObject { ["name"] = "Alice", ["score"] = 42 }
            });
            await write.SaveChangesAsync();
        }

        await using TestDbContext read = CreateContext();
        TestRecord? record = await read.Records.FindAsync(id);

        record.Should().NotBeNull();
        record!.Values["name"]!.GetValue<string>().Should().Be("Alice");
        record.Values["score"]!.GetValue<int>().Should().Be(42);
    }

    [Fact]
    public async Task HasJsonConversion_RoundTrip_EmptyObjectRoundTrips()
    {
        Guid id = Guid.NewGuid();

        await using (TestDbContext write = CreateContext())
        {
            write.Records.Add(new TestRecord { Id = id, Values = new JsonObject() });
            await write.SaveChangesAsync();
        }

        await using TestDbContext read = CreateContext();
        TestRecord? record = await read.Records.FindAsync(id);

        record.Should().NotBeNull();
        record!.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task HasJsonConversion_MutationDetected_ChangeIsPersisted()
    {
        Guid id = Guid.NewGuid();

        await using TestDbContext ctx = CreateContext();
        ctx.Records.Add(new TestRecord
        {
            Id = id,
            Values = new JsonObject { ["count"] = 1 }
        });
        await ctx.SaveChangesAsync();

        TestRecord? record = await ctx.Records.FindAsync(id);
        record!.Values["count"] = 99;
        await ctx.SaveChangesAsync();

        await using TestDbContext verify = CreateContext();
        TestRecord? updated = await verify.Records.FindAsync(id);
        updated!.Values["count"]!.GetValue<int>().Should().Be(99);
    }

    [Fact]
    public async Task HasJsonConversion_AddNewField_ChangeIsPersisted()
    {
        Guid id = Guid.NewGuid();

        await using TestDbContext ctx = CreateContext();
        ctx.Records.Add(new TestRecord
        {
            Id = id,
            Values = new JsonObject { ["existing"] = "yes" }
        });
        await ctx.SaveChangesAsync();

        TestRecord? record = await ctx.Records.FindAsync(id);
        record!.Values["newField"] = "added";
        await ctx.SaveChangesAsync();

        await using TestDbContext verify = CreateContext();
        TestRecord? updated = await verify.Records.FindAsync(id);
        updated!.Values["newField"]!.GetValue<string>().Should().Be("added");
        updated.Values["existing"]!.GetValue<string>().Should().Be("yes");
    }

    [Fact]
    public void HasJsonConversion_DefaultMode_UsesSemanticComparer()
    {
        using TestDbContext ctx = CreateContext();

        ctx.Model.FindEntityType(typeof(TestRecord))!
            .FindProperty(nameof(TestRecord.Values))!
            .GetValueComparer()
            .Should().BeOfType<JsonObjectValueComparer>();
    }

    [Fact]
    public void HasJsonConversion_SerializedMode_UsesSerializedComparer()
    {
        DbContextOptions options = new DbContextOptionsBuilder<SerializedComparisonDbContext>()
            .UseSqlite(_connection)
            .Options;
        using SerializedComparisonDbContext ctx = new(options);

        ctx.Model.FindEntityType(typeof(SerializedComparisonRecord))!
            .FindProperty(nameof(SerializedComparisonRecord.Values))!
            .GetValueComparer()
            .Should().BeOfType<SerializedJsonObjectValueComparer>();
    }

    private TestDbContext CreateContext()
    {
        DbContextOptions options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new TestDbContext(options);
    }

    private class TestRecord
    {
        public Guid Id { get; set; }
        public JsonObject Values { get; set; } = new();
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestRecord> Records => Set<TestRecord>();

        public TestDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestRecord>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Values).HasJsonConversion();
            });
        }
    }

    private class SerializedComparisonRecord
    {
        public Guid Id { get; set; }
        public JsonObject Values { get; set; } = new();
    }

    private class SerializedComparisonDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<SerializedComparisonRecord> Records => Set<SerializedComparisonRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SerializedComparisonRecord>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Values).HasJsonConversion(JsonObjectComparisonMode.Serialized);
            });
        }
    }
}
