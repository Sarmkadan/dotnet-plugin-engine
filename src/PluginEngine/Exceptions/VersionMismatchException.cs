#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace PluginEngine.Exceptions;

/// <summary>
/// Exception thrown when version constraints are not satisfied.
/// </summary>
public sealed class VersionMismatchException : PluginException
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
    /// Initializes a new instance of the <see cref="VersionMismatchException"/> class.
    /// </summary>
    public VersionMismatchException()
        : base()
    {
        ErrorCode = "VERSION_MISMATCH_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <c>null</c>.</exception>
    public VersionMismatchException(string message)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);
        ErrorCode = "VERSION_MISMATCH_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with a custom message and error code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> or <paramref name="errorCode"/> is <c>null</c>.</exception>
    public VersionMismatchException(string message, string errorCode)
        : base(message, errorCode)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(errorCode);
    }

    /// <summary>
    /// Initializes a new instance with a custom message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> or <paramref name="innerException"/> is <c>null</c>.</exception>
    public VersionMismatchException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(innerException);
        ErrorCode = "VERSION_MISMATCH_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with a custom message, error code, and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
    public VersionMismatchException(string message, string errorCode, Exception innerException)
        : base(message, errorCode, innerException)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(errorCode);
        ArgumentNullException.ThrowIfNull(innerException);
    }

    /// <summary>
    /// Initializes a new instance with full details.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="expectedVersion">The expected version constraint.</param>
    /// <param name="actualVersion">The actual version found.</param>
    /// <param name="componentType">The component type (e.g., Plugin).</param>
    /// <param name="componentName">The component name.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
    public VersionMismatchException(string message, string expectedVersion, string actualVersion, string componentType, string componentName)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(expectedVersion);
        ArgumentNullException.ThrowIfNull(actualVersion);
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(componentName);

        ErrorCode = "VERSION_MISMATCH_ERROR";
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
        ComponentType = componentType;
        ComponentName = componentName;
    }

    /// <summary>
    /// Initializes a new instance with full details and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="expectedVersion">The expected version constraint.</param>
    /// <param name="actualVersion">The actual version found.</param>
    /// <param name="componentType">The component type (e.g., Plugin).</param>
    /// <param name="componentName">The component name.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
    public VersionMismatchException(string message, string expectedVersion, string actualVersion, string componentType, string componentName, Exception innerException)
        : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(expectedVersion);
        ArgumentNullException.ThrowIfNull(actualVersion);
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(componentName);
        ArgumentNullException.ThrowIfNull(innerException);

        ErrorCode = "VERSION_MISMATCH_ERROR";
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
        ComponentType = componentType;
        ComponentName = componentName;
    }

    /// <summary>
    /// Gets a detailed error description.
    /// </summary>
    /// <returns>A formatted string containing the error details.</returns>
    public override string ToString()
    {
        var result = base.ToString();
        result += $"\n{ComponentType}: {ComponentName}\n";
        result += $"Expected: {ExpectedVersion}\n";
        result += $"Actual: {ActualVersion}\n";
        return result;
    }
}
