#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Data.Repositories;

/// <summary>
/// Repository implementation for plugin data operations using in-memory storage.
/// </summary>
public sealed class PluginRepository : IPluginRepository
{
    private readonly Dictionary<Guid, Plugin> _plugins = new();
    private readonly Dictionary<Guid, List<PluginDependency>> _dependencies = new();
    private readonly Dictionary<Guid, List<PluginCapability>> _capabilities = new();
    private readonly object _lockObject = new object();

    /// <summary>
    /// Adds a new plugin to the repository.
    /// </summary>
    public Task<Plugin> AddAsync(Plugin plugin, CancellationToken cancellationToken = default)
    {
        if (plugin is null)
            throw new ArgumentNullException(nameof(plugin));

        if (plugin.Id == Guid.Empty)
            plugin.Id = Guid.NewGuid();

        lock (_lockObject)
        {
            _plugins[plugin.Id] = plugin;
            _dependencies[plugin.Id] = new List<PluginDependency>();
            _capabilities[plugin.Id] = new List<PluginCapability>();
        }

        return Task.FromResult(plugin);
    }

    /// <summary>
    /// Updates an existing plugin.
    /// </summary>
    public Task<bool> UpdateAsync(Plugin plugin, CancellationToken cancellationToken = default)
    {
        if (plugin is null)
            throw new ArgumentNullException(nameof(plugin));

        lock (_lockObject)
        {
            if (!_plugins.ContainsKey(plugin.Id))
                return Task.FromResult(false);

            plugin.ModifiedAt = DateTime.UtcNow;
            _plugins[plugin.Id] = plugin;
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Deletes a plugin by ID.
    /// </summary>
    public Task<bool> DeleteAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (!_plugins.Remove(pluginId))
                return Task.FromResult(false);

            _dependencies.Remove(pluginId);
            _capabilities.Remove(pluginId);
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Gets a plugin by ID.
    /// </summary>
    public Task<Plugin?> GetByIdAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_plugins.TryGetValue(pluginId, out var plugin) ? plugin : null);
        }
    }

    /// <summary>
    /// Gets a plugin by name.
    /// </summary>
    public Task<Plugin?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult<Plugin?>(null);

        lock (_lockObject)
        {
            var plugin = _plugins.Values.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(plugin);
        }
    }

    /// <summary>
    /// Gets all plugins.
    /// </summary>
    public Task<IEnumerable<Plugin>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_plugins.Values.AsEnumerable());
        }
    }

    /// <summary>
    /// Gets plugins by status.
    /// </summary>
    public Task<IEnumerable<Plugin>> GetByStatusAsync(PluginStatus status, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _plugins.Values.Where(p => p.Status == status).ToList();
            return Task.FromResult<IEnumerable<Plugin>>(results);
        }
    }

    /// <summary>
    /// Checks if a plugin exists.
    /// </summary>
    public Task<bool> ExistsAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_plugins.ContainsKey(pluginId));
        }
    }

    /// <summary>
    /// Gets the count of all plugins.
    /// </summary>
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_plugins.Count);
        }
    }

    /// <summary>
    /// Searches for plugins based on criteria.
    /// </summary>
    public Task<IEnumerable<Plugin>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return GetAllAsync(cancellationToken);

        lock (_lockObject)
        {
            var results = _plugins.Values.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            return Task.FromResult<IEnumerable<Plugin>>(results);
        }
    }

    /// <summary>
    /// Adds a dependency to a plugin.
    /// </summary>
    public Task<bool> AddDependencyAsync(Guid pluginId, PluginDependency dependency, CancellationToken cancellationToken = default)
    {
        if (dependency is null)
            throw new ArgumentNullException(nameof(dependency));

        lock (_lockObject)
        {
            if (!_dependencies.ContainsKey(pluginId))
                _dependencies[pluginId] = new List<PluginDependency>();

            _dependencies[pluginId].Add(dependency);
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Removes a dependency from a plugin.
    /// </summary>
    public Task<bool> RemoveDependencyAsync(Guid pluginId, Guid dependencyId, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (!_dependencies.TryGetValue(pluginId, out var deps))
                return Task.FromResult(false);

            var dependency = deps.FirstOrDefault(d => d.Id == dependencyId);
            if (dependency is null)
                return Task.FromResult(false);

            deps.Remove(dependency);
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Gets all dependencies for a plugin.
    /// </summary>
    public Task<IEnumerable<PluginDependency>> GetDependenciesAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (!_dependencies.TryGetValue(pluginId, out var deps))
                return Task.FromResult(Enumerable.Empty<PluginDependency>());

            return Task.FromResult<IEnumerable<PluginDependency>>(new List<PluginDependency>(deps));
        }
    }

    /// <summary>
    /// Adds a capability to a plugin.
    /// </summary>
    public Task<bool> AddCapabilityAsync(Guid pluginId, PluginCapability capability, CancellationToken cancellationToken = default)
    {
        if (capability is null)
            throw new ArgumentNullException(nameof(capability));

        lock (_lockObject)
        {
            if (!_capabilities.ContainsKey(pluginId))
                _capabilities[pluginId] = new List<PluginCapability>();

            _capabilities[pluginId].Add(capability);
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Gets all capabilities for a plugin.
    /// </summary>
    public Task<IEnumerable<PluginCapability>> GetCapabilitiesAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (!_capabilities.TryGetValue(pluginId, out var caps))
                return Task.FromResult(Enumerable.Empty<PluginCapability>());

            return Task.FromResult<IEnumerable<PluginCapability>>(new List<PluginCapability>(caps));
        }
    }
}
