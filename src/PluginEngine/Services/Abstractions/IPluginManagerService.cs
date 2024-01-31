#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Services.Abstractions;

/// <summary>
/// Service interface for managing the overall plugin lifecycle.
/// </summary>
public interface IPluginManagerService
{
    /// <summary>
    /// Initializes the plugin manager.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuts down the plugin manager.
    /// </summary>
    Task ShutdownAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the manager status.
    /// </summary>
    Task<PluginManagerStatus> GetStatusAsync();

    /// <summary>
    /// Gets all plugins.
    /// </summary>
    Task<IEnumerable<Plugin>> GetAllPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plugins filtered by status.
    /// </summary>
    Task<IEnumerable<Plugin>> GetPluginsByStatusAsync(PluginStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a plugin.
    /// </summary>
    Task<bool> ActivatePluginAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a plugin.
    /// </summary>
    Task<bool> DeactivatePluginAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plugin details.
    /// </summary>
    Task<PluginDetails?> GetPluginDetailsAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for plugins.
    /// </summary>
    Task<IEnumerable<Plugin>> SearchPluginsAsync(PluginSearchCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets manager statistics.
    /// </summary>
    Task<PluginManagerStatistics> GetStatisticsAsync();
}

/// <summary>
/// Represents the status of the plugin manager.
/// </summary>
public sealed class PluginManagerStatus
{
    public bool IsInitialized { get; set; }
    public DateTime InitializedAt { get; set; }
    public int TotalPlugins { get; set; }
    public int LoadedPlugins { get; set; }
    public int ActivePlugins { get; set; }
    public int FailedPlugins { get; set; }
    public string? LastError { get; set; }
}

/// <summary>
/// Represents detailed information about a plugin.
/// </summary>
public sealed class PluginDetails
{
    public Plugin Plugin { get; set; } = new();
    public PluginMetadata? Metadata { get; set; }
    public IEnumerable<PluginAssembly> Assemblies { get; set; } = Enumerable.Empty<PluginAssembly>();
    public IEnumerable<PluginDependency> Dependencies { get; set; } = Enumerable.Empty<PluginDependency>();
    public IEnumerable<PluginCapability> Capabilities { get; set; } = Enumerable.Empty<PluginCapability>();
}

/// <summary>
/// Represents search criteria for plugins.
/// </summary>
public sealed class PluginSearchCriteria
{
    public string? Name { get; set; }
    public string? Author { get; set; }
    public PluginStatus? Status { get; set; }
    public string? Version { get; set; }
    public List<string> Tags { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Represents plugin manager statistics.
/// </summary>
public sealed class PluginManagerStatistics
{
    public int TotalPlugins { get; set; }
    public int LoadedPlugins { get; set; }
    public int ActivePlugins { get; set; }
    public int FailedPlugins { get; set; }
    public long TotalMemoryUsageBytes { get; set; }
    public int TotalLoadContexts { get; set; }
    public DateTime? LastOperationTime { get; set; }
    public double AverageLoadTimeMs { get; set; }
}
