# PluginEngineOptions

`PluginEngineOptions` is a configuration class used to control the behavior of the .NET plugin engine, including plugin discovery, loading, dependency resolution, and runtime behavior.

## API

### `public string PluginDirectory`
Gets or sets the directory path where plugins are located. This directory is scanned for assemblies containing plugin types. Must be a valid, accessible directory path; otherwise, plugin loading will fail.

### `public bool EnableHotReload`
Gets or sets whether hot reload functionality is enabled. When `true`, the engine monitors plugin assemblies for changes and reloads them without restarting the host application. Defaults to `false`.

### `public int HotReloadCheckIntervalMs`
Gets or sets the interval, in milliseconds, at which the engine checks for plugin file changes when hot reload is enabled. Must be a positive integer; otherwise, behavior is undefined.

### `public bool EnableDependencyCaching`
Gets or sets whether resolved plugin dependencies are cached to improve performance. When `true`, dependencies are stored in memory and reused across plugin loads. Defaults to `true`.

### `public int OperationTimeoutMs`
Gets or sets the maximum duration, in milliseconds, allowed for plugin operations such as loading, initialization, or execution. Must be a non-negative integer; a value of `0` indicates no timeout.

### `public bool EnableLogging`
Gets or sets whether diagnostic logging is enabled for the plugin engine. When `true`, internal events and errors are logged. Defaults to `false`.

### `public LogLevel LogLevel`
Gets or sets the minimum severity level for logged messages when logging is enabled. Accepts values from the `LogLevel` enum (e.g., `Debug`, `Info`, `Warning`, `Error`). Defaults to `Info`.

### `public int MaxConcurrentPluginLoads`
Gets or sets the maximum number of plugins that can be loaded concurrently. Must be a positive integer; otherwise, behavior is undefined. Defaults to `4`.

### `public int DependencyCacheTtlMinutes`
Gets or sets the time-to-live, in minutes, for cached plugin dependencies. After this period, cached dependencies are considered stale and re-resolved. Must be a non-negative integer; a value of `0` indicates no expiration.

### `public string TargetFramework`
Gets or sets the target framework moniker (TFM) used to validate plugin compatibility. If specified, plugins must target this framework or a compatible version. Defaults to `null`, indicating no framework restriction.

### `public bool StrictVersionChecking`
Gets or sets whether strict version checking is enforced during plugin and dependency resolution. When `true`, minor or patch version mismatches result in resolution failure. Defaults to `false`.

### `public bool EnableCircularDependencyDetection`
Gets or sets whether circular dependency detection is enabled during plugin resolution. When `true`, the engine throws if a circular dependency graph is detected. Defaults to `true`.

### `public int MaxDependencyResolutionAttempts`
Gets or sets the maximum number of attempts allowed for resolving plugin dependencies. Must be a positive integer; otherwise, behavior is undefined. Defaults to `5`.

### `public bool IsValid`
Gets a value indicating whether the current configuration is valid. Returns `true` if all required settings are present and within acceptable ranges; otherwise, `false`.

### `public List<string> GetValidationErrors()`
Returns a list of validation error messages describing why the current configuration is invalid. Each message corresponds to a specific configuration issue (e.g., invalid directory path, negative timeout). Returns an empty list if `IsValid` is `true`.

## Usage

### Example 1: Basic Configuration
