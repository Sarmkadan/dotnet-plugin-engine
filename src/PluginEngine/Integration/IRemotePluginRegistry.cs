#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// Contract for remote plugin registry operations including search, download, and publish.
/// </summary>
public interface IRemotePluginRegistry
{
    /// <summary>
    /// Searches the registry for plugins matching search criteria.
    /// </summary>
    /// <param name="query">The free-text search query. Cannot be null or whitespace.</param>
    /// <param name="limit">Maximum number of results. Must be greater than zero.</param>
    /// <returns>The matching plugins, empty when the registry returns nothing.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limit"/> is not positive.</exception>
    Task<List<PluginInfo>> SearchAsync(string query, int limit = 20);

    /// <summary>
    /// Gets information about a specific plugin from the registry.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin to retrieve.</param>
    /// <returns>The plugin information if found; otherwise, null.</returns>
    Task<PluginInfo?> GetPluginAsync(Guid pluginId);

    /// <summary>
    /// Gets all available versions of a plugin.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <returns>A list of version information for the plugin.</returns>
    Task<List<PluginVersionInfo>> GetVersionsAsync(Guid pluginId);

    /// <summary>
    /// Downloads a plugin from the registry.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin to download.</param>
    /// <param name="version">The version of the plugin to download. Cannot be null or whitespace.</param>
    /// <param name="downloadPath">The directory path where the plugin should be saved. Cannot be null or whitespace.</param>
    /// <returns>The full file path of the downloaded plugin if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="version"/> or <paramref name="downloadPath"/> is null or whitespace.</exception>
    Task<string?> DownloadPluginAsync(Guid pluginId, string version, string downloadPath);

    /// <summary>
    /// Publishes a plugin to the registry.
    /// </summary>
    /// <param name="filePath">The path to the plugin file to publish. Cannot be null or whitespace.</param>
    /// <param name="metadata">The metadata for the plugin being published. Cannot be null.</param>
    /// <returns>True if the plugin was successfully published; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is null.</exception>
    Task<bool> PublishPluginAsync(string filePath, PluginPublishMetadata metadata);

    /// <summary>
    /// Invalidates cached data for a specific plugin.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin whose cache should be invalidated.</param>
    void InvalidateCache(Guid pluginId);
}
