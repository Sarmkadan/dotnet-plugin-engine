#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Constants;

namespace PluginEngine.Configuration;

/// <summary>
/// Configuration options for the plugin engine.
/// </summary>
public sealed class PluginEngineOptions
{
    /// <summary>
    /// Gets or sets the plugin directory path.
    /// </summary>
    public string PluginDirectory { get; set; } = PluginEngineConstants.DefaultPluginDirectory;

    /// <summary>
    /// Gets or sets whether hot reload is enabled.
    /// </summary>
    public bool EnableHotReload { get; set; } = true;

    /// <summary>
    /// Gets or sets the hot reload check interval in milliseconds.
    /// </summary>
    public int HotReloadCheckIntervalMs { get; set; } = PluginEngineConstants.DefaultHotReloadCheckIntervalMs;

    /// <summary>
    /// Gets or sets whether dependency caching is enabled.
    /// </summary>
    public bool EnableDependencyCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the operation timeout in milliseconds.
    /// </summary>
    public int OperationTimeoutMs { get; set; } = PluginEngineConstants.DefaultOperationTimeoutMs;

    /// <summary>
    /// Gets or sets whether logging is enabled.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets the maximum concurrent plugin loads.
    /// </summary>
    public int MaxConcurrentPluginLoads { get; set; } = DefaultMaxConcurrentPluginLoads;

    /// <summary>
    /// Gets or sets the dependency cache TTL in minutes.
    /// </summary>
    public int DependencyCacheTtlMinutes { get; set; } = PluginEngineConstants.DependencyCacheTtlMinutes;

    /// <summary>
    /// Gets or sets the target framework.
    /// </summary>
    public string TargetFramework { get; set; } = PluginEngineConstants.TargetFramework;

    /// <summary>
    /// Gets or sets whether strict version checking is enabled.
    /// </summary>
    public bool StrictVersionChecking { get; set; } = true;

    /// <summary>
    /// Gets or sets whether circular dependency detection is enabled.
    /// </summary>
    public bool EnableCircularDependencyDetection { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum dependency resolution attempts.
    /// </summary>
    public int MaxDependencyResolutionAttempts { get; set; } = PluginEngineConstants.MaxDependencyResolutionAttempts;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(PluginDirectory))
        {
            return false;
        }

        if (HotReloadCheckIntervalMs <= 0)
        {
            return false;
        }

        if (OperationTimeoutMs <= 0)
        {
            return false;
        }

        if (MaxConcurrentPluginLoads <= 0)
        {
            return false;
        }

        if (DependencyCacheTtlMinutes <= 0)
        {
            return false;
        }

        if (MaxDependencyResolutionAttempts <= 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets validation error messages.
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(PluginDirectory))
        {
            errors.Add("Plugin directory cannot be empty.");
        }

        if (HotReloadCheckIntervalMs <= 0)
        {
            errors.Add("Hot reload check interval must be greater than 0.");
        }

        if (OperationTimeoutMs <= 0)
        {
            errors.Add("Operation timeout must be greater than 0.");
        }

        if (MaxConcurrentPluginLoads <= 0)
        {
            errors.Add("Max concurrent plugin loads must be greater than 0.");
        }

        if (DependencyCacheTtlMinutes <= 0)
        {
            errors.Add("Dependency cache TTL must be greater than 0.");
        }

        if (MaxDependencyResolutionAttempts <= 0)
        {
            errors.Add("Max dependency resolution attempts must be greater than 0.");
        }

        return errors;
    }

    /// <summary>
    /// Returns a string representation of the PluginEngineOptions.
    /// </summary>
    /// <returns>A string containing the key settings.</returns>
    public override string ToString()
    {
        return $"PluginEngineOptions: PluginDirectory={PluginDirectory}, EnableHotReload={EnableHotReload}, HotReloadCheckIntervalMs={HotReloadCheckIntervalMs}, OperationTimeoutMs={OperationTimeoutMs}, MaxConcurrentPluginLoads={MaxConcurrentPluginLoads}";
    }

    private const int DefaultMaxConcurrentPluginLoads = 4;
}

/// <summary>
/// Represents the log level.
/// </summary>
public enum LogLevel
{
    /// <summary>Trace-level diagnostic messages.</summary>
    Trace = 0,
    /// <summary>Debug-level diagnostic messages.</summary>
    Debug = 1,
    /// <summary>Informational messages describing normal operation.</summary>
    Information = 2,
    /// <summary>Warnings about recoverable problems.</summary>
    Warning = 3,
    /// <summary>Errors that prevent an operation from completing.</summary>
    Error = 4,
    /// <summary>Critical failures that affect the whole engine.</summary>
    Critical = 5,
    /// <summary>Disables logging.</summary>
    None = 6
}
