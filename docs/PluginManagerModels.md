# PluginManagerModels

Data models used by the plugin manager service for status reporting, plugin details, search filtering, and statistics.

## API

### `public sealed class PluginManagerStatus`

Represents the status of the plugin manager.

- **Returned by**: `IPluginManagerService.GetStatusAsync()`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsInitialized` | `bool` | Indicates whether the plugin manager is initialized. |
| `InitializedAt` | `DateTime` | Date and time when the plugin manager was initialized. |
| `TotalPlugins` | `int` | Total number of plugins discovered. |
| `LoadedPlugins` | `int` | Number of plugins that have been successfully loaded. |
| `ActivePlugins` | `int` | Number of plugins that are currently active. |
| `FailedPlugins` | `int` | Number of plugins that have failed to load or initialize. |
| `LastError` | `string?` | Last error message encountered by the plugin manager, if any. |

---

### `public sealed class PluginDetails`

Represents detailed information about a plugin.

- **Returned by**: `IPluginManagerService.GetPluginDetailsAsync(Guid pluginId)`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Plugin` | `Plugin` | The plugin entity. |
| `Metadata` | `PluginMetadata?` | Metadata associated with the plugin. |
| `Assemblies` | `IEnumerable<PluginAssembly>` | Collection of assemblies that comprise the plugin. |
| `Dependencies` | `IEnumerable<PluginDependency>` | Collection of dependencies declared by the plugin. |
| `Capabilities` | `IEnumerable<PluginCapability>` | Collection of capabilities declared by the plugin. |

---

### `public sealed class PluginSearchCriteria`

Represents search criteria for plugins.

- **Consumed by**: `IPluginManagerService.SearchPluginsAsync(PluginSearchCriteria criteria)`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string?` | Name filter for plugin search. |
| `Author` | `string?` | Author filter for plugin search. |
| `Status` | `PluginStatus?` | Status filter for plugin search. |
| `Version` | `string?` | Version filter for plugin search. |
| `Tags` | `List<string>` | Tags filter for plugin search. |
| `PageNumber` | `int` | Page number for paginated results (default: 1). |
| `PageSize` | `int` | Page size for paginated results (default: 10). |

---

### `public sealed class PluginManagerStatistics`

Represents plugin manager statistics.

- **Returned by**: `IPluginManagerService.GetStatisticsAsync()`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TotalPlugins` | `int` | Total number of plugins discovered. |
| `LoadedPlugins` | `int` | Number of plugins that have been successfully loaded. |
| `ActivePlugins` | `int` | Number of plugins that are currently active. |
| `FailedPlugins` | `int` | Number of plugins that have failed to load or initialize. |
| `TotalMemoryUsageBytes` | `long` | Total memory usage in bytes by all loaded plugins. |
| `TotalLoadContexts` | `int` | Total number of assembly load contexts created for plugins. |
| `LastOperationTime` | `DateTime?` | Timestamp of the last operation performed by the plugin manager. |
| `AverageLoadTimeMs` | `double` | Average load time in milliseconds for plugins. |

## Usage

These models are used throughout the plugin manager service to provide status information, detailed plugin data, search capabilities, and operational statistics.