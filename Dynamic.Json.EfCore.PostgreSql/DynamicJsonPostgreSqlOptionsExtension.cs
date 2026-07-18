using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Json.EfCore.PostgreSql;

/// <summary>
/// Contributes Dynamic.Json PostgreSQL configuration to an EF Core context.
/// </summary>
/// <remarks>
/// Instances are added by
/// <see cref="DynamicJsonPostgreSqlDbContextOptionsBuilderExtensions.UseDynamicJsonPostgreSql" />.
/// Translation service registration is introduced in a separate story.
/// </remarks>
internal sealed class DynamicJsonPostgreSqlOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    /// <inheritdoc />
    public DbContextOptionsExtensionInfo Info =>
        _info ??= new DynamicJsonPostgreSqlOptionsExtensionInfo(this);

    /// <summary>
    /// Applies services owned by the Dynamic.Json PostgreSQL extension.
    /// </summary>
    /// <param name="services">The service collection used by EF Core's internal service provider.</param>
    /// <remarks>No services are required until PostgreSQL translation support is introduced.</remarks>
    public void ApplyServices(IServiceCollection services)
    {
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
