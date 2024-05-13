# IPluginManagerService

Central interface for managing the lifecycle and state of plugins within the `dotnet-plugin-engine` framework. Provides access to initialization status, plugin counts, metadata, and runtime state of loaded assemblies and their dependencies.

## API

### `public bool IsInitialized`
Indicates whether the plugin manager has completed its initialization phase. Returns `true` once all plugins and dependencies have been loaded and validated; `false` otherwise. No parameters or exceptions.

### `public DateTime InitializedAt`
Timestamp marking when the plugin manager completed initialization. Returns the system time at the moment of successful initialization. No parameters or exceptions.

### `public int TotalPlugins`
Total number of plugins known to the manager, including those that failed to load. Returns a non-negative integer. No parameters or exceptions.

### `public int LoadedPlugins`
Number of plugins that were successfully loaded into the runtime. Returns a non-negative integer. No parameters or exceptions.

### `public int ActivePlugins`
Number of plugins currently active and eligible for execution. Returns a non-negative integer. No parameters or exceptions.

### `public int FailedPlugins`
Number of plugins that encountered errors during loading or activation. Returns a non-negative integer. No parameters or exceptions.

### `public string? LastError`
Most recent error message generated during plugin loading or management operations. Returns `null` if no error has occurred. No parameters or exceptions.

### `public Plugin Plugin`
Reference to the root plugin instance managed by this service. Returns `null` if no plugin is currently loaded. No parameters or exceptions.

### `public PluginMetadata? Metadata`
Metadata associated with the root plugin, including descriptive fields such as name, version, and author. Returns `null` if no metadata is available. No parameters or exceptions.

### `public IEnumerable<PluginAssembly> Assemblies`
Collection of assemblies currently loaded by the plugin manager. Returns an enumerable sequence of `PluginAssembly` objects. No parameters or exceptions.

### `public IEnumerable<PluginDependency> Dependencies`
Collection of external dependencies required by the loaded plugins. Returns an enumerable sequence of `PluginDependency` objects. No parameters or exceptions.

### `public IEnumerable<PluginCapability> Capabilities`
Capabilities exposed by the loaded plugins. Returns an enumerable sequence of `PluginCapability` objects. No parameters or exceptions.

### `public string? Name`
Human-readable name of the root plugin or plugin set. Returns `null` if not specified. No parameters or exceptions.

### `public string? Author`
Author or maintainer of the root plugin or plugin set. Returns `null` if not specified. No parameters or exceptions.

### `public PluginStatus? Status`
Current operational status of the plugin manager (e.g., `Initializing`, `Ready`, `Error`). Returns `null` if status is not tracked. No parameters or exceptions.

### `public string? Version`
Version identifier of the root plugin or plugin set. Returns `null` if not specified. No parameters or exceptions.

### `public List<string> Tags`
List of descriptive tags associated with the plugin or plugin set. Returns an empty list if no tags are defined. No parameters or exceptions.

### `public int PageNumber`
Current pagination index used when retrieving paginated plugin data. Returns a non-negative integer. No parameters or exceptions.

### `public int PageSize`
Maximum number of items returned per page in paginated operations. Returns a non-negative integer. No parameters or exceptions.

### `public int TotalPlugins`
Alias for `TotalPlugins`; included for consistency in pagination contexts. Returns the same value as `TotalPlugins`. No parameters or exceptions.

## Usage
