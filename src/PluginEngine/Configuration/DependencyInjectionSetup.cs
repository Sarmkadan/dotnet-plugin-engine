#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Data.Repositories;
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;

namespace PluginEngine.Configuration;

/// <summary>
/// Extension methods for setting up the plugin engine in the DI container.
/// </summary>
public static class DependencyInjectionSetup
{
    /// <summary>
    /// Adds plugin engine services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddPluginEngine(
        this IServiceCollection services,
        Action<PluginEngineOptions>? configureOptions = null)
    {
        var options = new PluginEngineOptions();
        configureOptions?.Invoke(options);

        if (!options.IsValid())
        {
            var errors = options.GetValidationErrors();
            throw new InvalidOperationException($"Plugin engine configuration is invalid: {string.Join(", ", errors)}");
        }

        // Register configuration
        services.AddSingleton(options);

        // Register repositories
        services.AddSingleton<IPluginRepository, PluginRepository>();

        // Register services
        services.AddSingleton<IPluginLoaderService, PluginLoaderService>();
        services.AddSingleton<IDependencyResolutionService, DependencyResolutionService>();
        services.AddSingleton<IVersioningService, VersioningService>();
        services.AddSingleton<IHotReloadService, HotReloadService>();
        services.AddSingleton<IPluginManagerService, PluginManagerService>();

        return services;
    }

    /// <summary>
    /// Adds plugin engine services with default options.
    /// </summary>
    public static IServiceCollection AddPluginEngine(this IServiceCollection services)
    {
        return services.AddPluginEngine(null);
    }
}
