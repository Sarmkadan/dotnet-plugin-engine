#nullable enable
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
    /// <param name="plugin">The plugin to resolve dependencies for.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumerable of all resolved <see cref="Plugin"/> dependencies.</returns>
    Task<IEnumerable<Plugin>> ResolveDependenciesAsync(Plugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that all dependencies for a plugin are satisfied.
    /// </summary>
    /// <param name="plugin">The plugin to validate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing <c>true</c> if valid, otherwise <c>false</c>.</returns>
    Task<bool> ValidateDependenciesAsync(Plugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks for circular dependencies.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing <c>true</c> if circular dependencies exist, otherwise <c>false</c>.</returns>
    Task<bool> HasCircularDependenciesAsync(Plugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the dependency graph for a plugin.
    /// </summary>
    /// <param name="pluginId">The unique ID of the plugin.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="DependencyGraph"/>.</returns>
    Task<DependencyGraph> GetDependencyGraphAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a single dependency.
    /// </summary>
    /// <param name="dependency">The dependency to resolve.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the resolved <see cref="Plugin"/> if found, otherwise <c>null</c>.</returns>
    Task<Plugin?> ResolveSingleDependencyAsync(PluginDependency dependency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all plugins that depend on a specific plugin.
    /// </summary>
    /// <param name="pluginId">The unique ID of the plugin.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumerable of dependent <see cref="Plugin"/> instances.</returns>
    Task<IEnumerable<Plugin>> GetDependentsAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the dependency cache.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearDependencyCacheAsync();
}

/// <summary>
/// Represents a dependency graph for a plugin.
/// </summary>
public sealed class DependencyGraph
{
    public Guid RootPluginId { get; set; }
    public List<DependencyNode> Nodes { get; set; } = new();
    public List<DependencyEdge> Edges { get; set; } = new();
}

/// <summary>
/// Represents a node in the dependency graph.
/// </summary>
public sealed class DependencyNode
{
    public Guid PluginId { get; set; }
    public string PluginName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int Level { get; set; }
}

/// <summary>
/// Represents an edge in the dependency graph.
/// </summary>
public sealed class DependencyEdge
{
    public Guid FromPluginId { get; set; }
    public Guid ToPluginId { get; set; }
    public string VersionConstraint { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}
