# Plugin Engine Constants

The `PluginEngine.Constants` namespace provides compile-time values used by the plugin engine, configuration key names, and error codes.

## Signatures

| Class | Signature | Purpose |
|---|---|---|
| `PluginEngineConstants` | `public static class PluginEngineConstants` | Contains constant values used throughout the plugin engine. |
| `ConfigurationKeys` | `public static class ConfigurationKeys` | Contains keys used in plugin engine configuration. |
| `ErrorCodes` | `public static class ErrorCodes` | Contains error codes used in the plugin engine. |

## `PluginEngineConstants`

| Constant | Value | Meaning |
|---|---:|---|
| `DefaultPluginDirectory` | `"plugins"` | Default plugin directory name. |
| `DefaultConfigFileName` | `"plugin-engine.json"` | Default configuration file name. |
| `LoadContextPrefix` | `"PluginContext_"` | Default load context prefix. |
| `MaxPluginNameLength` | `256` | Maximum plugin name length. |
| `MaxPluginDescriptionLength` | `1024` | Maximum plugin description length. |
| `DefaultOperationTimeoutMs` | `30000` | Default timeout for plugin operations, in milliseconds. |
| `DefaultHotReloadCheckIntervalMs` | `5000` | Default hot reload check interval, in milliseconds. |
| `MinimumDotNetVersion` | `"10.0"` | Minimum supported .NET version. |
| `TargetFramework` | `"net10.0"` | Target framework for plugins. |
| `MetadataFileExtension` | `".plugin.json"` | Plugin metadata file extension. |
| `AssemblyFileExtension` | `".dll"` | Plugin assembly file extension. |
| `MaxDependencyResolutionAttempts` | `10` | Maximum number of dependency resolution attempts. |
| `MaxDirectDependencies` | `20` | Maximum number of direct dependencies allowed for a healthy plugin structure. |
| `AssemblyResolverSubdirectory` | `"assemblies"` | Default assembly resolver search-path subdirectory. |
| `DependencyCacheTtlMinutes` | `60` | Default dependency cache time to live, in minutes. |
| `PluginInterfaceNamespacePrefix` | `"PluginEngine.Interfaces"` | Plugin interface namespace prefix. |
| `DefaultVersionFormat` | `"major.minor.patch"` | Default plugin version format. |
| `LogPrefixPluginLoading` | `"[PluginLoading]"` | Plugin loading log prefix. |
| `LogPrefixPluginUnloading` | `"[PluginUnloading]"` | Plugin unloading log prefix. |
| `LogPrefixHotReload` | `"[HotReload]"` | Hot reload log prefix. |
| `LogPrefixDependencyResolution` | `"[DependencyResolution]"` | Dependency resolution log prefix. |

## `ConfigurationKeys`

| Constant | Value | Meaning |
|---|---|---|
| `SectionName` | `"PluginEngine"` | Section name for plugin engine configuration. |
| `PluginDirectory` | `"PluginDirectory"` | Key for the plugin directory path. |
| `EnableHotReload` | `"EnableHotReload"` | Key for enabling hot reload. |
| `HotReloadCheckInterval` | `"HotReloadCheckInterval"` | Key for the hot reload check interval. |
| `EnableDependencyCaching` | `"EnableDependencyCaching"` | Key for enabling dependency caching. |
| `OperationTimeout` | `"OperationTimeout"` | Key for the operation timeout. |
| `EnableLogging` | `"EnableLogging"` | Key for enabling logging. |
| `LogLevel` | `"LogLevel"` | Key for the log level. |
| `MaxConcurrentPluginLoads` | `"MaxConcurrentPluginLoads"` | Key for the maximum number of concurrent plugin loads. |

## `ErrorCodes`

| Constant | Value | Meaning |
|---|---|---|
| `GenericError` | `"PLUGIN_ERROR"` | Generic plugin error. |
| `PluginNotFound` | `"PLUGIN_NOT_FOUND"` | Plugin not found error. |
| `PluginAlreadyLoaded` | `"PLUGIN_ALREADY_LOADED"` | Plugin already loaded error. |
| `PluginLoadFailed` | `"PLUGIN_LOAD_FAILED"` | Plugin load failed error. |
| `DependencyResolutionFailed` | `"DEPENDENCY_RESOLUTION_FAILED"` | Dependency resolution failed error. |
| `VersionMismatch` | `"VERSION_MISMATCH"` | Version mismatch error. |
| `InvalidConfiguration` | `"INVALID_CONFIGURATION"` | Invalid plugin configuration error. |
| `HotReloadFailed` | `"HOT_RELOAD_FAILED"` | Hot reload failed error. |
| `CircularDependency` | `"CIRCULAR_DEPENDENCY"` | Circular dependency error. |
| `OperationTimeout` | `"OPERATION_TIMEOUT"` | Operation timeout error. |

## Example

```csharp
using PluginEngine.Constants;

string pluginDirectory = PluginEngineConstants.DefaultPluginDirectory;
string configurationSection = ConfigurationKeys.SectionName;
string missingPluginCode = ErrorCodes.PluginNotFound;

Console.WriteLine($"Load plugins from '{pluginDirectory}'.");
Console.WriteLine($"Configuration section: {configurationSection}");
Console.WriteLine($"Missing plugin error code: {missingPluginCode}");
```
