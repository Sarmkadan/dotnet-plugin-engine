#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace PluginEngine.Utils.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to simplify plugin engine registration.
/// Provides fluent API for configuring plugins, middleware, caching, and event systems.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the complete plugin engine stack including middleware, caching, and events.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">Optional configuration action for plugin engine options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddPluginEngineStack(
        this IServiceCollection services,
        Action<PluginEngineOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Core services - forward the caller's configuration as-is so no option
        // fields get silently dropped on the way through
        services.AddPluginEngine(configureOptions);

        // Middleware pipeline
        services.AddSingleton<PluginMiddlewarePipeline>();

        // Caching
        services.AddMemoryCache();
        services.AddSingleton<IPluginCache, MemoryPluginCache>();

        // Events
        services.AddSingleton<IPluginEventPublisher, PluginEventPublisher>();
        services.AddSingleton<IPluginEventSubscriber, PluginEventSubscriber>();

        // HTTP clients
        services.AddHttpClient();
        services.AddSingleton<IRemotePluginRegistry, RemotePluginRegistry>();

        // Utilities
        services.AddSingleton<PluginValidator>();
        services.AddSingleton<FileSystemHelper>();
        services.AddSingleton<VersionHelper>();

        // Formatters
        services.AddSingleton<JsonPluginFormatter>();
        services.AddSingleton<CsvPluginFormatter>();
        services.AddSingleton<XmlPluginFormatter>();
        services.AddSingleton<FormatterFactory>();

        return services;
    }

    /// <summary>
    /// Registers a custom plugin middleware implementation.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddPluginMiddleware<TMiddleware>(
        this IServiceCollection services)
        where TMiddleware : class, IPluginMiddleware
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton<IPluginMiddleware, TMiddleware>();
    }

    /// <summary>
    /// Configures the plugin middleware pipeline with specific middleware.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Configuration action for the pipeline.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    public static IServiceCollection ConfigurePluginPipeline(
        this IServiceCollection services,
        Action<PluginMiddlewarePipeline> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddSingleton(provider =>
        {
            var pipeline = new PluginMiddlewarePipeline();
            configure(pipeline);
            return pipeline;
        });

        return services;
    }

    /// <summary>
    /// Registers a custom event handler for plugin events.
    /// </summary>
    /// <typeparam name="TEvent">The event type to handle.</typeparam>
    /// <typeparam name="THandler">The handler type to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddPluginEventHandler<TEvent, THandler>(
        this IServiceCollection services)
        where TEvent : IPluginEvent
        where THandler : class, IPluginEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton<IPluginEventHandler<TEvent>, THandler>();
    }

    /// <summary>
    /// Registers a custom plugin repository implementation.
    /// </summary>
    /// <typeparam name="TRepository">The repository type to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection UsePluginRepository<TRepository>(
        this IServiceCollection services)
        where TRepository : class, IPluginRepository
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPluginRepository, TRepository>();
        return services;
    }
}