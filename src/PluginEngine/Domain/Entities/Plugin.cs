// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.ObjectModel;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Represents a plugin entity in the plugin engine system.
/// </summary>
public class Plugin
{
    private List<PluginDependency> _dependencies = new();
    private List<PluginCapability> _capabilities = new();

    /// <summary>
    /// Gets the unique identifier for the plugin.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the plugin name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly path.
    /// </summary>
    public string AssemblyPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin status.
    /// </summary>
    public PluginStatus Status { get; set; } = PluginStatus.Unloaded;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modified timestamp.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether the plugin supports hot-reload.
    /// </summary>
    public bool SupportsHotReload { get; set; } = true;

    /// <summary>
    /// Gets or sets the AssemblyLoadContext identifier.
    /// </summary>
    public string LoadContextId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the collection of plugin dependencies.
    /// </summary>
    public IReadOnlyList<PluginDependency> Dependencies => new ReadOnlyCollection<PluginDependency>(_dependencies);

    /// <summary>
    /// Gets the collection of plugin capabilities.
    /// </summary>
    public IReadOnlyList<PluginCapability> Capabilities => new ReadOnlyCollection<PluginCapability>(_capabilities);

    /// <summary>
    /// Adds a dependency to the plugin.
    /// </summary>
    public void AddDependency(PluginDependency dependency)
    {
        if (dependency == null)
            throw new ArgumentNullException(nameof(dependency));

        if (!_dependencies.Any(d => d.PluginId == dependency.PluginId))
            _dependencies.Add(dependency);
    }

    /// <summary>
    /// Removes a dependency from the plugin.
    /// </summary>
    public bool RemoveDependency(Guid dependencyId)
    {
        var dependency = _dependencies.FirstOrDefault(d => d.Id == dependencyId);
        if (dependency != null)
        {
            _dependencies.Remove(dependency);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a capability to the plugin.
    /// </summary>
    public void AddCapability(PluginCapability capability)
    {
        if (capability == null)
            throw new ArgumentNullException(nameof(capability));

        if (!_capabilities.Any(c => c.Name == capability.Name))
            _capabilities.Add(capability);
    }

    /// <summary>
    /// Validates the plugin entity.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Version) &&
               !string.IsNullOrWhiteSpace(AssemblyPath) &&
               Id != Guid.Empty;
    }

    /// <summary>
    /// Gets a validation error message if the plugin is invalid.
    /// </summary>
    public string GetValidationError()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return "Plugin name is required.";
        if (string.IsNullOrWhiteSpace(Version))
            return "Plugin version is required.";
        if (string.IsNullOrWhiteSpace(AssemblyPath))
            return "Assembly path is required.";
        if (Id == Guid.Empty)
            return "Plugin ID cannot be empty.";

        return string.Empty;
    }
}

/// <summary>
/// Represents the status of a plugin.
/// </summary>
public enum PluginStatus
{
    Unloaded = 0,
    Loading = 1,
    Loaded = 2,
    Active = 3,
    Inactive = 4,
    Failed = 5,
    Unloading = 6
}
