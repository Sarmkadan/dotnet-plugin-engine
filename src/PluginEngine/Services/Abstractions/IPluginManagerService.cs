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
    /// Discovers and loads all plugins from configured directories, resolves dependencies,
    /// and activates auto-start plugins. Must be called before any other operations.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the initialization.</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gracefully shuts down all active plugins in reverse dependency order and
    /// releases all associated AssemblyLoadContexts.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the shutdown.</param>
    Task ShutdownAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current status of the plugin manager including initialization state,
    /// plugin counts by category, and last error information.
    /// </summary>
    /// <returns>A <see cref="PluginManagerStatus"/> snapshot.</returns>
    Task<PluginManagerStatus> GetStatusAsync();

    /// <summary>
    /// Returns all registered plugins regardless of their current status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An enumerable of all <see cref="Plugin"/> instances.</returns>
    Task<IEnumerable<Plugin>> GetAllPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns plugins filtered by their current lifecycle status.
    /// </summary>
    /// <param name="status">The plugin status to filter by (e.g., Active, Inactive, Failed).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An enumerable of plugins matching the specified status.</returns>
    Task<IEnumerable<Plugin>> GetPluginsByStatusAsync(PluginStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a previously loaded but inactive plugin. Resolves its dependencies
    /// and invokes the plugin's initialization hook.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin to activate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the plugin was activated successfully; <c>false</c> if not found or activation failed.</returns>
    Task<bool> ActivatePluginAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a running plugin, invoking its shutdown hook and releasing resources.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin to deactivate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the plugin was deactivated successfully; <c>false</c> if not found or already inactive.</returns>
    Task<bool> DeactivatePluginAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves detailed information about a plugin including metadata, loaded assemblies,
    /// dependency graph, and declared capabilities.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PluginDetails"/> object, or <c>null</c> if the plugin is not found.</returns>
    Task<PluginDetails?> GetPluginDetailsAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for plugins matching the specified criteria with pagination support.
    /// </summary>
    /// <param name="criteria">Search filters including name, author, status, version, and tags.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated enumerable of matching <see cref="Plugin"/> instances.</returns>
    Task<IEnumerable<Plugin>> SearchPluginsAsync(PluginSearchCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns aggregate statistics about plugin manager operations including memory usage,
    /// load context counts, and average load times.
    /// </summary>
    /// <returns>A <see cref="PluginManagerStatistics"/> snapshot.</returns>
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
