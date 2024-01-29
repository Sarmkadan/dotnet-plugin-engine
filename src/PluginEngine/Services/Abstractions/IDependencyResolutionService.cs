// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Services.Abstractions;

/// <summary>
/// Service interface for resolving plugin dependencies.
/// </summary>
public interface IDependencyResolutionService
{
    /// <summary>
    /// Resolves all dependencies for a plugin.
    /// </summary>
    Task<IEnumerable<Plugin>> ResolveDependenciesAsync(Plugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that all dependencies for a plugin are satisfied.
    /// </summary>
    Task<bool> ValidateDependenciesAsync(Plugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks for circular dependencies.
    /// </summary>
    Task<bool> HasCircularDependenciesAsync(Plugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the dependency graph for a plugin.
    /// </summary>
    Task<DependencyGraph> GetDependencyGraphAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a single dependency.
    /// </summary>
    Task<Plugin?> ResolveSingleDependencyAsync(PluginDependency dependency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all plugins that depend on a specific plugin.
    /// </summary>
    Task<IEnumerable<Plugin>> GetDependentsAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the dependency cache.
    /// </summary>
    Task ClearDependencyCacheAsync();
}

/// <summary>
/// Represents a dependency graph for a plugin.
/// </summary>
public class DependencyGraph
{
    public Guid RootPluginId { get; set; }
    public List<DependencyNode> Nodes { get; set; } = new();
    public List<DependencyEdge> Edges { get; set; } = new();
}

/// <summary>
/// Represents a node in the dependency graph.
/// </summary>
public class DependencyNode
{
    public Guid PluginId { get; set; }
    public string PluginName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int Level { get; set; }
}

/// <summary>
/// Represents an edge in the dependency graph.
/// </summary>
public class DependencyEdge
{
    public Guid FromPluginId { get; set; }
    public Guid ToPluginId { get; set; }
    public string VersionConstraint { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}
