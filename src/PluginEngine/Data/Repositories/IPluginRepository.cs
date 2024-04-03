#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Data.Repositories;

/// <summary>
/// Repository interface for plugin data operations.
/// </summary>
public interface IPluginRepository
{
    /// <summary>
    /// Adds a new plugin to the repository.
    /// </summary>
    Task<Plugin> AddAsync(Plugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing plugin.
    /// </summary>
    Task<bool> UpdateAsync(Plugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a plugin by ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a plugin by ID.
    /// </summary>
    Task<Plugin?> GetByIdAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a plugin by name.
    /// </summary>
    Task<Plugin?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all plugins.
    /// </summary>
    Task<IEnumerable<Plugin>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plugins by status.
    /// </summary>
    Task<IEnumerable<Plugin>> GetByStatusAsync(PluginStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a plugin exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of all plugins.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for plugins based on criteria.
    /// </summary>
    Task<IEnumerable<Plugin>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a dependency to a plugin.
    /// </summary>
    Task<bool> AddDependencyAsync(Guid pluginId, PluginDependency dependency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a dependency from a plugin.
    /// </summary>
    Task<bool> RemoveDependencyAsync(Guid pluginId, Guid dependencyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all dependencies for a plugin.
    /// </summary>
    Task<IEnumerable<PluginDependency>> GetDependenciesAsync(Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a capability to a plugin.
    /// </summary>
    Task<bool> AddCapabilityAsync(Guid pluginId, PluginCapability capability, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all capabilities for a plugin.
    /// </summary>
    Task<IEnumerable<PluginCapability>> GetCapabilitiesAsync(Guid pluginId, CancellationToken cancellationToken = default);
}
