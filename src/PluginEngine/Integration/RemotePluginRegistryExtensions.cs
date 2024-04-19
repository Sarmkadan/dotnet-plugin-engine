#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// Provides extension methods for <see cref="RemotePluginRegistry"/> to enhance plugin registry operations
/// with additional convenience functionality.
/// </summary>
public static class RemotePluginRegistryExtensions
{
    /// <summary>
    /// Searches for plugins by name or tag with case-insensitive matching.
    /// </summary>
    /// <param name="registry">The plugin registry instance</param>
    /// <param name="query">Search term (plugin name, author, or tag)</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <returns>List of matching plugins, ordered by relevance</returns>
    public static async Task<List<PluginInfo>> SearchByNameOrTagAsync(this RemotePluginRegistry registry, string query, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var results = await registry.SearchAsync(query, limit);

        // Filter by name (case-insensitive) and sort by relevance
        return results
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => !p.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .ThenBy(p => p.Name)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Gets the latest stable version of a plugin.
    /// </summary>
    /// <param name="registry">The plugin registry instance</param>
    /// <param name="pluginId">The plugin identifier</param>
    /// <returns>The latest stable version info, or null if none found</returns>
    public static async Task<PluginVersionInfo?> GetLatestStableVersionAsync(this RemotePluginRegistry registry, Guid pluginId)
    {
        var versions = await registry.GetVersionsAsync(pluginId);

        return versions
            .Where(v => v.IsStable)
            .OrderByDescending(v => v.PublishedAtUtc)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the latest prerelease version of a plugin.
    /// </summary>
    /// <param name="registry">The plugin registry instance</param>
    /// <param name="pluginId">The plugin identifier</param>
    /// <returns>The latest prerelease version info, or null if none found</returns>
    public static async Task<PluginVersionInfo?> GetLatestPrereleaseVersionAsync(this RemotePluginRegistry registry, Guid pluginId)
    {
        var versions = await registry.GetVersionsAsync(pluginId);

        return versions
            .Where(v => v.IsPrerelease)
            .OrderByDescending(v => v.PublishedAtUtc)
            .FirstOrDefault();
    }

    /// <summary>
    /// Downloads the latest stable version of a plugin.
    /// </summary>
    /// <param name="registry">The plugin registry instance</param>
    /// <param name="pluginId">The plugin identifier</param>
    /// <param name="downloadPath">Directory to save the plugin file</param>
    /// <returns>Path to the downloaded plugin file, or null if failed</returns>
    public static async Task<string?> DownloadLatestStableAsync(this RemotePluginRegistry registry, Guid pluginId, string downloadPath)
    {
        var latestVersion = await registry.GetLatestStableVersionAsync(pluginId);

        if (latestVersion is null)
        {
            return null;
        }

        return await registry.DownloadPluginAsync(pluginId, latestVersion.Version, downloadPath);
    }
}