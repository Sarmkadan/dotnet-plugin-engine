// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Marketplace;

/// <summary>
/// Extension methods for registering marketplace services with the dependency injection container.
/// </summary>
public static class MarketplaceExtensions
{
    /// <summary>
    /// Adds the plugin marketplace service to the dependency injection container.
    /// Registers <see cref="IPluginMarketplaceService"/> as a singleton backed by
    /// <see cref="PluginMarketplaceService"/>. Ensures memory caching is available
    /// so compatibility matrices can be cached between calls.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="IRemotePluginRegistry"/> to be registered first —
    /// call <c>AddPluginEngineStack()</c> or <c>AddPluginEngine()</c> before this method.
    /// </remarks>
    /// <param name="services">The service collection to add to.</param>
    public static IServiceCollection AddPluginMarketplace(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IPluginMarketplaceService, PluginMarketplaceService>();
        return services;
    }
}
