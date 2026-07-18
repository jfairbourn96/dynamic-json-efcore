using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Dynamic.Json.EfCore.SqlServer;

/// <summary>
/// Describes the Dynamic.Json SQL Server options extension to EF Core's service-provider cache and diagnostics.
/// </summary>
/// <param name="extension">The options extension whose metadata is being described.</param>
internal sealed class DynamicJsonSqlServerOptionsExtensionInfo(
    IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
{
    /// <inheritdoc />
    public override bool IsDatabaseProvider => false;

    /// <inheritdoc />
    public override string LogFragment => "using DynamicJsonSqlServer ";

    /// <summary>
    /// Returns a stable hash contribution for EF Core's internal service-provider cache.
    /// </summary>
    /// <returns>A hash code shared by equivalent Dynamic.Json SQL Server extensions.</returns>
    public override int GetServiceProviderHashCode()
        => typeof(DynamicJsonSqlServerOptionsExtension).GetHashCode();

    /// <summary>
    /// Determines whether another extension uses the same registered services.
    /// </summary>
    /// <param name="other">The other options extension metadata to compare.</param>
    /// <returns>
    /// <see langword="true" /> when the other extension is also a Dynamic.Json SQL Server extension;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        => other is DynamicJsonSqlServerOptionsExtensionInfo;

    /// <summary>
    /// Adds this extension's stable identity to EF Core's options debug information.
    /// </summary>
    /// <param name="debugInfo">The debug-information dictionary populated by EF Core extensions.</param>
    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    {
        debugInfo["DynamicJsonSqlServer"] = "1";
    }
}
