#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemVersion = System.Version;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Represents a capability or feature provided by a plugin.
/// </summary>
public sealed class PluginCapability : IEquatable<PluginCapability>
{
    /// <summary>
    /// Gets the unique identifier for this capability record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the plugin ID that provides this capability.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the capability name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the capability version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the interface type name that implements this capability.
    /// </summary>
    public string InterfaceTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the implementation type name.
    /// </summary>
    public string ImplementationTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the capability description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets tags for categorizing capabilities.
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this capability is mandatory for the plugin.
    /// </summary>
    public bool IsMandatory { get; set; } = false;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modified timestamp.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public bool Equals(PluginCapability? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) &&
               PluginId.Equals(other.PluginId) &&
               Name == other.Name &&
               Version == other.Version &&
               InterfaceTypeName == other.InterfaceTypeName &&
               ImplementationTypeName == other.ImplementationTypeName &&
               Description == other.Description &&
               Tags == other.Tags;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is PluginCapability other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, PluginId, Name, Version, InterfaceTypeName, ImplementationTypeName, Description, Tags);
    }

    public static bool operator ==(PluginCapability? left, PluginCapability? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PluginCapability? left, PluginCapability? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Gets the list of tags as an enumerable.
    /// </summary>
    public IEnumerable<string> GetTags()
    {
        if (string.IsNullOrWhiteSpace(Tags))
            return Enumerable.Empty<string>();

        return Tags.Split(',')
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }

    /// <summary>
    /// Sets tags from a collection of strings.
    /// </summary>
    public void SetTags(IEnumerable<string> tags)
    {
        if (tags is null)
        {
            Tags = string.Empty;
            return;
        }

        Tags = string.Join(",", tags.Where(t => !string.IsNullOrWhiteSpace(t)));
    }

    /// <summary>
    /// Checks if the capability has a specific tag.
    /// </summary>
    public bool HasTag(string tag)
    {
        return GetTags().Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates the capability.
    /// </summary>
    public bool IsValid()
    {
        return PluginId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Version) &&
               !string.IsNullOrWhiteSpace(InterfaceTypeName) &&
               SystemVersion.TryParse(this.Version, out _);
    }

    /// <summary>
    /// Gets a formatted display name for the capability.
    /// </summary>
    public string GetDisplayName()
    {
        return $"{Name} v{Version}";
    }
}
