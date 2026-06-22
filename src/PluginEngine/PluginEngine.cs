#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Configuration;
using PluginEngine.Services.Abstractions;

namespace PluginEngine;

/// <summary>
/// Main entry point and façade for the plugin engine system.
/// Provides a unified interface for managing plugins, dependencies, and hot reload.
/// </summary>
public sealed class PluginEngine
{
    private readonly IPluginManagerService _pluginManagerService;
    private readonly IPluginLoaderService _pluginLoaderService;
    private readonly IDependencyResolutionService _dependencyResolutionService;
    private readonly IVersioningService _versioningService;
    private readonly IHotReloadService _hotReloadService;
    private readonly PluginEngineOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginEngine"/> class.
    /// </summary>
    /// <param name="pluginManagerService">The plugin manager service.</param>
    /// <param name="pluginLoaderService">The plugin loader service.</param>
    /// <param name="dependencyResolutionService">The dependency resolution service.</param>
    /// <param name="versioningService">The versioning service.</param>
    /// <param name="hotReloadService">The hot reload service.</param>
    /// <param name="options">The plugin engine options.</param>
    public PluginEngine(
        IPluginManagerService pluginManagerService,
        IPluginLoaderService pluginLoaderService,
        IDependencyResolutionService dependencyResolutionService,
        IVersioningService versioningService,
        IHotReloadService hotReloadService,
        PluginEngineOptions options)
    {
        _pluginManagerService = pluginManagerService ?? throw new ArgumentNullException(nameof(pluginManagerService));
        _pluginLoaderService = pluginLoaderService ?? throw new ArgumentNullException(nameof(pluginLoaderService));
        _dependencyResolutionService = dependencyResolutionService ?? throw new ArgumentNullException(nameof(dependencyResolutionService));
        _versioningService = versioningService ?? throw new ArgumentNullException(nameof(versioningService));
        _hotReloadService = hotReloadService ?? throw new ArgumentNullException(nameof(hotReloadService));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Gets the plugin manager service.
    /// </summary>
    public IPluginManagerService PluginManager => _pluginManagerService;

    /// <summary>
    /// Gets the plugin loader service.
    /// </summary>
    public IPluginLoaderService PluginLoader => _pluginLoaderService;

    /// <summary>
    /// Gets the dependency resolution service.
    /// </summary>
    public IDependencyResolutionService DependencyResolver => _dependencyResolutionService;

    /// <summary>
    /// Gets the versioning service.
    /// </summary>
    public IVersioningService VersioningService => _versioningService;

    /// <summary>
    /// Gets the hot reload service.
    /// </summary>
    public IHotReloadService HotReloader => _hotReloadService;

    /// <summary>
    /// Gets the engine configuration options.
    /// </summary>
    public PluginEngineOptions Options => _options;

    /// <summary>
    /// Initializes the plugin engine.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _pluginManagerService.InitializeAsync(cancellationToken);
    }

    /// <summary>
    /// Shuts down the plugin engine and unloads all plugins.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        await _pluginManagerService.ShutdownAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the current engine status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="Services.Abstractions.PluginManagerStatus"/>.</returns>
    public async Task<Services.Abstractions.PluginManagerStatus> GetStatusAsync()
    {
        return await _pluginManagerService.GetStatusAsync();
    }

    /// <summary>
    /// Loads all plugins from the configured plugin directory.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the count of loaded plugins.</returns>
    public async Task<int> LoadAllPluginsAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_options.PluginDirectory))
            throw new DirectoryNotFoundException($"Plugin directory not found: {_options.PluginDirectory}");

        var loadedPlugins = await _pluginLoaderService.LoadPluginsFromDirectoryAsync(_options.PluginDirectory, cancellationToken);
        return loadedPlugins.Count();
    }

    /// <summary>
    /// Unloads all plugins.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UnloadAllPluginsAsync(CancellationToken cancellationToken = default)
    {
        var plugins = await _pluginLoaderService.GetAllLoadedPluginsAsync(cancellationToken);
        foreach (var plugin in plugins)
        {
            await _pluginLoaderService.UnloadPluginAsync(plugin.Id, cancellationToken);
        }
    }

    /// <summary>
    /// Gets engine statistics and health information.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the health information string.</returns>
    public async Task<string> GetHealthInfoAsync()
    {
        var status = await _pluginManagerService.GetStatusAsync();
        var stats = await _pluginManagerService.GetStatisticsAsync();

        var info = new System.Text.StringBuilder();
        info.AppendLine("=== Plugin Engine Health Report ===");
        info.AppendLine($"Initialized: {status.IsInitialized}");
        info.AppendLine($"Initialized At: {status.InitializedAt:O}");
        info.AppendLine($"Total Plugins: {stats.TotalPlugins}");
        info.AppendLine($"Loaded Plugins: {stats.LoadedPlugins}");
        info.AppendLine($"Active Plugins: {stats.ActivePlugins}");
        info.AppendLine($"Failed Plugins: {stats.FailedPlugins}");
        info.AppendLine($"Total Load Contexts: {stats.TotalLoadContexts}");
        info.AppendLine($"Memory Usage: {stats.TotalMemoryUsageBytes} bytes");
        info.AppendLine($"Last Operation: {stats.LastOperationTime:O}");

        return info.ToString();
    }
}
