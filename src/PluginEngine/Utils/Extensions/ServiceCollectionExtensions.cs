// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to simplify plugin engine registration.
/// Provides fluent API for configuring plugins, middleware, caching, and event systems.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the complete plugin engine stack including middleware, caching, and events.
    /// </summary>
    public static IServiceCollection AddPluginEngineStack(
        this IServiceCollection services,
        Action<PluginEngineOptions>? configureOptions = null)
    {
        var options = new PluginEngineOptions();
        configureOptions?.Invoke(options);

        // Core services
        services.AddPluginEngine(o =>
        {
            o.PluginDirectory = options.PluginDirectory;
            o.EnableHotReload = options.EnableHotReload;
            o.OperationTimeoutMs = options.OperationTimeoutMs;
        });

        // Middleware pipeline
        services.AddSingleton<PluginMiddlewarePipeline>();
        services.AddSingleton<IPluginMiddleware, LoggingMiddleware>();
        services.AddSingleton<IPluginMiddleware, ErrorHandlingMiddleware>();

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

        // Background services
        services.AddHostedService<BackgroundPluginMonitor>();
        services.AddHostedService<PluginHealthCheckService>();

        return services;
    }

    /// <summary>
    /// Registers a custom plugin middleware implementation.
    /// </summary>
    public static IServiceCollection AddPluginMiddleware<TMiddleware>(
        this IServiceCollection services)
        where TMiddleware : class, IPluginMiddleware
    {
        return services.AddSingleton<IPluginMiddleware, TMiddleware>();
    }

    /// <summary>
    /// Configures the plugin middleware pipeline with specific middleware.
    /// </summary>
    public static IServiceCollection ConfigurePluginPipeline(
        this IServiceCollection services,
        Action<PluginMiddlewarePipeline> configure)
    {
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
    public static IServiceCollection AddPluginEventHandler<TEvent, THandler>(
        this IServiceCollection services)
        where TEvent : IPluginEvent
        where THandler : class, IPluginEventHandler<TEvent>
    {
        return services.AddSingleton<IPluginEventHandler<TEvent>, THandler>();
    }

    /// <summary>
    /// Registers a custom plugin repository implementation.
    /// </summary>
    public static IServiceCollection UsePluginRepository<TRepository>(
        this IServiceCollection services)
        where TRepository : class, IPluginRepository
    {
        services.AddSingleton<IPluginRepository, TRepository>();
        return services;
    }
}
