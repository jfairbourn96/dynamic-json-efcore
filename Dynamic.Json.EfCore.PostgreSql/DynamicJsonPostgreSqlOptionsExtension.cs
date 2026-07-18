using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Json.EfCore.PostgreSql;

/// <summary>
/// Contributes Dynamic.Json PostgreSQL configuration to an EF Core context.
/// </summary>
/// <remarks>
/// Instances are added by
/// <see cref="DynamicJsonPostgreSqlDbContextOptionsBuilderExtensions.UseDynamicJsonPostgreSql" />.
/// PostgreSQL translator plugins are registered only when this extension is present.
/// </remarks>
internal sealed class DynamicJsonPostgreSqlOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    /// <inheritdoc />
    public DbContextOptionsExtensionInfo Info =>
        _info ??= new DynamicJsonPostgreSqlOptionsExtensionInfo(this);

    /// <summary>
    /// Registers the PostgreSQL method-call translator plugin with EF Core's service collection.
    /// </summary>
    /// <param name="services">The service collection used by EF Core's internal service provider.</param>
    public void ApplyServices(IServiceCollection services)
    {
        services.AddScoped<IMethodCallTranslatorPlugin, DynamicJsonPostgreSqlMethodCallTranslatorPlugin>();
    }

    /// <summary>
    /// Applies provider defaults required by this extension.
    /// </summary>
    /// <param name="options">The completed context options.</param>
    /// <remarks>No additional defaults are currently required.</remarks>
    public void ApplyDefaults(IDbContextOptions options)
    {
    }

    /// <summary>
    /// Validates the completed context options for this extension.
    /// </summary>
    /// <param name="options">The completed context options.</param>
    /// <remarks>No additional validation is currently required.</remarks>
    public void Validate(IDbContextOptions options)
    {
    }
}
