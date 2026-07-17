using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dynamic.Json.EfCore.SqlServer;

/// <summary>
/// Extension methods for enabling SQL Server translations for Dynamic.Json.EfCore query functions.
/// </summary>
public static class DynamicJsonSqlServerDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Registers SQL Server method translators for provider-neutral dynamic JSON query functions.
    /// </summary>
    /// <param name="builder">The EF Core options builder to configure.</param>
    /// <returns>The same options builder so configuration calls can be chained.</returns>
    public static DbContextOptionsBuilder UseDynamicJsonSqlServer(this DbContextOptionsBuilder builder)
    {
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new DynamicJsonSqlServerOptionsExtension());

        return builder;
    }
}

internal sealed class DynamicJsonSqlServerOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddScoped<IMethodCallTranslatorPlugin, DynamicJsonSqlServerMethodCallTranslatorPlugin>();
        services.Replace(ServiceDescriptor.Scoped<IQueryTranslationPreprocessorFactory,
            DynamicJsonQueryTranslationPreprocessorFactory>());
        services.Replace(ServiceDescriptor.Singleton<IQuerySqlGeneratorFactory,
            DynamicJsonSqlServerQuerySqlGeneratorFactory>());
    }

    public void ApplyDefaults(IDbContextOptions options)
    {
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "using DynamicJsonSqlServer ";

        public override int GetServiceProviderHashCode()
            => typeof(DynamicJsonSqlServerOptionsExtension).GetHashCode();

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["DynamicJsonSqlServer"] = "1";
        }
    }
}
