#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Thread-safe in-memory registry of all known plugins.
/// Provides fast lookup by ID, name, and status, and acts as the single source
/// of truth for plugin metadata during the engine's lifetime.
/// </summary>
public sealed class PluginRegistry
{
    private readonly ILogger<PluginRegistry> _logger;
    private readonly ConcurrentDictionary<Guid, Plugin> _plugins = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginRegistry"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public PluginRegistry(ILogger<PluginRegistry> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Registers a plugin. If a plugin with the same ID already exists, it is replaced.
    /// </summary>
    /// <param name="plugin">The plugin to register.</param>
    /// <returns><c>true</c> if newly added; <c>false</c> if it replaced an existing entry.</returns>
    public bool Register(Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        var isNew = _plugins.TryAdd(plugin.Id, plugin);
        if (!isNew)
        {
            _plugins[plugin.Id] = plugin;
            _logger.LogInformation("Plugin {Name} ({Id}) re-registered.", plugin.Name, plugin.Id);
        }
        else
        {
            _logger.LogInformation("Plugin {Name} ({Id}) registered.", plugin.Name, plugin.Id);
        }

        return isNew;
    }

    /// <summary>
    /// Removes a plugin from the registry.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin to remove.</param>
    /// <returns><c>true</c> if the plugin was found and removed; otherwise <c>false</c>.</returns>
    public bool Unregister(Guid pluginId)
    {
        var removed = _plugins.TryRemove(pluginId, out var plugin);
        if (removed)
        {
            _logger.LogInformation("Plugin {Name} ({Id}) unregistered.", plugin!.Name, pluginId);
        }

        return removed;
    }

    /// <summary>
    /// Retrieves a plugin by its unique identifier.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The <see cref="Plugin"/> if found; otherwise <c>null</c>.</returns>
    public Plugin? GetById(Guid pluginId)
    {
        _plugins.TryGetValue(pluginId, out var plugin);
        return plugin;
    }

    /// <summary>
    /// Retrieves the first plugin matching the given name (case-insensitive).
    /// </summary>
    /// <param name="name">The plugin name to search for.</param>
    /// <returns>The <see cref="Plugin"/> if found; otherwise <c>null</c>.</returns>
    public Plugin? GetByName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _plugins.Values.FirstOrDefault(
            p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns all plugins that currently have the specified status.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <returns>An enumerable of matching plugins.</returns>
    public IEnumerable<Plugin> GetByStatus(PluginStatus status)
    {
        return _plugins.Values.Where(p => p.Status == status);
    }

    /// <summary>
    /// Returns a snapshot of all registered plugins.
    /// </summary>
    /// <returns>A read-only list of all plugins.</returns>
    public IReadOnlyList<Plugin> GetAll()
    {
        return _plugins.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Returns the total number of registered plugins.
    /// </summary>
    public int Count => _plugins.Count;
}
