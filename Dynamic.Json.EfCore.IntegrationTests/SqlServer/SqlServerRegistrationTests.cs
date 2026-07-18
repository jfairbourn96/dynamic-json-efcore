using Dynamic.Json.EfCore.SqlServer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Dynamic.Json.EfCore.IntegrationTests.SqlServer;

public sealed class SqlServerRegistrationTests
{
    [Fact]
    public void UseDynamicJsonSqlServer_AddsNonProviderOptionsExtension()
    {
        DbContextOptionsBuilder builder = new();

        DbContextOptionsBuilder result = builder.UseDynamicJsonSqlServer();

        result.Should().BeSameAs(builder);
        IDbContextOptionsExtension extension = GetDynamicJsonExtension(builder.Options);
        extension.Info.IsDatabaseProvider.Should().BeFalse();
        extension.Info.LogFragment.Should().Contain("DynamicJsonSqlServer");
    }

    [Fact]
    public void UseDynamicJsonSqlServer_CalledTwice_KeepsSingleOptionsExtension()
    {
        DbContextOptionsBuilder builder = new();

        builder.UseDynamicJsonSqlServer();
        builder.UseDynamicJsonSqlServer();

        builder.Options.Extensions
            .Where(IsDynamicJsonExtension)
            .Should().ContainSingle();
    }

    private static IDbContextOptionsExtension GetDynamicJsonExtension(DbContextOptions options)
        => options.Extensions.Single(IsDynamicJsonExtension);

    private static bool IsDynamicJsonExtension(IDbContextOptionsExtension extension)
        => extension.GetType().Assembly == typeof(DynamicJsonSqlServerDbContextOptionsBuilderExtensions).Assembly &&
           extension.GetType().Name == "DynamicJsonSqlServerOptionsExtension";
}
