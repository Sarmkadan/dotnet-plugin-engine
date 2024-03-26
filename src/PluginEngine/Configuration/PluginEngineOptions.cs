// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Configuration;

/// <summary>
/// Configuration options for the plugin engine.
/// </summary>
public class PluginEngineOptions
{
    /// <summary>
    /// Gets or sets the plugin directory path.
    /// </summary>
    public string PluginDirectory { get; set; } = "plugins";

    /// <summary>
    /// Gets or sets whether hot reload is enabled.
    /// </summary>
    public bool EnableHotReload { get; set; } = true;

    /// <summary>
    /// Gets or sets the hot reload check interval in milliseconds.
    /// </summary>
    public int HotReloadCheckIntervalMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets whether dependency caching is enabled.
    /// </summary>
    public bool EnableDependencyCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the operation timeout in milliseconds.
    /// </summary>
    public int OperationTimeoutMs { get; set; } = 30000;

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
    public int MaxConcurrentPluginLoads { get; set; } = 4;

    /// <summary>
    /// Gets or sets the dependency cache TTL in minutes.
    /// </summary>
    public int DependencyCacheTtlMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the target framework.
    /// </summary>
    public string TargetFramework { get; set; } = "net10.0";

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
    public int MaxDependencyResolutionAttempts { get; set; } = 10;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(PluginDirectory))
            return false;

        if (HotReloadCheckIntervalMs <= 0)
            return false;

        if (OperationTimeoutMs <= 0)
            return false;

        if (MaxConcurrentPluginLoads <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets validation error messages.
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(PluginDirectory))
            errors.Add("Plugin directory cannot be empty.");

        if (HotReloadCheckIntervalMs <= 0)
            errors.Add("Hot reload check interval must be greater than 0.");

        if (OperationTimeoutMs <= 0)
            errors.Add("Operation timeout must be greater than 0.");

        if (MaxConcurrentPluginLoads <= 0)
            errors.Add("Max concurrent plugin loads must be greater than 0.");

        if (DependencyCacheTtlMinutes <= 0)
            errors.Add("Dependency cache TTL must be greater than 0.");

        if (MaxDependencyResolutionAttempts <= 0)
            errors.Add("Max dependency resolution attempts must be greater than 0.");

        return errors;
    }
}

/// <summary>
/// Represents the log level.
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    None = 6
}
