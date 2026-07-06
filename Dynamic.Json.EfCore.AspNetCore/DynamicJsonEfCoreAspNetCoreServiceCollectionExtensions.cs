using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dynamic.Json.EfCore.AspNetCore;

/// <summary>
/// Extension methods for registering Dynamic.Json.EfCore ASP.NET Core services.
/// </summary>
public static class DynamicJsonEfCoreAspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default Dynamic.Json.EfCore ASP.NET Core services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection so registration calls can be chained.</returns>
    public static IServiceCollection AddDynamicJsonEfCoreAspNetCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IDynamicSearchQueryParser, DynamicSearchQueryParser>();

        return services;
    }
}
