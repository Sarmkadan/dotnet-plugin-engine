#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Constants;

/// <summary>
/// Contains constant values used throughout the plugin engine.
/// </summary>
public static class PluginEngineConstants
{
    /// <summary>
    /// Default plugin directory name.
    /// </summary>
    public const string DefaultPluginDirectory = "plugins";

    /// <summary>
    /// Default configuration file name.
    /// </summary>
    public const string DefaultConfigFileName = "plugin-engine.json";

    /// <summary>
    /// Default load context prefix.
    /// </summary>
    public const string LoadContextPrefix = "PluginContext_";

    /// <summary>
    /// Maximum plugin name length.
    /// </summary>
    public const int MaxPluginNameLength = 256;

    /// <summary>
    /// Maximum plugin description length.
    /// </summary>
    public const int MaxPluginDescriptionLength = 1024;

    /// <summary>
    /// Default timeout for plugin operations in milliseconds.
    /// </summary>
    public const int DefaultOperationTimeoutMs = 30000;

    /// <summary>
    /// Default hot reload check interval in milliseconds.
    /// </summary>
    public const int DefaultHotReloadCheckIntervalMs = 5000;

    /// <summary>
    /// Minimum supported .NET version.
    /// </summary>
    public const string MinimumDotNetVersion = "10.0";

    /// <summary>
    /// Target framework for plugins.
    /// </summary>
    public const string TargetFramework = "net10.0";

    /// <summary>
    /// Plugin metadata file extension.
    /// </summary>
    public const string MetadataFileExtension = ".plugin.json";

    /// <summary>
    /// Plugin assembly extension.
    /// </summary>
    public const string AssemblyFileExtension = ".dll";

    /// <summary>
    /// Maximum number of dependency resolution attempts.
    /// </summary>
    public const int MaxDependencyResolutionAttempts = 10;

    /// <summary>
    /// Maximum number of direct dependencies allowed for healthy plugin structure.
    /// </summary>
    public const int MaxDirectDependencies = 20;

    /// <summary>
    /// Default assembly resolver search paths subdirectory.
    /// </summary>
    public const string AssemblyResolverSubdirectory = "assemblies";

    /// <summary>
    /// Default dependency cache ttl in minutes.
    /// </summary>
    public const int DependencyCacheTtlMinutes = 60;

    /// <summary>
    /// Plugin interface namespace prefix.
    /// </summary>
    public const string PluginInterfaceNamespacePrefix = "PluginEngine.Interfaces";

    /// <summary>
    /// Default plugin version format.
    /// </summary>
    public const string DefaultVersionFormat = "major.minor.patch";

    /// <summary>
    /// Plugin loading log prefix.
    /// </summary>
    public const string LogPrefixPluginLoading = "[PluginLoading]";

    /// <summary>
    /// Plugin unloading log prefix.
    /// </summary>
    public const string LogPrefixPluginUnloading = "[PluginUnloading]";

    /// <summary>
    /// Hot reload log prefix.
    /// </summary>
    public const string LogPrefixHotReload = "[HotReload]";

    /// <summary>
    /// Dependency resolution log prefix.
    /// </summary>
    public const string LogPrefixDependencyResolution = "[DependencyResolution]";
}

/// <summary>
/// Configuration keys used in plugin engine configuration.
/// </summary>
public static class ConfigurationKeys
{
    /// <summary>
    /// Section name for plugin engine configuration.
    /// </summary>
    public const string SectionName = "PluginEngine";

    /// <summary>
    /// Key for plugin directory path.
    /// </summary>
    public const string PluginDirectory = "PluginDirectory";

    /// <summary>
    /// Key for enable hot reload.
    /// </summary>
    public const string EnableHotReload = "EnableHotReload";

    /// <summary>
    /// Key for hot reload check interval.
    /// </summary>
    public const string HotReloadCheckInterval = "HotReloadCheckInterval";

    /// <summary>
    /// Key for enable dependency caching.
    /// </summary>
    public const string EnableDependencyCaching = "EnableDependencyCaching";

    /// <summary>
    /// Key for operation timeout.
    /// </summary>
    public const string OperationTimeout = "OperationTimeout";

    /// <summary>
    /// Key for enable logging.
    /// </summary>
    public const string EnableLogging = "EnableLogging";

    /// <summary>
    /// Key for log level.
    /// </summary>
    public const string LogLevel = "LogLevel";

    /// <summary>
    /// Key for maximum concurrent plugin loads.
    /// </summary>
    public const string MaxConcurrentPluginLoads = "MaxConcurrentPluginLoads";
}

/// <summary>
/// Error codes used in the plugin engine.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Generic plugin error.
    /// </summary>
    public const string GenericError = "PLUGIN_ERROR";

    /// <summary>
    /// Plugin not found error.
    /// </summary>
    public const string PluginNotFound = "PLUGIN_NOT_FOUND";

    /// <summary>
    /// Plugin already loaded error.
    /// </summary>
    public const string PluginAlreadyLoaded = "PLUGIN_ALREADY_LOADED";

    /// <summary>
    /// Plugin load failed error.
    /// </summary>
    public const string PluginLoadFailed = "PLUGIN_LOAD_FAILED";

    /// <summary>
    /// Dependency resolution failed error.
    /// </summary>
    public const string DependencyResolutionFailed = "DEPENDENCY_RESOLUTION_FAILED";

    /// <summary>
    /// Version mismatch error.
    /// </summary>
    public const string VersionMismatch = "VERSION_MISMATCH";

    /// <summary>
    /// Invalid plugin configuration error.
    /// </summary>
    public const string InvalidConfiguration = "INVALID_CONFIGURATION";

    /// <summary>
    /// Hot reload failed error.
    /// </summary>
    public const string HotReloadFailed = "HOT_RELOAD_FAILED";

    /// <summary>
    /// Circular dependency error.
    /// </summary>
    public const string CircularDependency = "CIRCULAR_DEPENDENCY";

    /// <summary>
    /// Operation timeout error.
    /// </summary>
    public const string OperationTimeout = "OPERATION_TIMEOUT";
}
