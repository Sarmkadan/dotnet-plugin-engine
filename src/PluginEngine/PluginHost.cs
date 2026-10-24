#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PluginEngine.Configuration;
using PluginEngine.Services.Abstractions;

namespace PluginEngine;

/// <summary>
/// Host for managing the plugin engine lifecycle with detailed logging.
/// </summary>
public sealed class PluginHost : IPluginHost, IDisposable
{
    private readonly ILogger<PluginHost> _logger;
    private readonly PluginEngine _engine;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginHost"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="engine">The plugin engine.</param>
    public PluginHost(ILogger<PluginHost> logger, PluginEngine engine)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(engine);

        _logger = logger;
        _engine = engine;
    }

    /// <summary>
    /// Starts the plugin host and initializes the engine.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(PluginHost));
        }

        PluginHostLog.HostStart(_logger);
        await _engine.InitializeAsync(cancellationToken);

        var loadedCount = await _engine.LoadAllPluginsAsync(cancellationToken);
        var status = await _engine.GetStatusAsync();

        PluginHostLog.PluginHostStarted(_logger, loadedCount, status.InitializedAt);

        // Log each loaded plugin
        var plugins = await _engine.PluginLoader.GetAllLoadedPluginsAsync(cancellationToken);
        foreach (var plugin in plugins)
        {
            PluginHostLog.PluginLoaded(_logger, plugin.Id.ToString(), plugin.Name, plugin.Version?.ToString());
        }
    }

    /// <summary>
    /// Stops the plugin host and shuts down the engine.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(PluginHost));
        }

        PluginHostLog.HostStop(_logger);
        var plugins = await _engine.PluginLoader.GetAllLoadedPluginsAsync(cancellationToken);

        foreach (var plugin in plugins)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _engine.PluginLoader.UnloadPluginAsync(plugin.Id, cancellationToken);
                stopwatch.Stop();
                PluginHostLog.PluginUnloaded(_logger, plugin.Id.ToString(), plugin.Name, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                PluginHostLog.PluginUnloadFailed(_logger, plugin.Id.ToString(), plugin.Name, stopwatch.ElapsedMilliseconds, ex.ToString());
            }
        }

        await _engine.ShutdownAsync(cancellationToken);
        PluginHostLog.PluginHostStopped(_logger);
    }

    /// <summary>
    /// Executes a plugin by its ID.
    /// </summary>
    /// <param name="pluginId">The plugin ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecutePluginAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(PluginHost));
        }

        if (string.IsNullOrWhiteSpace(pluginId))
        {
            throw new ArgumentException("Plugin ID cannot be null or empty.", nameof(pluginId));
        }

        if (!Guid.TryParse(pluginId, out var parsedPluginId))
        {
            throw new ArgumentException("Plugin ID must be a valid GUID.", nameof(pluginId));
        }

        var correlationId = Guid.NewGuid().ToString();
        try
        {
            // In a real implementation, this would invoke the plugin's entry point
            // For now, we simulate execution by checking if the plugin is loaded
            var plugin = await _engine.PluginLoader.GetLoadedPluginAsync(parsedPluginId, cancellationToken);
            if (plugin == null)
            {
                throw new InvalidOperationException($"Plugin '{pluginId}' is not loaded.");
            }

            // Simulate plugin execution (placeholder)
            await Task.CompletedTask;
            PluginHostLog.PluginExecuted(_logger, pluginId, correlationId);
        }
        catch (Exception ex)
        {
            PluginHostLog.PluginExecutionFailed(_logger, pluginId, correlationId, ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        // Note: We don't dispose the engine here as it might be shared
        GC.SuppressFinalize(this);
    }
}

// Logger messages for PluginHost
internal static partial class PluginHostLog
{
    [LoggerMessage(EventId = 1000, Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Plugin host started. Loaded {LoadedCount} plugins at {StartedAt:O}.")]
    public static partial void PluginHostStarted(ILogger logger, int loadedCount, DateTimeOffset startedAt);

    [LoggerMessage(EventId = 1001, Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Plugin host stopped.")]
    public static partial void PluginHostStopped(ILogger logger);

    [LoggerMessage(EventId = 1002, Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Plugin loaded: {PluginId} ({PluginName}) version {PluginVersion}.")]
    public static partial void PluginLoaded(ILogger logger, string pluginId, string pluginName, string? pluginVersion);

    [LoggerMessage(EventId = 1003, Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Plugin unloaded: {PluginId} ({PluginName}) in {ElapsedMilliseconds}ms.")]
    public static partial void PluginUnloaded(ILogger logger, string pluginId, string pluginName, long elapsedMilliseconds);

    [LoggerMessage(EventId = 1004, Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Failed to unload plugin: {PluginId} ({PluginName}) after {ElapsedMilliseconds}ms. Error: {ErrorMessage}")]
    public static partial void PluginUnloadFailed(ILogger logger, string pluginId, string pluginName, long elapsedMilliseconds, string errorMessage);

    [LoggerMessage(EventId = 1005, Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Plugin executed: {PluginId} with correlation ID {CorrelationId}.")]
    public static partial void PluginExecuted(ILogger logger, string pluginId, string correlationId);

    [LoggerMessage(EventId = 1006, Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Plugin execution failed: {PluginId} with correlation ID {CorrelationId}. Error: {ErrorMessage}")]
    public static partial void PluginExecutionFailed(ILogger logger, string pluginId, string correlationId, string errorMessage);

    [LoggerMessage(EventId = 1007, Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Starting plugin host...")]
    public static partial void HostStart(ILogger logger);

    [LoggerMessage(EventId = 1008, Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Stopping plugin host...")]
    public static partial void HostStop(ILogger logger);
}