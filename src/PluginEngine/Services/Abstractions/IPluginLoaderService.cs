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
    Task<Plugin> LoadPluginAsync(string assemblyPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unloads a previously loaded plugin.
    /// </summary>
    Task<bool> UnloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a loaded plugin by ID.
    /// </summary>
    Task<Plugin?> GetLoadedPluginAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currently loaded plugins.
    /// </summary>
    Task<IEnumerable<Plugin>> GetAllLoadedPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a plugin is loaded.
    /// </summary>
    Task<bool> IsPluginLoadedAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all plugins from a directory.
    /// </summary>
    Task<IEnumerable<Plugin>> LoadPluginsFromDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reloads a plugin.
    /// </summary>
    Task<Plugin> ReloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default);
}
