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
            UnresolvedDependencies.Add(name);
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
    Unknown = 0,
    DependencyNotFound = 1,
    VersionMismatch = 2,
    CircularDependency = 3,
    DependencyNotLoaded = 4,
    OptionalDependencyFailed = 5
}
