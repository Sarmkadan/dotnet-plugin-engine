// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Exceptions;

/// <summary>
/// Exception thrown when version constraints are not satisfied.
/// </summary>
public class VersionMismatchException : PluginException
{
    /// <summary>
    /// Gets or sets the expected version constraint.
    /// </summary>
    public string ExpectedVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the actual version found.
    /// </summary>
    public string ActualVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the component type (Plugin, Assembly, etc).
    /// </summary>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the component name.
    /// </summary>
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the VersionMismatchException class.
    /// </summary>
    public VersionMismatchException() : base()
    {
        ErrorCode = "VERSION_MISMATCH_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with a message.
    /// </summary>
    public VersionMismatchException(string message) : base(message)
    {
        ErrorCode = "VERSION_MISMATCH_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with full details.
    /// </summary>
    public VersionMismatchException(string message, string expectedVersion, string actualVersion, string componentType, string componentName)
        : base(message)
    {
        ErrorCode = "VERSION_MISMATCH_ERROR";
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
        ComponentType = componentType;
        ComponentName = componentName;
    }

    /// <summary>
    /// Initializes a new instance with full details and inner exception.
    /// </summary>
    public VersionMismatchException(string message, string expectedVersion, string actualVersion, string componentType, string componentName, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = "VERSION_MISMATCH_ERROR";
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
        ComponentType = componentType;
        ComponentName = componentName;
    }

    /// <summary>
    /// Gets a detailed error description.
    /// </summary>
    public override string ToString()
    {
        var result = base.ToString();
        result = $"{ComponentType}: {ComponentName}\n";
        result += $"Expected: {ExpectedVersion}\n";
        result += $"Actual: {ActualVersion}\n";
        result += result;
        return result;
    }
}
