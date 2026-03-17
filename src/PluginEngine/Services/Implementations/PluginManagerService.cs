// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Service implementation for managing the overall plugin lifecycle.
/// </summary>
public class PluginManagerService : IPluginManagerService
{
    private readonly IPluginLoaderService _pluginLoaderService;
    private readonly IDependencyResolutionService _dependencyResolutionService;
    private readonly IVersioningService _versioningService;
    private readonly IHotReloadService _hotReloadService;
    private bool _initialized = false;
    private DateTime _initializedAt = DateTime.MinValue;

    public PluginManagerService(
        IPluginLoaderService pluginLoaderService,
        IDependencyResolutionService dependencyResolutionService,
        IVersioningService versioningService,
        IHotReloadService hotReloadService)
    {
        _pluginLoaderService = pluginLoaderService ?? throw new ArgumentNullException(nameof(pluginLoaderService));
        _dependencyResolutionService = dependencyResolutionService ?? throw new ArgumentNullException(nameof(dependencyResolutionService));
        _versioningService = versioningService ?? throw new ArgumentNullException(nameof(versioningService));
        _hotReloadService = hotReloadService ?? throw new ArgumentNullException(nameof(hotReloadService));
    }

    /// <summary>
    /// Initializes the plugin manager.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
            return;

        _initialized = true;
        _initializedAt = DateTime.UtcNow;

        await _hotReloadService.StartHotReloadMonitoringAsync(cancellationToken);
    }

    /// <summary>
    /// Shuts down the plugin manager.
    /// </summary>
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        await _hotReloadService.StopHotReloadMonitoringAsync();

        var plugins = await _pluginLoaderService.GetAllLoadedPluginsAsync(cancellationToken);
        foreach (var plugin in plugins)
        {
            await _pluginLoaderService.UnloadPluginAsync(plugin.Id, cancellationToken);
        }

        _initialized = false;
    }

    /// <summary>
    /// Gets the manager status.
    /// </summary>
    public async Task<PluginManagerStatus> GetStatusAsync()
    {
        var allPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync();
        var stats = await GetStatisticsAsync();

        return new PluginManagerStatus
        {
            IsInitialized = _initialized,
            InitializedAt = _initializedAt,
            TotalPlugins = stats.TotalPlugins,
            LoadedPlugins = stats.LoadedPlugins,
            ActivePlugins = stats.ActivePlugins,
            FailedPlugins = stats.FailedPlugins
        };
    }

    /// <summary>
    /// Gets all plugins.
    /// </summary>
    public async Task<IEnumerable<Plugin>> GetAllPluginsAsync(CancellationToken cancellationToken = default)
    {
        return await _pluginLoaderService.GetAllLoadedPluginsAsync(cancellationToken);
    }

    /// <summary>
    /// Gets plugins filtered by status.
    /// </summary>
    public async Task<IEnumerable<Plugin>> GetPluginsByStatusAsync(PluginStatus status, CancellationToken cancellationToken = default)
    {
        var allPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync(cancellationToken);
        return allPlugins.Where(p => p.Status == status).ToList();
    }

    /// <summary>
    /// Activates a plugin.
    /// </summary>
    public async Task<bool> ActivatePluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        var plugin = await _pluginLoaderService.GetLoadedPluginAsync(pluginId, cancellationToken);
        if (plugin == null)
            return false;

        if (!await _dependencyResolutionService.ValidateDependenciesAsync(plugin, cancellationToken))
            return false;

        plugin.Status = PluginStatus.Active;
        return true;
    }

    /// <summary>
    /// Deactivates a plugin.
    /// </summary>
    public async Task<bool> DeactivatePluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        var plugin = await _pluginLoaderService.GetLoadedPluginAsync(pluginId, cancellationToken);
        if (plugin == null)
            return false;

        plugin.Status = PluginStatus.Inactive;
        return true;
    }

    /// <summary>
    /// Gets plugin details.
    /// </summary>
    public async Task<PluginDetails?> GetPluginDetailsAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        var plugin = await _pluginLoaderService.GetLoadedPluginAsync(pluginId, cancellationToken);
        if (plugin == null)
            return null;

        var details = new PluginDetails
        {
            Plugin = plugin,
            Dependencies = plugin.Dependencies,
            Capabilities = plugin.Capabilities
        };

        return details;
    }

    /// <summary>
    /// Searches for plugins.
    /// </summary>
    public async Task<IEnumerable<Plugin>> SearchPluginsAsync(PluginSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var allPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync(cancellationToken);

        var results = allPlugins.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(criteria.Name))
            results = results.Where(p => p.Name.Contains(criteria.Name, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(criteria.Author))
            results = results.Where(p => p.Author.Contains(criteria.Author, StringComparison.OrdinalIgnoreCase));

        if (criteria.Status.HasValue)
            results = results.Where(p => p.Status == criteria.Status.Value);

        if (!string.IsNullOrWhiteSpace(criteria.Version))
            results = results.Where(p => p.Version.Equals(criteria.Version));

        var list = results.ToList();
        var pageStart = (criteria.PageNumber - 1) * criteria.PageSize;
        return list.Skip(pageStart).Take(criteria.PageSize).ToList();
    }

    /// <summary>
    /// Gets manager statistics.
    /// </summary>
    public async Task<PluginManagerStatistics> GetStatisticsAsync()
    {
        var allPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync();
        var loadedPlugins = allPlugins.Where(p => p.Status == PluginStatus.Loaded).ToList();
        var activePlugins = allPlugins.Where(p => p.Status == PluginStatus.Active).ToList();
        var failedPlugins = allPlugins.Where(p => p.Status == PluginStatus.Failed).ToList();

        return new PluginManagerStatistics
        {
            TotalPlugins = allPlugins.Count(),
            LoadedPlugins = loadedPlugins.Count,
            ActivePlugins = activePlugins.Count,
            FailedPlugins = failedPlugins.Count,
            TotalMemoryUsageBytes = 0,
            TotalLoadContexts = allPlugins.Select(p => p.LoadContextId).Distinct().Count(),
            LastOperationTime = DateTime.UtcNow,
            AverageLoadTimeMs = 0
        };
    }
}
