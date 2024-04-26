#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Data.Repositories;

/// <summary>
/// Extension methods for PluginRepository providing additional convenience operations.
/// </summary>
public static class PluginRepositoryExtensions
{
    /// <summary>
    /// Gets a plugin by name asynchronously.
    /// </summary>
    public static async Task<Plugin?> GetByNameAsync(this IPluginRepository repository, string name, bool ignoreCase = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

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
    public static async Task<IEnumerable<Plugin>> GetByStatusAsync(this IPluginRepository repository, PluginStatus status, CancellationToken cancellationToken = default)
    {
        return await repository.GetByStatusAsync(status, cancellationToken).ConfigureAwait(false) ?? Enumerable.Empty<Plugin>();
    }

    /// <summary>
    /// Gets all plugins with any of the specified statuses.
    /// </summary>
    public static async Task<IEnumerable<Plugin>> GetByStatusesAsync(this IPluginRepository repository, IEnumerable<PluginStatus> statuses, CancellationToken cancellationToken = default)
    {
        if (statuses == null)
            throw new ArgumentNullException(nameof(statuses));

        var statusArray = statuses.ToArray();
        if (statusArray.Length == 0)
            return await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var allPlugins = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return allPlugins.Where(p => statusArray.Contains(p.Status));
    }

    /// <summary>
    /// Gets all plugins that depend on a specific plugin.
    /// </summary>
    public static async Task<IEnumerable<Plugin>> GetDependentPluginsAsync(this IPluginRepository repository, Guid pluginId, CancellationToken cancellationToken = default)
    {
        if (pluginId == Guid.Empty)
            return Enumerable.Empty<Plugin>();

        var allPlugins = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var results = new List<Plugin>();

        foreach (var plugin in allPlugins)
        {
            var dependencies = await repository.GetDependenciesAsync(plugin.Id, cancellationToken).ConfigureAwait(false);
            if (dependencies.Any(d => d.DependencyPluginId == pluginId))
            {
                results.Add(plugin);
            }
        }

        return results;
    }
}
