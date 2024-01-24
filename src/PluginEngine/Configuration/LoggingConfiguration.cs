// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Configuration;

/// <summary>
/// Configuration options for plugin logging and diagnostics.
/// Provides granular control over logging levels, outputs, and formats.
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Enables structured logging for plugin operations.
    /// </summary>
    public bool EnableStructuredLogging { get; set; } = true;

    /// <summary>
    /// Minimum log level for plugin operations.
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Enables logging of detailed operation metrics.
    /// </summary>
    public bool EnableDetailedMetrics { get; set; } = false;

    /// <summary>
    /// Path to log file for persistent logging.
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// Maximum size of log file before rotation (in MB).
    /// </summary>
    public int MaxLogFileSizeMb { get; set; } = 100;

    /// <summary>
    /// Number of log files to retain.
    /// </summary>
    public int MaxLogFiles { get; set; } = 10;

    /// <summary>
    /// Enables performance profiling logs.
    /// </summary>
    public bool EnablePerformanceProfiling { get; set; } = false;

    /// <summary>
    /// Threshold in milliseconds for slow operation logging.
    /// </summary>
    public int SlowOperationThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Enables logging of plugin dependency resolution.
    /// </summary>
    public bool LogDependencyResolution { get; set; } = true;

    /// <summary>
    /// Enables logging of hot reload operations.
    /// </summary>
    public bool LogHotReload { get; set; } = true;

    /// <summary>
    /// Console output format: Simple, Json, or Structured.
    /// </summary>
    public string ConsoleFormat { get; set; } = "Structured";
}

/// <summary>
/// Extension methods for configuring logging in the plugin engine.
/// </summary>
public static class LoggingConfigurationExtensions
{
    /// <summary>
    /// Adds plugin engine logging configuration to the service collection.
    /// </summary>
    public static IServiceCollection AddPluginEngineLogging(
        this IServiceCollection services,
        Action<LoggingConfiguration>? configure = null)
    {
        var config = new LoggingConfiguration();
        configure?.Invoke(config);

        services.AddSingleton(config);
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(config.MinimumLogLevel);
            builder.AddConsole();

            if (!string.IsNullOrEmpty(config.LogFilePath))
            {
                // Configure file logging
                builder.AddFile(config.LogFilePath);
            }
        });

        return services;
    }

    /// <summary>
    /// Configures verbose logging for debugging.
    /// </summary>
    public static LoggingConfiguration WithVerboseLogging(this LoggingConfiguration config)
    {
        config.MinimumLogLevel = LogLevel.Debug;
        config.EnableDetailedMetrics = true;
        config.EnablePerformanceProfiling = true;
        return config;
    }

    /// <summary>
    /// Configures minimal logging for production.
    /// </summary>
    public static LoggingConfiguration WithProductionLogging(this LoggingConfiguration config)
    {
        config.MinimumLogLevel = LogLevel.Warning;
        config.EnableDetailedMetrics = false;
        config.EnablePerformanceProfiling = false;
        return config;
    }

    /// <summary>
    /// Configures file-based logging output.
    /// </summary>
    public static LoggingConfiguration WithFileLogging(
        this LoggingConfiguration config,
        string filePath,
        int maxSizeMb = 100,
        int maxFiles = 10)
    {
        config.LogFilePath = filePath;
        config.MaxLogFileSizeMb = maxSizeMb;
        config.MaxLogFiles = maxFiles;
        return config;
    }
}
