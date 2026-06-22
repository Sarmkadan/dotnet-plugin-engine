#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Services.Abstractions;

/// <summary>
/// Service interface for loading and unloading plugins.
/// </summary>
public interface IPluginLoaderService
{
    /// <summary>
    /// Loads a plugin from the specified assembly path.
    /// </summary>
    /// <param name="assemblyPath">The file path to the plugin assembly.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the loaded <see cref="Plugin"/>.</returns>
    Task<Plugin> LoadPluginAsync(string assemblyPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unloads a previously loaded plugin.
    /// </summary>
    /// <param name="pluginId">The unique ID of the plugin to unload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing <c>true</c> if successful, otherwise <c>false</c>.</returns>
    Task<bool> UnloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a loaded plugin by ID.
    /// </summary>
    /// <param name="pluginId">The unique ID of the plugin.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="Plugin"/> if found, otherwise <c>null</c>.</returns>
    Task<Plugin?> GetLoadedPluginAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currently loaded plugins.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumerable of all loaded <see cref="Plugin"/> instances.</returns>
    Task<IEnumerable<Plugin>> GetAllLoadedPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a plugin is loaded.
    /// </summary>
    /// <param name="pluginId">The unique ID of the plugin.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing <c>true</c> if loaded, otherwise <c>false</c>.</returns>
    Task<bool> IsPluginLoadedAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all plugins from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path containing plugins.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumerable of loaded <see cref="Plugin"/> instances.</returns>
    Task<IEnumerable<Plugin>> LoadPluginsFromDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reloads a plugin.
    /// </summary>
    /// <param name="pluginId">The unique ID of the plugin to reload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the reloaded <see cref="Plugin"/>.</returns>
    Task<Plugin> ReloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default);
}
