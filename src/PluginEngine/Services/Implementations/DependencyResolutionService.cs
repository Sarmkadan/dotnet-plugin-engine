#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;
using PluginEngine.Exceptions;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Service implementation for resolving plugin dependencies.
/// </summary>
public sealed class DependencyResolutionService : IDependencyResolutionService
{
    private readonly IPluginLoaderService _pluginLoaderService;
    private readonly Dictionary<Guid, List<Plugin>> _dependencyCache = new();

    public DependencyResolutionService(IPluginLoaderService pluginLoaderService)
    {
        _pluginLoaderService = pluginLoaderService ?? throw new ArgumentNullException(nameof(pluginLoaderService));
    }

    /// <summary>
    /// Resolves all dependencies for a plugin.
    /// </summary>
    public async Task<IEnumerable<Plugin>> ResolveDependenciesAsync(Plugin plugin, CancellationToken cancellationToken = default)
    {
        if (plugin is null)
            throw new ArgumentNullException(nameof(plugin));

        if (_dependencyCache.TryGetValue(plugin.Id, out var cached))
            return cached;

        var resolvedDependencies = new List<Plugin>();
        var visited = new HashSet<Guid>();

        await ResolveDependenciesRecursiveAsync(plugin, resolvedDependencies, visited, cancellationToken);

        _dependencyCache[plugin.Id] = resolvedDependencies;
        return resolvedDependencies;
    }

    /// <summary>
    /// Validates that all dependencies for a plugin are satisfied.
    /// </summary>
    public async Task<bool> ValidateDependenciesAsync(Plugin plugin, CancellationToken cancellationToken = default)
    {
        if (plugin is null)
            throw new ArgumentNullException(nameof(plugin));

        foreach (var dependency in plugin.Dependencies)
        {
            var resolved = await ResolveSingleDependencyAsync(dependency, cancellationToken);
            if (resolved is null && !dependency.IsOptional)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks for circular dependencies.
    /// </summary>
    public async Task<bool> HasCircularDependenciesAsync(Plugin plugin, CancellationToken cancellationToken = default)
    {
        if (plugin is null)
            throw new ArgumentNullException(nameof(plugin));

        var visited = new HashSet<Guid>();
        var recursionStack = new HashSet<Guid>();

        return await HasCircularDependenciesRecursiveAsync(plugin.Id, visited, recursionStack, cancellationToken);
    }

    /// <summary>
    /// Gets the dependency graph for a plugin.
    /// </summary>
    public async Task<DependencyGraph> GetDependencyGraphAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        var plugin = await _pluginLoaderService.GetLoadedPluginAsync(pluginId, cancellationToken);
        if (plugin is null)
            throw new PluginException($"Plugin {pluginId} not found.", "PLUGIN_NOT_FOUND");

        var graph = new DependencyGraph { RootPluginId = pluginId };
        var nodes = new Dictionary<Guid, DependencyNode>();
        var edges = new List<DependencyEdge>();

        await BuildDependencyGraphAsync(plugin, graph, nodes, edges, 0, cancellationToken);

        return graph;
    }

    /// <summary>
    /// Resolves a single dependency.
    /// </summary>
    public async Task<Plugin?> ResolveSingleDependencyAsync(PluginDependency dependency, CancellationToken cancellationToken = default)
    {
        if (dependency is null)
            throw new ArgumentNullException(nameof(dependency));

        var loadedPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync(cancellationToken);
        var resolved = loadedPlugins.FirstOrDefault(p => p.Id == dependency.DependencyPluginId);

        if (resolved is null)
            return null;

        if (!dependency.IsSatisfiedBy(resolved.Version))
            return null;

        return resolved;
    }

    /// <summary>
    /// Gets all plugins that depend on a specific plugin.
    /// </summary>
    public async Task<IEnumerable<Plugin>> GetDependentsAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        var allPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync(cancellationToken);
        var dependents = new List<Plugin>();

        foreach (var plugin in allPlugins)
        {
            if (plugin.Dependencies.Any(d => d.DependencyPluginId == pluginId))
                dependents.Add(plugin);
        }

        return dependents;
    }

    /// <summary>
    /// Clears the dependency cache.
    /// </summary>
    public Task ClearDependencyCacheAsync()
    {
        _dependencyCache.Clear();
        return Task.CompletedTask;
    }

    private async Task ResolveDependenciesRecursiveAsync(Plugin plugin, List<Plugin> resolved, HashSet<Guid> visited, CancellationToken cancellationToken)
    {
        if (visited.Contains(plugin.Id))
            return;

        visited.Add(plugin.Id);

        foreach (var dependency in plugin.Dependencies)
        {
            var resolvedDep = await ResolveSingleDependencyAsync(dependency, cancellationToken);
            if (resolvedDep is not null && !resolved.Any(p => p.Id == resolvedDep.Id))
            {
                resolved.Add(resolvedDep);
                await ResolveDependenciesRecursiveAsync(resolvedDep, resolved, visited, cancellationToken);
            }
        }
    }

    private async Task<bool> HasCircularDependenciesRecursiveAsync(Guid pluginId, HashSet<Guid> visited, HashSet<Guid> recursionStack, CancellationToken cancellationToken)
    {
        visited.Add(pluginId);
        recursionStack.Add(pluginId);

        var plugin = await _pluginLoaderService.GetLoadedPluginAsync(pluginId, cancellationToken);
        if (plugin is null)
            return false;

        foreach (var dep in plugin.Dependencies)
        {
            if (!visited.Contains(dep.DependencyPluginId))
            {
                if (await HasCircularDependenciesRecursiveAsync(dep.DependencyPluginId, visited, recursionStack, cancellationToken))
                    return true;
            }
            else if (recursionStack.Contains(dep.DependencyPluginId))
                return true;
        }

        recursionStack.Remove(pluginId);
        return false;
    }

    private async Task BuildDependencyGraphAsync(Plugin plugin, DependencyGraph graph, Dictionary<Guid, DependencyNode> nodes, List<DependencyEdge> edges, int level, CancellationToken cancellationToken)
    {
        if (!nodes.ContainsKey(plugin.Id))
        {
            var node = new DependencyNode
            {
                PluginId = plugin.Id,
                PluginName = plugin.Name,
                Version = plugin.Version,
                Level = level
            };
            nodes[plugin.Id] = node;
            graph.Nodes.Add(node);
        }

        foreach (var dep in plugin.Dependencies)
        {
            var resolvedDep = await ResolveSingleDependencyAsync(dep, cancellationToken);
            if (resolvedDep is not null)
            {
                var edge = new DependencyEdge
                {
                    FromPluginId = plugin.Id,
                    ToPluginId = dep.DependencyPluginId,
                    VersionConstraint = dep.GetVersionConstraint(),
                    IsOptional = dep.IsOptional
                };
                edges.Add(edge);
                graph.Edges.Add(edge);

                await BuildDependencyGraphAsync(resolvedDep, graph, nodes, edges, level + 1, cancellationToken);
            }
        }
    }
}
