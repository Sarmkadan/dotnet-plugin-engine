#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Exceptions;

/// <summary>
/// Exception thrown when a plugin declares incompatible version constraints.
/// </summary>
public sealed class PluginIncompatibleException : PluginException
{
    /// <summary>
    /// Gets the declared constraint that was not satisfied.
    /// </summary>
    public string? DeclaredConstraint { get; }

    /// <summary>
    /// Gets the host engine version.
    /// </summary>
    public string? HostEngineVersion { get; }

    /// <summary>
/// Initializes a new instance of the PluginIncompatibleException class with the specified plugin name, constraint, and host version.
/// </summary>
/// <param name="pluginName">The name of the plugin that is incompatible.</param>
/// <param name="constraint">The version constraint that was not satisfied.</param>
/// <param name="hostVersion">The host engine version.</param>
public PluginIncompatibleException(string? pluginName, string? constraint, string? hostVersion)
        : base(
            $"Plugin '{pluginName ?? "null"}' is incompatible with the host engine version {hostVersion ?? "null"}. Constraint '{constraint ?? "null"}' is not satisfied.",
            "PLUGIN_INCOMPATIBLE")
    {
        DeclaredConstraint = constraint;
        HostEngineVersion = hostVersion;
    }

    /// <summary>
/// Returns a string representation of the exception including the declared constraint and host engine version.
/// </summary>
/// <returns>A string representation of the exception.</returns>
public override string ToString()
    {
        return $"{base.ToString()}\nDeclared Constraint: {DeclaredConstraint}\nHost Engine Version: {HostEngineVersion}";
    }
}
