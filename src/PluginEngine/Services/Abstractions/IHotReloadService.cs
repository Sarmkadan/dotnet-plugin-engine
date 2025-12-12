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
}

/// <summary>
/// Represents hot reload statistics.
/// </summary>
public class HotReloadStatistics
{
    public int TotalReloads { get; set; }
    public int SuccessfulReloads { get; set; }
    public int FailedReloads { get; set; }
    public DateTime? LastReloadTime { get; set; }
    public TimeSpan AverageReloadTime { get; set; }
    public List<HotReloadEvent> RecentEvents { get; set; } = new();
}

/// <summary>
/// Represents a hot reload event.
/// </summary>
public class HotReloadEvent
{
    public Guid PluginId { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Represents the hot reload status of a plugin.
/// </summary>
public class HotReloadStatus
{
    public Guid PluginId { get; set; }
    public bool SupportsHotReload { get; set; }
    public DateTime? LastReloadTime { get; set; }
    public int ReloadCount { get; set; }
    public string? LastError { get; set; }
}
