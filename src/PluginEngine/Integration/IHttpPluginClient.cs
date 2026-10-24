#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// Contract for the HTTP client that talks to remote plugin registries, allowing consumers such as
/// <see cref="RemotePluginRegistry"/> to depend on an abstraction instead of the concrete client.
/// </summary>
public interface IHttpPluginClient
{
    /// <summary>
    /// Sends a GET request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to send the request to.</param>
    /// <returns>The HTTP response message.</returns>
    Task<HttpResponseMessage> GetAsync(string url);

    /// <summary>
    /// Searches the registry for plugins matching the query.
    /// </summary>
    /// <param name="query">The free-text search query. Cannot be null or whitespace.</param>
    /// <param name="limit">Maximum number of results. Must be greater than zero.</param>
    /// <returns>The matching plugins, or an empty list when the registry is not configured or unreachable.</returns>
    Task<List<PluginInfo>> SearchPluginsAsync(string query, int limit = 20);

    /// <summary>
    /// Retrieves plugin information from the registry.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The plugin information, or null if not found or an error occurred.</returns>
    Task<PluginInfo?> GetPluginInfoAsync(Guid pluginId);

    /// <summary>
    /// Retrieves the published versions of a plugin from the registry.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The published versions, or an empty list when the registry is not configured or unreachable.</returns>
    Task<List<PluginVersionInfo>> GetPluginVersionsAsync(Guid pluginId);

    /// <summary>
    /// Uploads a plugin file to the registry.
    /// </summary>
    /// <param name="filePath">The path to the plugin file to upload.</param>
    /// <param name="description">Optional description of the plugin.</param>
    /// <returns>True if the upload was successful; otherwise, false.</returns>
    Task<bool> UploadPluginAsync(string filePath, string description = "");
}
