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
    /// Initializes a new instance of the PluginEngine class.
    /// </summary>
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
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _pluginManagerService.InitializeAsync(cancellationToken);
    }

    /// <summary>
    /// Shuts down the plugin engine and unloads all plugins.
    /// </summary>
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        await _pluginManagerService.ShutdownAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the current engine status.
    /// </summary>
    public async Task<Services.Abstractions.PluginManagerStatus> GetStatusAsync()
    {
        return await _pluginManagerService.GetStatusAsync();
    }

    /// <summary>
    /// Loads all plugins from the configured plugin directory.
    /// </summary>
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
