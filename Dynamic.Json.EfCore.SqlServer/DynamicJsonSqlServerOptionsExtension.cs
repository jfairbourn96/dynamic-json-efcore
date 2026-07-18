using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Json.EfCore.SqlServer;

/// <summary>
/// Contributes Dynamic.Json SQL Server translator services to an EF Core context's internal service provider.
/// </summary>
/// <remarks>
/// Instances are added by <see cref="DynamicJsonSqlServerDbContextOptionsBuilderExtensions.UseDynamicJsonSqlServer" />.
/// This keeps provider registration in the SQL Server package rather than the provider-neutral core package.
/// </remarks>
internal sealed class DynamicJsonSqlServerOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    /// <inheritdoc />
    public DbContextOptionsExtensionInfo Info =>
        _info ??= new DynamicJsonSqlServerOptionsExtensionInfo(this);

    /// <summary>
    /// Registers the SQL Server method-call translator plugin with EF Core's service collection.
    /// </summary>
    /// <param name="services">The service collection used by EF Core's internal service provider.</param>
    public void ApplyServices(IServiceCollection services)
    {
        services.AddScoped<IMethodCallTranslatorPlugin, DynamicJsonSqlServerMethodCallTranslatorPlugin>();
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
