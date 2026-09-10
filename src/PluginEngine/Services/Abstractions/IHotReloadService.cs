#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Services.Abstractions;

/// <summary>
/// Service interface for managing hot reload of plugins.
/// </summary>
public interface IHotReloadService
{
    /// <summary>
    /// Starts monitoring for changes and enables hot reload.
    /// </summary>
    Task StartHotReloadMonitoringAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops monitoring for changes.
    /// </summary>
    Task StopHotReloadMonitoringAsync();

    /// <summary>
    /// Checks if a plugin can be hot reloaded.
    /// </summary>
    bool CanHotReload(Plugin plugin);

    /// <summary>
    /// Performs a hot reload of a plugin.
    /// </summary>
    Task<bool> HotReloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets hot reload statistics.
    /// </summary>
    Task<HotReloadStatistics> GetStatisticsAsync();

    /// <summary>
    /// Registers a hot reload callback.
    /// </summary>
    void RegisterHotReloadCallback(Guid pluginId, Func<Plugin, Task> callback);

    /// <summary>
    /// Unregisters a hot reload callback.
    /// </summary>
    void UnregisterHotReloadCallback(Guid pluginId);

    /// <summary>
    /// Gets the hot reload status of a plugin.
    /// </summary>
    Task<HotReloadStatus?> GetHotReloadStatusAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes callbacks belonging to a specific AssemblyLoadContext to prevent memory leaks.
    /// </summary>
    void RemoveCallbacksForContext(System.Runtime.Loader.AssemblyLoadContext context);
}

/// <summary>
/// Represents hot reload statistics.
/// </summary>
public sealed class HotReloadStatistics
{
    /// <summary>
    /// Gets or sets the total number of reloads.
    /// </summary>
    public int TotalReloads { get; set; }

    /// <summary>
    /// Gets or sets the number of successful reloads.
    /// </summary>
    public int SuccessfulReloads { get; set; }

    /// <summary>
    /// Gets or sets the number of failed reloads.
    /// </summary>
    public int FailedReloads { get; set; }

    /// <summary>
    /// Gets or sets the time of the last reload.
    /// </summary>
    public DateTime? LastReloadTime { get; set; }

    /// <summary>
    /// Gets or sets the average reload time.
    /// </summary>
    public TimeSpan AverageReloadTime { get; set; }

    /// <summary>
    /// Gets or sets the list of recent hot reload events.
    /// </summary>
    public List<HotReloadEvent> RecentEvents { get; set; } = new();
}

/// <summary>
/// Represents a hot reload event.
/// </summary>
public sealed class HotReloadEvent
{
    /// <summary>
    /// Gets or sets the plugin identifier.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the event.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the reload was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if the reload failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the duration of the reload operation.
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Represents the hot reload status of a plugin.
/// </summary>
public sealed class HotReloadStatus
{
    /// <summary>
    /// Gets or sets the plugin identifier.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the plugin supports hot reload.
    /// </summary>
    public bool SupportsHotReload { get; set; }

    /// <summary>
    /// Gets or sets the time of the last reload.
    /// </summary>
    public DateTime? LastReloadTime { get; set; }

    /// <summary>
    /// Gets or sets the number of times the plugin has been reloaded.
    /// </summary>
    public int ReloadCount { get; set; }

    /// <summary>
    /// Gets or sets the last error message encountered during reload.
    /// </summary>
    public string? LastError { get; set; }
}