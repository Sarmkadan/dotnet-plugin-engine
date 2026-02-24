// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Represents a dependency relationship between plugins.
/// </summary>
public class PluginDependency
{
    /// <summary>
    /// Gets the unique identifier for this dependency record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the plugin ID that has the dependency.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the plugin ID of the required dependency.
    /// </summary>
    public Guid DependencyPluginId { get; set; }

    /// <summary>
    /// Gets or sets the minimum required version of the dependency.
    /// </summary>
    public string MinimumVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the maximum allowed version of the dependency.
    /// </summary>
    public string MaximumVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the dependency is optional.
    /// </summary>
    public bool IsOptional { get; set; } = false;

    /// <summary>
    /// Gets or sets the dependency type.
    /// </summary>
    public DependencyType Type { get; set; } = DependencyType.Runtime;

    /// <summary>
    /// Gets or sets a description of the dependency.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks if the provided version satisfies the dependency constraints.
    /// </summary>
    public bool IsSatisfiedBy(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return false;

        var parsed = Version.TryParse(version, out var versionObj);
        if (!parsed)
            return false;

        var minVersion = Version.Parse(MinimumVersion);
        if (versionObj < minVersion)
            return false;

        if (!string.IsNullOrWhiteSpace(MaximumVersion))
        {
            var maxVersion = Version.Parse(MaximumVersion);
            if (versionObj > maxVersion)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a human-readable version constraint description.
    /// </summary>
    public string GetVersionConstraint()
    {
        if (string.IsNullOrWhiteSpace(MaximumVersion))
            return $">= {MinimumVersion}";

        return $">= {MinimumVersion} && <= {MaximumVersion}";
    }

    /// <summary>
    /// Validates the dependency.
    /// </summary>
    public bool IsValid()
    {
        return PluginId != Guid.Empty &&
               DependencyPluginId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(MinimumVersion) &&
               Version.TryParse(MinimumVersion, out _) &&
               (string.IsNullOrWhiteSpace(MaximumVersion) || Version.TryParse(MaximumVersion, out _));
    }
}

/// <summary>
/// Represents the type of plugin dependency.
/// </summary>
public enum DependencyType
{
    Runtime = 0,
    CompileTime = 1,
    Optional = 2,
    Conditional = 3
}
