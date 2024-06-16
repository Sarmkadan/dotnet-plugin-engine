#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Data.Repositories;

/// <summary>
/// Extension methods for <see cref="IPluginRepository"/> providing additional convenience operations.
/// </summary>
public static class PluginRepositoryExtensions
{
    /// <summary>
    /// Gets a plugin by name asynchronously.
    /// </summary>
    /// <param name="repository">The plugin repository.</param>
    /// <param name="name">The name of the plugin to find.</param>
    /// <param name="ignoreCase">Whether to perform case-insensitive search if exact match is not found.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The plugin with the specified name, or null if not found.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    public static async Task<Plugin?> GetByNameAsync(this IPluginRepository repository, string name, bool ignoreCase = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var plugin = await repository.GetByNameAsync(name, cancellationToken).ConfigureAwait(false);

        if (plugin != null || !ignoreCase)
            return plugin;

        // If not found with exact case match and ignoreCase is true, try case-insensitive search
        var allPlugins = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return allPlugins.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all plugins with the specified status.
    /// </summary>
    /// <param name="repository">The plugin repository.</param>
    /// <param name="status">The plugin status to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An enumerable of plugins with the specified status.</returns>
    /// <exception cref="ArgumentException"><paramref name="status"/> is not a valid enum value.</exception>
    public static async Task<IEnumerable<Plugin>> GetByStatusAsync(this IPluginRepository repository, PluginStatus status, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(status), $"The value {status} is not defined in enum {nameof(PluginStatus)}.");
        return await repository.GetByStatusAsync(status, cancellationToken).ConfigureAwait(false) ?? Enumerable.Empty<Plugin>();
    }

    /// <summary>
    /// Gets all plugins with any of the specified statuses.
    /// </summary>
    /// <param name="repository">The plugin repository.</param>
    /// <param name="statuses">The plugin statuses to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An enumerable of plugins with any of the specified statuses.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="statuses"/> is null.</exception>
    public static async Task<IEnumerable<Plugin>> GetByStatusesAsync(this IPluginRepository repository, IEnumerable<PluginStatus> statuses, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(statuses);

        var statusArray = statuses.ToArray();
        if (statusArray.Length == 0)
            return await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var allPlugins = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return allPlugins.Where(p => statusArray.Contains(p.Status));
    }

    /// <summary>
    /// Gets all plugins that depend on a specific plugin.
    /// </summary>
    /// <param name="repository">The plugin repository.</param>
    /// <param name="pluginId">The ID of the plugin to find dependents for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An enumerable of plugins that depend on the specified plugin.</returns>
    /// <exception cref="ArgumentException"><paramref name="pluginId"/> is empty.</exception>
    public static async Task<IEnumerable<Plugin>> GetDependentPluginsAsync(this IPluginRepository repository, Guid pluginId, CancellationToken cancellationToken = default)
    {
        if (pluginId == Guid.Empty)
            throw new ArgumentException("Plugin ID cannot be empty.", nameof(pluginId));

        var allPlugins = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var dependentPlugins = new List<Plugin>();

        await foreach (var plugin in allPlugins.ToAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var dependencies = await repository.GetDependenciesAsync(plugin.Id, cancellationToken).ConfigureAwait(false);
            if (dependencies?.Any(d => d.DependencyPluginId == pluginId) == true)
            {
                dependentPlugins.Add(plugin);
            }
        }

        return dependentPlugins;
    }
}
