#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Provides extension methods for the <see cref="Plugin"/> class to enhance plugin management functionality.
/// </summary>
public static class PluginExtensions
{
    /// <summary>
    /// Determines whether the plugin is in an active state.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns><see langword="true"/> if the plugin status is Active; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static bool IsActive(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return plugin.Status == PluginStatus.Active;
    }

    /// <summary>
    /// Determines whether the plugin is in a failed state.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns><see langword="true"/> if the plugin status is Failed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static bool IsFailed(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return plugin.Status == PluginStatus.Failed;
    }

    /// <summary>
    /// Gets the full display name of the plugin including version and author.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <returns>A formatted string containing the plugin name, version, and author.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static string GetDisplayName(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        return string.IsNullOrWhiteSpace(plugin.Author)
            ? $"{plugin.Name} v{plugin.Version}"
            : $"{plugin.Name} v{plugin.Version} by {plugin.Author}";
    }

    /// <summary>
    /// Gets the formatted creation date of the plugin using invariant culture.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <returns>A formatted date string in ISO 8601 format (YYYY-MM-DD).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static string GetFormattedCreationDate(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        return plugin.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the formatted modification date of the plugin using invariant culture.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <returns>A formatted date string in ISO 8601 format (YYYY-MM-DD).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static string GetFormattedModificationDate(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        return plugin.ModifiedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Determines whether the plugin has any dependencies.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns><see langword="true"/> if the plugin has one or more dependencies; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static bool HasDependencies(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return plugin.Dependencies.Count > 0;
    }

    /// <summary>
    /// Determines whether the plugin has any capabilities.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns><see langword="true"/> if the plugin has one or more capabilities; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static bool HasCapabilities(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return plugin.Capabilities.Count > 0;
    }

    /// <summary>
    /// Gets a summary of the plugin's metadata including description and version.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <returns>A formatted summary string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static string GetMetadataSummary(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        var summary = new System.Text.StringBuilder();
        summary.Append($"{plugin.GetDisplayName()}");

        if (!string.IsNullOrWhiteSpace(plugin.Description))
        {
            summary.Append($" - {plugin.Description}");
        }

        return summary.ToString();
    }

    /// <summary>
    /// Updates the ModifiedAt timestamp to the current UTC time.
    /// </summary>
    /// <param name="plugin">The plugin to update.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static void Touch(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        plugin.ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the plugin is in a loading or unloading state.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns><see langword="true"/> if the plugin status is Loading or Unloading; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static bool IsTransitioning(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return plugin.Status is PluginStatus.Loading or PluginStatus.Unloading;
    }

    /// <summary>
    /// Gets the plugin's age in days since creation.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <returns>The number of days since the plugin was created.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static int GetAgeInDays(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        var age = DateTime.UtcNow - plugin.CreatedAt;
        return (int)age.TotalDays;
    }
}