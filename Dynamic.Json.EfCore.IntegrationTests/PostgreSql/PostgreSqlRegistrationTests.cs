using Dynamic.Json.EfCore.PostgreSql;
using Dynamic.Json.EfCore.SqlServer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public void UseDynamicJsonPostgreSql_RegistersTranslatorPluginWithScopedLifetime()
    {
        DbContextOptionsBuilder builder = new();
        builder.UseDynamicJsonPostgreSql();
        ServiceCollection services = new();

        GetDynamicJsonExtension(builder.Options).ApplyServices(services);

        ServiceDescriptor descriptor = services.Should()
            .ContainSingle(service => service.ServiceType == typeof(IMethodCallTranslatorPlugin))
            .Subject;
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptor.ImplementationType.Should().Be(typeof(DynamicJsonPostgreSqlMethodCallTranslatorPlugin));
    }

    [Fact]
    public void PostgreSqlAndSqlServerOptions_RegisterOnlyTheirOwnTranslatorPlugins()
    {
        DbContextOptionsBuilder postgreSqlBuilder = new();
        postgreSqlBuilder.UseDynamicJsonPostgreSql();
        DbContextOptionsBuilder sqlServerBuilder = new();
        sqlServerBuilder.UseDynamicJsonSqlServer();

        Type[] postgreSqlPlugins = GetTranslatorPluginTypes(GetDynamicJsonExtension(postgreSqlBuilder.Options));
        Type[] sqlServerPlugins = GetTranslatorPluginTypes(GetSqlServerDynamicJsonExtension(sqlServerBuilder.Options));

        postgreSqlPlugins.Should().Equal(typeof(DynamicJsonPostgreSqlMethodCallTranslatorPlugin));
        sqlServerPlugins.Should().Equal(typeof(DynamicJsonSqlServerMethodCallTranslatorPlugin));
    }

    [Fact]
    public void TranslatorPlugin_BeforeFunctionTranslationStories_HasNoTranslators()
    {
        DynamicJsonPostgreSqlMethodCallTranslatorPlugin plugin = new();

        plugin.Translators.Should().BeEmpty();
    }

    private static IDbContextOptionsExtension GetDynamicJsonExtension(DbContextOptions options)
        => options.Extensions.Single(IsDynamicJsonExtension);

    private static IDbContextOptionsExtension GetSqlServerDynamicJsonExtension(DbContextOptions options)
        => options.Extensions.Single(extension =>
            extension.GetType().Assembly == typeof(DynamicJsonSqlServerDbContextOptionsBuilderExtensions).Assembly &&
            extension.GetType().Name == "DynamicJsonSqlServerOptionsExtension");

    private static Type[] GetTranslatorPluginTypes(IDbContextOptionsExtension extension)
    {
        ServiceCollection services = new();
        extension.ApplyServices(services);

        return services
            .Where(service => service.ServiceType == typeof(IMethodCallTranslatorPlugin))
            .Select(service => service.ImplementationType!)
            .ToArray();
    }

    private static bool IsDynamicJsonExtension(IDbContextOptionsExtension extension)
        => extension.GetType().Assembly == typeof(DynamicJsonPostgreSqlDbContextOptionsBuilderExtensions).Assembly &&
           extension.GetType().Name == "DynamicJsonPostgreSqlOptionsExtension";
}
