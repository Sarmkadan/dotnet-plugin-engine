#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading;
using System.Threading.Tasks;

namespace PluginEngine.Services.Abstractions;

/// <summary>
/// Defines methods for hosting the plugin engine lifecycle.
/// </summary>
public interface IPluginHost : IDisposable
{
    /// <summary>
    /// Starts the plugin host and initializes the engine.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the plugin host and shuts down the engine.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a plugin by its ID.
    /// </summary>
    /// <param name="pluginId">The plugin ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecutePluginAsync(string pluginId, CancellationToken cancellationToken = default);
}