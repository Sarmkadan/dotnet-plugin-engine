#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Exceptions;

/// <summary>
/// Exception thrown when plugin dependency resolution fails.
/// </summary>
public sealed class DependencyResolutionException : PluginException
{
    /// <summary>
    /// Gets or sets the required dependency plugin ID.
    /// </summary>
    public Guid? DependencyPluginId { get; set; }

    /// <summary>
    /// Gets or sets the required dependency version constraint.
    /// </summary>
    public string VersionConstraint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolution error reason.
    /// </summary>
    public DependencyResolutionReason Reason { get; set; } = DependencyResolutionReason.Unknown;

    /// <summary>
    /// Gets or sets the list of unresolved dependency names.
    /// </summary>
    public List<string> UnresolvedDependencies { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the DependencyResolutionException class.
    /// </summary>
    public DependencyResolutionException() : base()
    {
        ErrorCode = "DEPENDENCY_RESOLUTION_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with a message.
    /// </summary>
    public DependencyResolutionException(string message) : base(message)
    {
        ErrorCode = "DEPENDENCY_RESOLUTION_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with a message and reason.
    /// </summary>
    public DependencyResolutionException(string message, DependencyResolutionReason reason)
        : base(message)
    {
        ErrorCode = "DEPENDENCY_RESOLUTION_ERROR";
        Reason = reason;
    }

    /// <summary>
    /// Initializes a new instance with full details.
    /// </summary>
    public DependencyResolutionException(string message, Guid dependencyPluginId, string versionConstraint, DependencyResolutionReason reason)
        : base(message)
    {
        ErrorCode = "DEPENDENCY_RESOLUTION_ERROR";
        DependencyPluginId = dependencyPluginId;
        VersionConstraint = versionConstraint;
        Reason = reason;
    }

    /// <summary>
    /// Adds an unresolved dependency to the list.
    /// </summary>
    public DependencyResolutionException AddUnresolvedDependency(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            UnresolvedDependencies.Add(name);
        }

        return this;
    }

    /// <summary>
    /// Gets a detailed error description.
    /// </summary>
    public override string ToString()
    {
        var result = base.ToString();

        if (UnresolvedDependencies.Count > 0)
        {
            result += $"\nUnresolved Dependencies ({UnresolvedDependencies.Count}):\n";
            result += string.Join("\n", UnresolvedDependencies.Select(d => $"  - {d}"));
        }

        return result;
    }
}

/// <summary>
/// Represents the reason for dependency resolution failure.
/// </summary>
public enum DependencyResolutionReason
{
    /// <summary>The reason could not be determined.</summary>
    Unknown = 0,
    /// <summary>A required dependency is not registered.</summary>
    DependencyNotFound = 1,
    /// <summary>An installed dependency version does not satisfy the constraint.</summary>
    VersionMismatch = 2,
    /// <summary>The dependency graph contains a cycle.</summary>
    CircularDependency = 3,
    /// <summary>A dependency exists but has not been loaded.</summary>
    DependencyNotLoaded = 4,
    /// <summary>An optional dependency failed to resolve.</summary>
    OptionalDependencyFailed = 5
}
