# LoggingConfiguration
The `LoggingConfiguration` type in the `dotnet-plugin-engine` project provides a set of properties and methods to configure logging for plugin engine applications. It allows developers to customize logging behavior, such as enabling structured logging, setting log levels, and configuring file logging. This configuration is essential for monitoring and troubleshooting plugin engine applications.

## API
The `LoggingConfiguration` type has the following public members:
* `EnableStructuredLogging`: A boolean property that enables or disables structured logging.
* `MinimumLogLevel`: A `LogLevel` property that sets the minimum log level for logging.
* `EnableDetailedMetrics`: A boolean property that enables or disables detailed metrics logging.
* `LogFilePath`: A string property that specifies the file path for log files.
* `MaxLogFileSizeMb`: An integer property that sets the maximum log file size in megabytes.
* `MaxLogFiles`: An integer property that sets the maximum number of log files.
* `EnablePerformanceProfiling`: A boolean property that enables or disables performance profiling.
* `SlowOperationThresholdMs`: An integer property that sets the threshold for slow operation logging in milliseconds.
* `LogDependencyResolution`: A boolean property that enables or disables dependency resolution logging.
* `LogHotReload`: A boolean property that enables or disables hot reload logging.
* `ConsoleFormat`: A string property that specifies the console format for logging.
* `AddPluginEngineLogging`: A static method that adds plugin engine logging to an `IServiceCollection`.
* `WithVerboseLogging`: A static method that returns a `LoggingConfiguration` instance with verbose logging enabled.
* `WithProductionLogging`: A static method that returns a `LoggingConfiguration` instance with production logging settings.
* `WithFileLogging`: A static method that returns a `LoggingConfiguration` instance with file logging enabled.

## Usage
Here are two examples of using the `LoggingConfiguration` type:
```csharp
// Example 1: Enable verbose logging
var loggingConfig = LoggingConfiguration.WithVerboseLogging();
// Use loggingConfig to configure logging

// Example 2: Configure logging with custom settings
var loggingConfig = new LoggingConfiguration
{
    EnableStructuredLogging = true,
    MinimumLogLevel = LogLevel.Debug,
    LogFilePath = "logs/plugin-engine.log",
    MaxLogFileSizeMb = 10,
    MaxLogFiles = 5
};
// Use loggingConfig to configure logging
```

## Notes
When using the `LoggingConfiguration` type, consider the following:
* The `EnableStructuredLogging` property only takes effect when the logging system supports structured logging.
* The `MinimumLogLevel` property filters out log messages with lower log levels.
* The `LogFilePath` property must be a valid file path; otherwise, logging to a file will fail.
* The `MaxLogFileSizeMb` and `MaxLogFiles` properties control log file rotation and retention.
* The `EnablePerformanceProfiling` property may introduce performance overhead due to profiling.
* The `SlowOperationThresholdMs` property helps identify slow operations, but may generate excessive log output if set too low.
* The `LogDependencyResolution` and `LogHotReload` properties are useful for debugging and troubleshooting, but may generate excessive log output in production environments.
* The `ConsoleFormat` property affects the format of log output on the console.
* The `AddPluginEngineLogging` method modifies the `IServiceCollection` instance and returns it for further configuration.
* The `WithVerboseLogging`, `WithProductionLogging`, and `WithFileLogging` methods return pre-configured `LoggingConfiguration` instances for common logging scenarios.
* The `LoggingConfiguration` type is thread-safe, but logging configuration changes may not take effect immediately due to caching or other logging system internals.
