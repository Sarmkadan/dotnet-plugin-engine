#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.ObjectModel;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Represents metadata about a plugin without loading the full assembly.
/// </summary>
public sealed class PluginMetadata
{
    private Dictionary<string, string> _customProperties = new();

    /// <summary>
    /// Gets the unique identifier for this metadata record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the plugin ID this metadata belongs to.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the plugin name.
    /// </summary>
    public string PluginName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin version.
    /// </summary>
    public string PluginVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly name.
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly version.
    /// </summary>
    public string AssemblyVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target framework.
    /// </summary>
    public string TargetFramework { get; set; } = "net10.0";

    /// <summary>
    /// Gets or sets the plugin author.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the company or organization that created the plugin.
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin license.
    /// </summary>
    public string License { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin repository URL.
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum required CLR version.
    /// </summary>
    public string MinimumClrVersion { get; set; } = "10.0";

    /// <summary>
    /// Gets or sets whether the plugin is signed.
    /// </summary>
    public bool IsSigned { get; set; } = false;

    /// <summary>
    /// Gets or sets the public key token.
    /// </summary>
    public string PublicKeyToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the collection of custom metadata properties.
    /// </summary>
    public IReadOnlyDictionary<string, string> CustomProperties => new ReadOnlyDictionary<string, string>(_customProperties);

    /// <summary>
    /// Adds or updates a custom metadata property.
    /// </summary>
    public void SetCustomProperty(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Property key cannot be empty.", nameof(key));

        _customProperties[key] = value ?? string.Empty;
    }

    /// <summary>
    /// Gets a custom metadata property value.
    /// </summary>
    public string? GetCustomProperty(string key)
    {
        return _customProperties.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Removes a custom metadata property.
    /// </summary>
    public bool RemoveCustomProperty(string key)
    {
        return _customProperties.Remove(key);
    }

    /// <summary>
    /// Clears all custom metadata properties.
    /// </summary>
    public void ClearCustomProperties()
    {
        _customProperties.Clear();
    }

    /// <summary>
    /// Gets a summary of the metadata.
    /// </summary>
    public string GetSummary()
    {
        return $"{PluginName} v{PluginVersion} ({AssemblyName} v{AssemblyVersion}) - {Description}";
    }

    /// <summary>
    /// Validates the metadata.
    /// </summary>
    public bool IsValid()
    {
        return PluginId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(PluginName) &&
               !string.IsNullOrWhiteSpace(PluginVersion) &&
               !string.IsNullOrWhiteSpace(AssemblyName) &&
               !string.IsNullOrWhiteSpace(TargetFramework);
    }
}
