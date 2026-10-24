#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Resolves plugin dependency graphs using topological sorting,
/// detects circular references, and validates version constraints.
/// </summary>
public sealed class DependencyResolver
{
    private readonly ILogger<DependencyResolver> _logger;
    private readonly IDependencyResolutionService _resolutionService;
    private readonly IPluginLoaderService _loaderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyResolver"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="resolutionService">The underlying dependency resolution service.</param>
    /// <param name="loaderService">The plugin loader service for querying loaded plugins.</param>
    public DependencyResolver(
        ILogger<DependencyResolver> logger,
        IDependencyResolutionService resolutionService,
        IPluginLoaderService loaderService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(resolutionService);
        ArgumentNullException.ThrowIfNull(loaderService);
        _logger = logger;
        _resolutionService = resolutionService;
        _loaderService = loaderService;
    }

    /// <summary>
    /// Computes a topologically sorted load order for the given set of plugins
    /// so that each plugin's dependencies are loaded before the plugin itself.
    /// </summary>
    /// <param name="plugins">Plugins to sort.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of plugins in dependency-first order.</returns>
    /// <exception cref="DependencyResolutionException">Thrown when a circular dependency is detected.</exception>
    public async Task<List<Plugin>> ComputeLoadOrderAsync(
        IEnumerable<Plugin> plugins, CancellationToken cancellationToken = default)
    {
        var pluginList = plugins.ToList();
        _logger.LogInformation("Computing load order for {Count} plugins.", pluginList.Count);

        var lookup = pluginList.ToDictionary(p => p.Id);
        var sorted = new List<Plugin>();
        var visited = new HashSet<Guid>();
        var visiting = new HashSet<Guid>();

        foreach (var plugin in pluginList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!visited.Contains(plugin.Id))
            {
                Visit(plugin, lookup, visited, visiting, sorted);
            }
        }

        _logger.LogInformation("Load order computed: {Order}.",
            string.Join(" -> ", sorted.Select(p => p.Name)));
        return sorted;
    }

    /// <summary>
    /// Validates that every declared dependency of <paramref name="plugin"/> can be satisfied
    /// by plugins currently loaded in <paramref name="availablePlugins"/>.
    /// </summary>
    /// <param name="plugin">The plugin whose dependencies to validate.</param>
    /// <param name="availablePlugins">The set of available plugins.</param>
    /// <returns>A list of missing dependency names. Empty if all are satisfied.</returns>
    public List<string> FindMissingDependencies(Plugin plugin, IEnumerable<Plugin> availablePlugins)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        var available = availablePlugins.ToDictionary(p => p.Id);
        var missing = new List<string>();

        foreach (var dep in plugin.Dependencies)
        {
            if (!available.ContainsKey(dep.DependencyPluginId))
            {
                missing.Add(dep.Description);
                _logger.LogWarning("Plugin {Plugin} has unsatisfied dependency: {Dep}.", plugin.Name, dep.DependencyPluginId);
            }
        }

        return missing;
    }

    /// <summary>
    /// Checks whether adding <paramref name="plugin"/> to the current set would introduce
    /// a circular dependency.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if a cycle is detected; otherwise <c>false</c>.</returns>
    public async Task<bool> WouldCreateCycleAsync(Plugin plugin, CancellationToken cancellationToken = default)
    {
        return await _resolutionService.HasCircularDependenciesAsync(plugin, cancellationToken);
    }

    /// <summary>
    /// Retrieves all plugins that transitively depend on the specified plugin,
    /// useful for determining the blast radius of an unload or upgrade.
    /// </summary>
    /// <param name="pluginId">The plugin to find dependents of.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All transitive dependents.</returns>
    public async Task<IReadOnlyList<Plugin>> GetTransitiveDependentsAsync(
        Guid pluginId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Resolving transitive dependents for plugin {PluginId}.", pluginId);
        var directDependents = await _resolutionService.GetDependentsAsync(pluginId, cancellationToken);
        var all = new Dictionary<Guid, Plugin>();

        foreach (var dep in directDependents)
        {
            if (all.TryAdd(dep.Id, dep))
            {
                var nested = await GetTransitiveDependentsAsync(dep.Id, cancellationToken);
                foreach (var n in nested)
                {
                    all.TryAdd(n.Id, n);
                }
            }
        }

        return all.Values.ToList().AsReadOnly();
    }

    private static void Visit(
        Plugin plugin,
        Dictionary<Guid, Plugin> lookup,
        HashSet<Guid> visited,
        HashSet<Guid> visiting,
        List<Plugin> sorted)
    {
        if (visited.Contains(plugin.Id))
        {
            return;
        }

        if (!visiting.Add(plugin.Id))
        {
            throw new DependencyResolutionException($"Circular dependency detected involving plugin '{plugin.Name}'.");
        }

        foreach (var dep in plugin.Dependencies)
        {
            if (lookup.TryGetValue(dep.DependencyPluginId, out var depPlugin))
            {
                Visit(depPlugin, lookup, visited, visiting, sorted);
            }
        }

        visiting.Remove(plugin.Id);
        visited.Add(plugin.Id);
        sorted.Add(plugin);
    }
}
