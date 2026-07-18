using Dynamic.Json.EfCore.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.PostgreSql;

public sealed class PostgreSqlRegistrationTests
{
    [Fact]
    public void UseDynamicJsonPostgreSql_AddsNonProviderOptionsExtension()
    {
        DbContextOptionsBuilder builder = new();

        DbContextOptionsBuilder result = builder.UseDynamicJsonPostgreSql();

        result.Should().BeSameAs(builder);
        IDbContextOptionsExtension extension = GetDynamicJsonExtension(builder.Options);
        extension.Info.IsDatabaseProvider.Should().BeFalse();
        extension.Info.LogFragment.Should().Contain("DynamicJsonPostgreSql");
    }

    [Fact]
    public void UseDynamicJsonPostgreSql_CalledTwice_KeepsSingleOptionsExtension()
    {
        DbContextOptionsBuilder builder = new();

        builder.UseDynamicJsonPostgreSql();
        builder.UseDynamicJsonPostgreSql();

        builder.Options.Extensions
            .Where(IsDynamicJsonExtension)
            .Should().ContainSingle();
    }

    [Fact]
    public void UseDynamicJsonPostgreSql_ComposesWithNpgsqlConfiguration()
    {
        DbContextOptionsBuilder builder = new();

        builder
            .UseNpgsql("Host=localhost;Database=DynamicJsonEfCoreConfiguration")
            .UseDynamicJsonPostgreSql();

        builder.Options.Extensions.Should().Contain(extension =>
            extension.Info.IsDatabaseProvider &&
            extension.GetType().Assembly.GetName().Name == "Npgsql.EntityFrameworkCore.PostgreSQL");
        GetDynamicJsonExtension(builder.Options).Should().NotBeNull();
    }

    private static IDbContextOptionsExtension GetDynamicJsonExtension(DbContextOptions options)
        => options.Extensions.Single(IsDynamicJsonExtension);

    private static bool IsDynamicJsonExtension(IDbContextOptionsExtension extension)
        => extension.GetType().Assembly == typeof(DynamicJsonPostgreSqlDbContextOptionsBuilderExtensions).Assembly &&
           extension.GetType().Name == "DynamicJsonPostgreSqlOptionsExtension";
}
