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
    /// Searches for plugins by name, author, or tag with case-insensitive matching.
    /// </summary>
    /// <param name="registry">The plugin registry instance. Must not be null.</param>
    /// <param name="query">Search term (plugin name, author, or tag). Must not be null or whitespace.</param>
    /// <param name="limit">Maximum number of results to return. Must be greater than 0.</param>
    /// <returns>List of matching plugins, ordered by relevance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="query"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="limit"/> is less than or equal to 0.</exception>
    public static async Task<List<PluginInfo>> SearchByNameOrTagAsync(this RemotePluginRegistry registry, string query, int limit = 20)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, 0);

        var results = await registry.SearchAsync(query, limit);

        // Filter by name or author (case-insensitive) and sort by relevance
        return results
            .Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (p.Author?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(p => p.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Gets the latest stable version of a plugin.
    /// </summary>
    /// <param name="registry">The plugin registry instance. Must not be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The latest stable version info, or null if none found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/> is null.</exception>
    public static async Task<PluginVersionInfo?> GetLatestStableVersionAsync(this RemotePluginRegistry registry, Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var versions = await registry.GetVersionsAsync(pluginId);

        return versions
            .Where(v => v.IsStable)
            .OrderByDescending(v => v.PublishedAtUtc)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the latest prerelease version of a plugin.
    /// </summary>
    /// <param name="registry">The plugin registry instance. Must not be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The latest prerelease version info, or null if none found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/> is null.</exception>
    public static async Task<PluginVersionInfo?> GetLatestPrereleaseVersionAsync(this RemotePluginRegistry registry, Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var versions = await registry.GetVersionsAsync(pluginId);

        return versions
            .Where(v => v.IsPrerelease)
            .OrderByDescending(v => v.PublishedAtUtc)
            .FirstOrDefault();
    }

    /// <summary>
    /// Downloads the latest stable version of a plugin.
    /// </summary>
    /// <param name="registry">The plugin registry instance. Must not be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="downloadPath">Directory to save the plugin file. Must not be null or whitespace.</param>
    /// <returns>Path to the downloaded plugin file, or null if failed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/> or <paramref name="downloadPath"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="downloadPath"/> is null or whitespace.</exception>
    public static async Task<string?> DownloadLatestStableAsync(this RemotePluginRegistry registry, Guid pluginId, string downloadPath)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadPath);

        var latestVersion = await registry.GetLatestStableVersionAsync(pluginId);

        return latestVersion is null
            ? null
            : await registry.DownloadPluginAsync(pluginId, latestVersion.Version, downloadPath);
    }
}