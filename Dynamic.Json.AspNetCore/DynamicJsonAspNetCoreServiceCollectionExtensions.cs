using Dynamic.Json.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dynamic.Json.AspNetCore;

/// <summary>
/// Extension methods for registering Dynamic.Json ASP.NET Core services.
/// </summary>
public static class DynamicJsonAspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default Dynamic.Json ASP.NET Core services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection so registration calls can be chained.</returns>
    public static IServiceCollection AddDynamicJsonAspNetCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IDynamicSearchQueryParser, DynamicSearchQueryParser>();

        return services;
    }

    /// <summary>
    /// Registers the default Dynamic.Json ASP.NET Core services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection so registration calls can be chained.</returns>
    [Obsolete("Use AddDynamicJsonAspNetCore instead.")]
    public static IServiceCollection AddDynamicJsonEfCoreAspNetCore(this IServiceCollection services)
    {
        return services.AddDynamicJsonAspNetCore();
    }
}