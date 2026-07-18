using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Dynamic.Json.EfCore.PostgreSql;

/// <summary>
/// Describes the Dynamic.Json PostgreSQL options extension to EF Core's service-provider cache and diagnostics.
/// </summary>
/// <param name="extension">The options extension whose metadata is being described.</param>
internal sealed class DynamicJsonPostgreSqlOptionsExtensionInfo(
    IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
{
    /// <inheritdoc />
    public override bool IsDatabaseProvider => false;

    /// <inheritdoc />
    public override string LogFragment => "using DynamicJsonPostgreSql ";

    /// <summary>
    /// Returns a stable hash contribution for EF Core's internal service-provider cache.
    /// </summary>
    /// <returns>A hash code shared by equivalent Dynamic.Json PostgreSQL extensions.</returns>
    public override int GetServiceProviderHashCode()
        => typeof(DynamicJsonPostgreSqlOptionsExtension).GetHashCode();

    /// <summary>
    /// Determines whether another extension uses the same registered services.
    /// </summary>
    /// <param name="other">The other options extension metadata to compare.</param>
    /// <returns>
    /// <see langword="true" /> when the other extension is also a Dynamic.Json PostgreSQL extension;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        => other is DynamicJsonPostgreSqlOptionsExtensionInfo;

    /// <summary>
    /// Adds this extension's stable identity to EF Core's options debug information.
    /// </summary>
    /// <param name="debugInfo">The debug-information dictionary populated by EF Core extensions.</param>
    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    {
        debugInfo["DynamicJsonPostgreSql"] = "1";
    }
}
