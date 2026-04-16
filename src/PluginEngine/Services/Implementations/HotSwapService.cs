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
/// Default implementation of <see cref="IHotSwapService"/>.
/// Replaces a running plugin's assembly without stopping the host application,
/// recording every swap so callers can inspect history and trigger rollbacks.
/// </summary>
public sealed class HotSwapService : IHotSwapService
{
    private readonly IPluginLoaderService _loader;
    private readonly ILogger<HotSwapService> _logger;

    private readonly ConcurrentDictionary<Guid, List<SwapRecord>> _history = new();
    private readonly ConcurrentDictionary<Guid, Func<Plugin, Task>> _postSwapCallbacks = new();

    /// <summary>Initialises a new instance of <see cref="HotSwapService"/>.</summary>
    public HotSwapService(IPluginLoaderService loader, ILogger<HotSwapService> logger)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult> SwapPluginAsync(
        Guid pluginId, string newAssemblyPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newAssemblyPath))
            return PluginOperationResult.CreateFailure("New assembly path must not be empty.", 400);

        if (!File.Exists(newAssemblyPath))
            return PluginOperationResult.CreateFailure($"Assembly not found: {newAssemblyPath}", 404);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var currentPlugin = await _loader.GetLoadedPluginAsync(pluginId, cancellationToken);

        if (currentPlugin is null)
            return PluginOperationResult.CreateFailure($"Plugin {pluginId} is not loaded.", 404);

        if (!CanSwap(currentPlugin))
            return PluginOperationResult.CreateFailure(
                $"Plugin '{currentPlugin.Name}' cannot be swapped in its current state ({currentPlugin.Status}).", 409);

        var previousPath = currentPlugin.AssemblyPath;

        _logger.LogInformation(
            "Hot-swap starting for plugin {PluginId} ({PluginName}): {OldPath} → {NewPath}",
            pluginId, currentPlugin.Name, previousPath, newAssemblyPath);

        Plugin? newPlugin = null;

        try
        {
            // Unload current assembly context
            await _loader.UnloadPluginAsync(pluginId, cancellationToken);

            // Load replacement assembly
            newPlugin = await _loader.LoadPluginAsync(newAssemblyPath, cancellationToken);
            stopwatch.Stop();

            var record = new SwapRecord
            {
                PluginId            = pluginId,
                PreviousAssemblyPath = previousPath,
                NewAssemblyPath     = newAssemblyPath,
                Success             = true,
                SwappedAtUtc        = DateTime.UtcNow,
                Duration            = stopwatch.Elapsed
            };

            AppendRecord(pluginId, record);

            _logger.LogInformation(
                "Hot-swap completed for plugin {PluginId} in {ElapsedMs}ms",
                pluginId, stopwatch.ElapsedMilliseconds);

            if (_postSwapCallbacks.TryGetValue(pluginId, out var callback))
            {
                try   { await callback(newPlugin); }
                catch (Exception cbEx)
                {
                    _logger.LogWarning(cbEx, "Post-swap callback threw for plugin {PluginId}", pluginId);
                }
            }

            return PluginOperationResult.CreateSuccess(
                $"Plugin '{newPlugin.Name}' swapped successfully in {stopwatch.ElapsedMilliseconds}ms.",
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Hot-swap failed for plugin {PluginId}; attempting rollback", pluginId);

            var failRecord = new SwapRecord
            {
                PluginId             = pluginId,
                PreviousAssemblyPath = previousPath,
                NewAssemblyPath      = newAssemblyPath,
                Success              = false,
                SwappedAtUtc         = DateTime.UtcNow,
                Duration             = stopwatch.Elapsed,
                ErrorMessage         = ex.Message
            };
            AppendRecord(pluginId, failRecord);

            // Best-effort rollback: reload previous assembly
            await AttemptRollbackAsync(pluginId, previousPath, cancellationToken);

            return PluginOperationResult.FromException(ex, stopwatch.ElapsedMilliseconds);
        }
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult> RollbackSwapAsync(
        Guid pluginId, CancellationToken cancellationToken = default)
    {
        var history = GetHistory(pluginId);
        var lastSuccessful = history.LastOrDefault(r => r.Success && !r.RolledBack);

        if (lastSuccessful is null)
            return PluginOperationResult.CreateFailure(
                $"No successful, un-rolled-back swap found for plugin {pluginId}.", 404);

        var rollbackPath = lastSuccessful.PreviousAssemblyPath;
        _logger.LogInformation(
            "Rolling back plugin {PluginId} to {PreviousPath}", pluginId, rollbackPath);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _loader.UnloadPluginAsync(pluginId, cancellationToken);
            await _loader.LoadPluginAsync(rollbackPath, cancellationToken);
            stopwatch.Stop();

            lastSuccessful.RolledBack = true;
            _logger.LogInformation(
                "Rollback complete for plugin {PluginId} in {ElapsedMs}ms",
                pluginId, stopwatch.ElapsedMilliseconds);

            return PluginOperationResult.CreateSuccess(
                $"Plugin {pluginId} rolled back to '{rollbackPath}' in {stopwatch.ElapsedMilliseconds}ms.",
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Rollback failed for plugin {PluginId}", pluginId);
            return PluginOperationResult.FromException(ex, stopwatch.ElapsedMilliseconds);
        }
    }

    /// <inheritdoc />
    public Task<PluginOperationResult<SwapRecord?>> GetLastSwapRecordAsync(
        Guid pluginId, CancellationToken cancellationToken = default)
    {
        var last = GetHistory(pluginId).LastOrDefault();
        return Task.FromResult(
            PluginOperationResult<SwapRecord?>.CreateSuccess(last, last is null
                ? "No swap history found."
                : "Last swap record retrieved."));
    }

    /// <inheritdoc />
    public Task<PluginOperationResult<List<SwapRecord>>> GetSwapHistoryAsync(
        Guid pluginId, CancellationToken cancellationToken = default)
    {
        var history = new List<SwapRecord>(GetHistory(pluginId));
        return Task.FromResult(
            PluginOperationResult<List<SwapRecord>>.CreateSuccess(history,
                $"{history.Count} swap record(s) found."));
    }

    /// <inheritdoc />
    public void RegisterPostSwapCallback(Guid pluginId, Func<Plugin, Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _postSwapCallbacks[pluginId] = callback;
    }

    /// <inheritdoc />
    public void UnregisterPostSwapCallback(Guid pluginId)
        => _postSwapCallbacks.TryRemove(pluginId, out _);

    /// <inheritdoc />
    public bool CanSwap(Plugin plugin)
    {
        if (plugin is null) return false;
        return plugin.Status is PluginStatus.Loaded or PluginStatus.Active
               && !string.IsNullOrWhiteSpace(plugin.AssemblyPath);
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private List<SwapRecord> GetHistory(Guid pluginId)
        => _history.GetOrAdd(pluginId, _ => []);

    private void AppendRecord(Guid pluginId, SwapRecord record)
    {
        var list = GetHistory(pluginId);
        lock (list)
        {
            list.Add(record);
            // Cap history at 50 entries per plugin to avoid unbounded growth
            if (list.Count > 50)
                list.RemoveAt(0);
        }
    }

    private async Task AttemptRollbackAsync(Guid pluginId, string previousPath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(previousPath) || !File.Exists(previousPath))
        {
            _logger.LogWarning(
                "Rollback skipped for {PluginId}: previous path '{Path}' is not accessible",
                pluginId, previousPath);
            return;
        }

        try
        {
            await _loader.LoadPluginAsync(previousPath, ct);
            _logger.LogInformation("Rollback-on-failure succeeded for plugin {PluginId}", pluginId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback-on-failure also failed for plugin {PluginId}", pluginId);
        }
    }
}
