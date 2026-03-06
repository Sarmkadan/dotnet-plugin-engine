#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.ObjectModel;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Represents information about an AssemblyLoadContext used for plugin isolation.
/// </summary>
public sealed class AssemblyLoadContextInfo
{
    private List<string> _loadedAssemblies = new();

    /// <summary>
    /// Gets the unique identifier for this load context information.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the load context identifier.
    /// </summary>
    public string ContextId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin ID this context belongs to.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the context name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the context creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last activity timestamp.
    /// </summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether the context is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the total memory used by this context in bytes.
    /// </summary>
    public long MemoryUsageBytes { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of types loaded in this context.
    /// </summary>
    public int LoadedTypeCount { get; set; } = 0;

    /// <summary>
    /// Gets the collection of loaded assembly names.
    /// </summary>
    public IReadOnlyList<string> LoadedAssemblies => new ReadOnlyCollection<string>(_loadedAssemblies);

    /// <summary>
    /// Adds an assembly to the loaded collection.
    /// </summary>
    public void AddLoadedAssembly(string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
            throw new ArgumentException("Assembly name cannot be empty.", nameof(assemblyName));

        if (!_loadedAssemblies.Contains(assemblyName))
            _loadedAssemblies.Add(assemblyName);
    }

    /// <summary>
    /// Removes an assembly from the loaded collection.
    /// </summary>
    public bool RemoveLoadedAssembly(string assemblyName)
    {
        return _loadedAssemblies.Remove(assemblyName);
    }

    /// <summary>
    /// Clears all loaded assemblies.
    /// </summary>
    public void ClearLoadedAssemblies()
    {
        _loadedAssemblies.Clear();
    }

    /// <summary>
    /// Checks if an assembly is loaded in this context.
    /// </summary>
    public bool IsAssemblyLoaded(string assemblyName)
    {
        return _loadedAssemblies.Any(a => a.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Updates the activity timestamp to current UTC time.
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the total assembly count in this context.
    /// </summary>
    public int GetAssemblyCount()
    {
        return _loadedAssemblies.Count;
    }

    /// <summary>
    /// Validates the load context information.
    /// </summary>
    public bool IsValid()
    {
        return PluginId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(ContextId) &&
               !string.IsNullOrWhiteSpace(Name);
    }

    /// <summary>
    /// Gets a summary of the load context state.
    /// </summary>
    public string GetStatusSummary()
    {
        var status = IsActive ? "Active" : "Inactive";
        return $"{Name} ({status}) - {GetAssemblyCount()} assemblies, {LoadedTypeCount} types, {MemoryUsageBytes} bytes";
    }
}
