# API Reference

Complete reference for all public APIs in dotnet-plugin-engine.

## Main Façade: PluginEngine

The primary entry point for all plugin engine functionality.

```csharp
public sealed class PluginEngine
{
    // Initialization
    /// <summary>
    /// Initializes the plugin engine, setting up services and repositories.
    /// Must be called before any other operations.
    /// </summary>
    public Task InitializeAsync();

    // Plugin Discovery & Loading
    /// <summary>
    /// Loads all plugins from the configured plugin directory.
    /// Returns the count of successfully loaded plugins.
    /// </summary>
    public Task<int> LoadAllPluginsAsync();

    /// <summary>
    /// Loads all plugins from a specific directory.
    /// Returns the count of successfully loaded plugins.
    /// </summary>
    public Task<int> LoadAllPluginsAsync(string directory);

    // Plugin Querying
    /// <summary>
    /// Gets all currently loaded plugins.
    /// </summary>
    public Task<IReadOnlyList<Plugin>> GetLoadedPluginsAsync();

    // Plugin Unloading
    /// <summary>
    /// Unloads a plugin and cleans up its AssemblyLoadContext.
    /// </summary>
    /// <exception cref="PluginException">Thrown if unload fails.</exception>
    public Task UnloadPluginAsync(string pluginId);

    // Diagnostics
    /// <summary>
    /// Gets comprehensive health information about all loaded plugins.
    /// </summary>
    public Task<EngineHealthInfo> GetHealthInfoAsync();

    /// <summary>
    /// Gets engine statistics including load times and reload counts.
    /// </summary>
    public Task<PluginStatistics> GetStatisticsAsync();

    // Cleanup
    /// <summary>
    /// Gracefully shuts down the engine, unloading all plugins.
    /// </summary>
    public Task ShutdownAsync();
}
```

## IPluginLoaderService

Handles plugin discovery, loading, and unloading.

```csharp
public interface IPluginLoaderService
{
    /// <summary>
    /// Loads a plugin from the specified assembly path.
    /// Creates a new AssemblyLoadContext for isolation.
    /// </summary>
    /// <param name="assemblyPath">Path to the plugin DLL file.</param>
    /// <returns>The loaded Plugin entity.</returns>
    /// <exception cref="PluginLoadException">Thrown if loading fails.</exception>
    Task<Plugin> LoadPluginAsync(string assemblyPath);

    /// <summary>
    /// Loads all plugins from the specified directory.
    /// Recursively searches subdirectories if configured.
    /// </summary>
    /// <param name="directory">Path to search for plugin DLLs.</param>
    /// <returns>List of successfully loaded plugins.</returns>
    Task<IReadOnlyList<Plugin>> LoadPluginsFromDirectoryAsync(string directory);

    /// <summary>
    /// Unloads a plugin and releases its AssemblyLoadContext.
    /// </summary>
    /// <param name="plugin">The plugin to unload.</param>
    /// <exception cref="PluginException">Thrown if unload fails.</exception>
    Task UnloadPluginAsync(Plugin plugin);

    /// <summary>
    /// Reloads a plugin by unloading and reloading from disk.
    /// Used by hot reload functionality.
    /// </summary>
    /// <param name="pluginId">The ID of the plugin to reload.</param>
    /// <returns>The reloaded Plugin entity.</returns>
    Task<Plugin> ReloadPluginAsync(string pluginId);
}
```

## IDependencyResolutionService

Sophisticated dependency analysis and resolution.

```csharp
public interface IDependencyResolutionService
{
    /// <summary>
    /// Resolves all direct and transitive dependencies for a plugin.
    /// Respects version constraints and caching.
    /// </summary>
    /// <param name="plugin">The plugin to resolve dependencies for.</param>
    /// <returns>List of all resolved dependencies.</returns>
    /// <exception cref="DependencyResolutionException">If dependencies cannot be resolved.</exception>
    Task<IReadOnlyList<PluginDependency>> ResolveDependenciesAsync(Plugin plugin);

    /// <summary>
    /// Validates that all plugin dependencies are currently satisfied.
    /// Checks that required plugins are loaded and versions match constraints.
    /// </summary>
    /// <param name="plugin">The plugin to validate.</param>
    /// <returns>True if all dependencies are satisfied, false otherwise.</returns>
    Task<bool> ValidateDependenciesAsync(Plugin plugin);

    /// <summary>
    /// Detects if a plugin has any circular dependencies.
    /// Performs depth-first search through dependency graph.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns>True if circular dependencies exist, false otherwise.</returns>
    Task<bool> HasCircularDependenciesAsync(Plugin plugin);

    /// <summary>
    /// Builds a complete dependency graph for the specified plugin.
    /// Includes all transitive dependencies and their relationships.
    /// </summary>
    /// <param name="pluginId">The ID of the plugin.</param>
    /// <returns>A DependencyGraph object representing all relationships.</returns>
    Task<DependencyGraph> GetDependencyGraphAsync(string pluginId);
}
```

## IVersioningService

Version parsing, comparison, and constraint validation.

```csharp
public interface IVersioningService
{
    /// <summary>
    /// Parses a version string into a VersionInfo object.
    /// Supports semantic versioning including pre-release and build metadata.
    /// </summary>
    /// <param name="versionString">Version string (e.g., "1.2.3", "1.0.0-alpha.1")</param>
    /// <returns>Parsed VersionInfo object.</returns>
    VersionInfo ParseVersion(string versionString);

    /// <summary>
    /// Compares two versions.
    /// Returns negative if v1 < v2, zero if equal, positive if v1 > v2.
    /// </summary>
    int CompareVersions(VersionInfo v1, VersionInfo v2);

    /// <summary>
    /// Checks if a version satisfies a constraint.
    /// Supports operators: =, !=, <, >, <=, >=, ~, ^ and ranges like >=1.0.0,<2.0.0
    /// </summary>
    /// <param name="version">The version to check.</param>
    /// <param name="constraint">The constraint expression.</param>
    /// <returns>True if constraint is satisfied, false otherwise.</returns>
    bool SatisfiesConstraint(VersionInfo version, string constraint);

    /// <summary>
    /// Checks semantic version compatibility between two versions.
    /// v2.0.0+ breaks compatibility with v1.x.x (major version bump).
    /// </summary>
    bool IsCompatibleVersion(VersionInfo v1, VersionInfo v2);
}
```

## IHotReloadService

Plugin hot reload monitoring and execution.

```csharp
public interface IHotReloadService
{
    /// <summary>
    /// Starts automatic monitoring for plugin file changes.
    /// Uses FileSystemWatcher with debouncing to prevent rapid reloads.
    /// </summary>
    Task StartHotReloadMonitoringAsync();

    /// <summary>
    /// Stops automatic monitoring for plugin file changes.
    /// Previously triggered reloads will not occur.
    /// </summary>
    Task StopHotReloadMonitoringAsync();

    /// <summary>
    /// Performs a hot reload of a specific plugin.
    /// Unloads the current version and loads the latest from disk.
    /// </summary>
    /// <param name="pluginId">The ID of the plugin to reload.</param>
    /// <returns>True if reload succeeded, false otherwise.</returns>
    Task<bool> HotReloadPluginAsync(string pluginId);

    /// <summary>
    /// Registers a callback to be executed when a plugin is reloaded.
    /// Useful for re-initializing plugin state after reload.
    /// </summary>
    /// <param name="pluginId">The plugin to watch for reloads.</param>
    /// <param name="callback">Async callback executed with the reloaded plugin.</param>
    Task RegisterHotReloadCallback(string pluginId, Func<Plugin, Task> callback);

    /// <summary>
    /// Gets statistics about hot reload operations.
    /// Includes reload counts, timing, and error information.
    /// </summary>
    Task<HotReloadStatistics> GetStatisticsAsync();
}
```

## IPluginManagerService

High-level plugin orchestration and execution.

```csharp
public interface IPluginManagerService
{
    /// <summary>
    /// Gets all currently loaded plugins.
    /// </summary>
    Task<IReadOnlyList<Plugin>> GetAllLoadedPluginsAsync();

    /// <summary>
    /// Gets a specific plugin by ID.
    /// Returns null if plugin is not loaded.
    /// </summary>
    Task<Plugin?> GetPluginAsync(string pluginId);

    /// <summary>
    /// Executes a plugin with the provided context.
    /// Operations are wrapped in middleware pipeline for logging, caching, etc.
    /// </summary>
    /// <param name="pluginId">The plugin to execute.</param>
    /// <param name="context">Execution context with parameters.</param>
    /// <returns>Result containing either the value or error information.</returns>
    Task<PluginOperationResult> ExecutePluginAsync(
        string pluginId,
        PluginExecutionContext context);

    /// <summary>
    /// Enables a plugin, allowing it to be executed.
    /// Validates dependencies are satisfied before enabling.
    /// </summary>
    /// <exception cref="DependencyResolutionException">If dependencies not satisfied.</exception>
    Task EnablePluginAsync(string pluginId);

    /// <summary>
    /// Disables a plugin, preventing further execution.
    /// Does not unload the plugin from memory.
    /// </summary>
    Task DisablePluginAsync(string pluginId);
}
```

## Domain Entities

### Plugin

```csharp
public class Plugin
{
    /// <summary>Unique identifier for the plugin.</summary>
    public string Id { get; set; } = null!;

    /// <summary>Human-readable name of the plugin.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Semantic version of the plugin.</summary>
    public string Version { get; set; } = null!;

    /// <summary>Optional metadata extracted from assembly.</summary>
    public PluginMetadata? Metadata { get; set; }

    /// <summary>List of plugins this plugin depends on.</summary>
    public IReadOnlyList<PluginDependency> Dependencies { get; set; } = [];

    /// <summary>List of capabilities/features provided by this plugin.</summary>
    public IReadOnlyList<PluginCapability> Capabilities { get; set; } = [];

    /// <summary>Information about the plugin's assembly.</summary>
    public PluginAssembly Assembly { get; set; } = null!;

    /// <summary>Information about the AssemblyLoadContext isolating this plugin.</summary>
    public AssemblyLoadContextInfo? ALCInfo { get; set; }

    /// <summary>Whether the plugin is currently enabled.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>When the plugin was loaded.</summary>
    public DateTime LoadedAt { get; set; }
}
```

### PluginDependency

```csharp
public class PluginDependency
{
    /// <summary>ID of the plugin this dependency refers to.</summary>
    public string Id { get; set; } = null!;

    /// <summary>Name of the dependent plugin.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Version constraint (e.g., ">=1.0.0,<2.0.0").</summary>
    public string VersionConstraint { get; set; } = "*";

    /// <summary>Whether this is an optional dependency.</summary>
    public bool IsOptional { get; set; }

    /// <summary>Description of why this dependency is needed.</summary>
    public string? Description { get; set; }
}
```

### PluginCapability

```csharp
public class PluginCapability
{
    /// <summary>Unique name of the capability within the plugin.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Version of this capability.</summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>Description of what this capability provides.</summary>
    public string? Description { get; set; }

    /// <summary>Optional metadata for the capability.</summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
```

### VersionInfo

```csharp
public class VersionInfo
{
    /// <summary>Major version number (breaking changes).</summary>
    public int Major { get; set; }

    /// <summary>Minor version number (new features).</summary>
    public int Minor { get; set; }

    /// <summary>Patch version number (bug fixes).</summary>
    public int Patch { get; set; }

    /// <summary>Pre-release identifier (alpha, beta, rc).</summary>
    public string? PreRelease { get; set; }

    /// <summary>Build metadata.</summary>
    public string? BuildMetadata { get; set; }

    /// <summary>Original version string.</summary>
    public string Original { get; set; } = null!;

    /// <summary>Returns normalized version string.</summary>
    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
```

### PluginMetadata

```csharp
public class PluginMetadata
{
    /// <summary>Plugin author name.</summary>
    public string? Author { get; set; }

    /// <summary>Plugin author email.</summary>
    public string? AuthorEmail { get; set; }

    /// <summary>Plugin description.</summary>
    public string? Description { get; set; }

    /// <summary>Plugin website or documentation URL.</summary>
    public string? ProjectUrl { get; set; }

    /// <summary>License identifier (e.g., MIT, Apache-2.0).</summary>
    public string? License { get; set; }

    /// <summary>Tags for categorizing the plugin.</summary>
    public IReadOnlyList<string> Tags { get; set; } = [];

    /// <summary>Additional metadata as key-value pairs.</summary>
    public Dictionary<string, string> AdditionalMetadata { get; set; } = [];
}
```

### PluginAssembly

```csharp
public class PluginAssembly
{
    /// <summary>Full path to the plugin assembly file.</summary>
    public string FilePath { get; set; } = null!;

    /// <summary>Name of the assembly.</summary>
    public string AssemblyName { get; set; } = null!;

    /// <summary>Size of the assembly file in bytes.</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>When the assembly file was last modified.</summary>
    public DateTime LastModified { get; set; }

    /// <summary>Target framework (e.g., net10.0).</summary>
    public string TargetFramework { get; set; } = null!;

    /// <summary>Whether the assembly is currently loaded.</summary>
    public bool IsLoaded { get; set; }
}
```

## Configuration

### PluginEngineOptions

```csharp
public class PluginEngineOptions
{
    // Discovery
    public string PluginDirectory { get; set; } = "plugins";

    // Hot Reload
    public bool EnableHotReload { get; set; } = true;
    public int HotReloadCheckIntervalMs { get; set; } = 5000;

    // Caching
    public bool EnableDependencyCaching { get; set; } = true;
    public int DependencyCacheTTLMs { get; set; } = 300000;

    // Performance
    public int OperationTimeoutMs { get; set; } = 30000;
    public int MaxConcurrentPluginLoads { get; set; } = 4;

    // Validation
    public bool StrictVersionChecking { get; set; } = true;
    public bool EnableCircularDependencyDetection { get; set; } = true;
    public int MaxDependencyResolutionAttempts { get; set; } = 10;

    // Logging
    public bool EnableLogging { get; set; } = true;
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    // Webhooks
    public WebhookConfiguration? WebhookConfig { get; set; }
}
```

## Exceptions

### PluginException

Base exception for all plugin engine errors.

```csharp
public class PluginException : Exception
{
    public string ErrorCode { get; set; }
    public string? PluginName { get; set; }
    public DateTime OccurredAt { get; set; }
}
```

### PluginLoadException

Thrown when plugin loading fails.

```csharp
public class PluginLoadException : PluginException
{
    public PluginLoadStage LoadStage { get; set; }
    public string? FileName { get; set; }

    public enum PluginLoadStage
    {
        Discovery,
        AssemblyLoad,
        MetadataExtraction,
        ALCCreation,
        Initialization
    }
}
```

### DependencyResolutionException

Thrown when dependencies cannot be resolved.

```csharp
public class DependencyResolutionException : PluginException
{
    public IReadOnlyList<string> UnresolvedDependencies { get; set; }
    public IReadOnlyList<VersionConstraintViolation> VersionViolations { get; set; }
}
```

### VersionMismatchException

Thrown when version constraints are violated.

```csharp
public class VersionMismatchException : PluginException
{
    public string RequiredVersion { get; set; }
    public string AvailableVersion { get; set; }
    public string Constraint { get; set; }
}
```

## Extension Interfaces

### IPluginRepository

Data access abstraction for plugin persistence.

```csharp
public interface IPluginRepository
{
    Task AddAsync(Plugin plugin);
    Task<Plugin?> GetAsync(string id);
    Task UpdateAsync(Plugin plugin);
    Task DeleteAsync(string id);
    Task<IReadOnlyList<Plugin>> GetAllAsync();
    Task<IReadOnlyList<Plugin>> QueryAsync(Func<Plugin, bool> predicate);
}
```

### IPluginFormatter

Output formatters for plugin information.

```csharp
public interface IPluginFormatter
{
    Task<string> FormatAsync(IEnumerable<Plugin> plugins);
    Task<string> FormatAsync(Plugin plugin);
    string ContentType { get; }
}
```

## Usage Examples

See the [examples/](../examples/) directory for complete working code samples demonstrating all APIs.
