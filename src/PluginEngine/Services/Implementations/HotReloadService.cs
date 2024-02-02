#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Service implementation for managing hot reload of plugins.
/// </summary>
public sealed class HotReloadService : IHotReloadService
{
    private readonly IPluginLoaderService _pluginLoaderService;
    private readonly Dictionary<Guid, Func<Plugin, Task>> _hotReloadCallbacks = new();
    private readonly ConcurrentDictionary<Guid, HotReloadStatus> _hotReloadStatusMap = new();
    private readonly List<HotReloadEvent> _recentEvents = new();
    private CancellationTokenSource? _monitoringTokenSource;
    private FileSystemWatcher? _fileSystemWatcher;
    private Task? _monitoringTask;
    private string _monitoringDirectory = string.Empty;

    public HotReloadService(IPluginLoaderService pluginLoaderService)
    {
        _pluginLoaderService = pluginLoaderService ?? throw new ArgumentNullException(nameof(pluginLoaderService));
    }

    /// <summary>
    /// Starts monitoring for changes and enables hot reload.
    /// </summary>
    public async Task StartHotReloadMonitoringAsync(CancellationToken cancellationToken = default)
    {
        if (_monitoringTokenSource is not null)
            return;

        _monitoringTokenSource = new CancellationTokenSource();
        _monitoringDirectory = AppContext.BaseDirectory;
        _fileSystemWatcher = new FileSystemWatcher(_monitoringDirectory, "*.dll");

        _fileSystemWatcher.Changed += OnPluginFileChanged;
        _fileSystemWatcher.EnableRaisingEvents = true;

        _monitoringTask = Task.CompletedTask;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Stops monitoring for changes.
    /// </summary>
    public async Task StopHotReloadMonitoringAsync()
    {
        if (_monitoringTokenSource is null)
            return;

        _monitoringTokenSource.Cancel();
        _fileSystemWatcher?.Dispose();
        _fileSystemWatcher = null;
        _monitoringTokenSource = null;

        if (_monitoringTask is not null)
            await _monitoringTask;
    }

    /// <summary>
    /// Checks if a plugin can be hot reloaded.
    /// </summary>
    public bool CanHotReload(Plugin plugin)
    {
        if (plugin is null)
            return false;

        return plugin.SupportsHotReload && plugin.Status == PluginStatus.Loaded;
    }

    /// <summary>
    /// Performs a hot reload of a plugin.
    /// </summary>
    public async Task<bool> HotReloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var plugin = await _pluginLoaderService.GetLoadedPluginAsync(pluginId, cancellationToken);

        if (plugin is null)
            return false;

        if (!CanHotReload(plugin))
            return false;

        try
        {
            var reloaded = await _pluginLoaderService.ReloadPluginAsync(pluginId, cancellationToken);
            stopwatch.Stop();

            var @event = new HotReloadEvent
            {
                PluginId = pluginId,
                Timestamp = DateTime.UtcNow,
                Success = true,
                Duration = stopwatch.Elapsed
            };

            RecordHotReloadEvent(@event);
            UpdateHotReloadStatus(pluginId, true, null);

            if (_hotReloadCallbacks.TryGetValue(pluginId, out var callback))
                await callback(reloaded);

            return true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var @event = new HotReloadEvent
            {
                PluginId = pluginId,
                Timestamp = DateTime.UtcNow,
                Success = false,
                ErrorMessage = ex.Message,
                Duration = stopwatch.Elapsed
            };

            RecordHotReloadEvent(@event);
            UpdateHotReloadStatus(pluginId, false, ex.Message);

            return false;
        }
    }

    /// <summary>
    /// Gets hot reload statistics.
    /// </summary>
    public async Task<HotReloadStatistics> GetStatisticsAsync()
    {
        var stats = new HotReloadStatistics
        {
            TotalReloads = _recentEvents.Count,
            SuccessfulReloads = _recentEvents.Count(e => e.Success),
            FailedReloads = _recentEvents.Count(e => !e.Success),
            LastReloadTime = _recentEvents.LastOrDefault()?.Timestamp,
            RecentEvents = new List<HotReloadEvent>(_recentEvents.TakeLast(10))
        };

        if (_recentEvents.Count > 0)
            stats.AverageReloadTime = TimeSpan.FromMilliseconds(_recentEvents.Average(e => e.Duration.TotalMilliseconds));

        return await Task.FromResult(stats);
    }

    /// <summary>
    /// Registers a hot reload callback.
    /// </summary>
    public void RegisterHotReloadCallback(Guid pluginId, Func<Plugin, Task> callback)
    {
        if (callback is not null)
            _hotReloadCallbacks[pluginId] = callback;
    }

    /// <summary>
    /// Unregisters a hot reload callback.
    /// </summary>
    public void UnregisterHotReloadCallback(Guid pluginId)
    {
        _hotReloadCallbacks.Remove(pluginId);
    }

    /// <summary>
    /// Gets the hot reload status of a plugin.
    /// </summary>
    public async Task<HotReloadStatus?> GetHotReloadStatusAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_hotReloadStatusMap.TryGetValue(pluginId, out var status) ? status : null);
    }

    /// <summary>
    /// Removes callbacks belonging to a specific AssemblyLoadContext to prevent memory leaks.
    /// </summary>
    public void RemoveCallbacksForContext(System.Runtime.Loader.AssemblyLoadContext context)
    {
        foreach (var key in _hotReloadCallbacks.Keys.ToList())
        {
            var callback = _hotReloadCallbacks[key];
            var assembly = callback.Method.DeclaringType?.Assembly;
            if (assembly != null && System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(assembly) == context)
            {
                _hotReloadCallbacks.Remove(key);
            }
        }
    }

    private void OnPluginFileChanged(object sender, FileSystemEventArgs e)
    {
        // File changed event handler - would typically trigger hot reload logic
    }

    private void RecordHotReloadEvent(HotReloadEvent @event)
    {
        _recentEvents.Add(@event);
        if (_recentEvents.Count > 100)
            _recentEvents.RemoveAt(0);
    }

    private void UpdateHotReloadStatus(Guid pluginId, bool success, string? errorMessage)
    {
        var status = _hotReloadStatusMap.AddOrUpdate(pluginId,
            new HotReloadStatus
            {
                PluginId = pluginId,
                SupportsHotReload = true,
                LastReloadTime = DateTime.UtcNow,
                ReloadCount = 1,
                LastError = errorMessage
            },
            (id, existing) =>
            {
                existing.LastReloadTime = DateTime.UtcNow;
                existing.ReloadCount++;
                if (!success)
                    existing.LastError = errorMessage;
                return existing;
            });
    }
}
