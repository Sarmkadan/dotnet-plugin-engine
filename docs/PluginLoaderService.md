# PluginLoaderService

`PluginLoaderService` is the central runtime component responsible for loading, unloading, reloading, and tracking .NET plugins within the `dotnet-plugin-engine` framework. It wraps `PluginAssemblyLoadContext` to provide isolation boundaries per plugin and exposes a fully asynchronous API for lifecycle management, discovery from directories, and status queries.

## API

### PluginAssemblyLoadContext(string name, string pluginPath)
**Note:** This is the base class constructor, not a member of `PluginLoaderService` itself. It is invoked internally when creating isolated load contexts for each plugin.

- **Parameters:**
  - `name` (`string`): A unique logical name assigned to the load context, typically derived from the plugin identifier.
  - `pluginPath` (`string`): The absolute or relative file system path to the plugin assembly (.dll).
- **Purpose:** Initializes a dedicated `AssemblyLoadContext` that isolates the plugin’s dependencies from the host and from other plugins. This constructor is called by `PluginLoaderService` during load operations; consumers do not instantiate it directly.

---

### PluginLoaderService
The default constructor for the service. It prepares internal registries for tracking loaded plugins and their associated load contexts. No configuration or parameters are required at construction time.

---

### async Task<Plugin> LoadPluginAsync
- **Parameters:**
  - `pluginPath` (`string`): The file path to the plugin assembly.
  - *(Optionally)* `pluginName` (`string`): A unique name for the plugin. If omitted, the name is inferred from the assembly metadata or file name.
- **Returns:** `Task<Plugin>` – A `Plugin` object representing the loaded plugin, including its metadata, loaded types, and the isolated load context reference.
- **Purpose:** Loads a single plugin assembly into its own `PluginAssemblyLoadContext`, discovers exported types, and registers the plugin in the service’s internal catalog.
- **Exceptions:**
  - `ArgumentException` when `pluginPath` is null, empty, or does not point to an existing file.
  - `InvalidOperationException` when a plugin with the same name is already loaded.
  - `PluginLoadException` (or derived) when the assembly cannot be loaded, does not contain valid plugin types, or dependency resolution fails.

---

### async Task<bool> UnloadPluginAsync
- **Parameters:**
  - `pluginName` (`string`): The unique name of the plugin to unload.
- **Returns:** `Task<bool>` – `true` if the plugin was found and successfully unloaded; `false` if no plugin with the given name was loaded.
- **Purpose:** Unloads a previously loaded plugin by disposing its `PluginAssemblyLoadContext` and removing it from the internal registry. After a successful unload, the plugin’s types and assemblies become eligible for garbage collection.
- **Exceptions:**
  - `PluginUnloadException` when the load context cannot be cleanly unloaded (e.g., lingering references prevent GC).

---

### async Task<Plugin?> GetLoadedPluginAsync
- **Parameters:**
  - `pluginName` (`string`): The unique name of the plugin to retrieve.
- **Returns:** `Task<Plugin?>` – The `Plugin` instance if found; `null` otherwise.
- **Purpose:** Provides a non-destructive lookup of a loaded plugin by name without altering its state.

---

### async Task<IEnumerable<Plugin>> GetAllLoadedPluginsAsync
- **Returns:** `Task<IEnumerable<Plugin>>` – A collection of all currently loaded `Plugin` instances. Returns an empty enumerable if no plugins are loaded.
- **Purpose:** Enumerates the entire set of active plugins for inspection, diagnostics, or batch operations.

---

### async Task<bool> IsPluginLoadedAsync
- **Parameters:**
  - `pluginName` (`string`): The unique name to check.
- **Returns:** `Task<bool>` – `true` if a plugin with the given name is currently loaded; `false` otherwise.
- **Purpose:** Lightweight existence check without retrieving the full `Plugin` object.

---

### async Task<IEnumerable<Plugin>> LoadPluginsFromDirectoryAsync
- **Parameters:**
  - `directoryPath` (`string`): The path to a directory containing plugin assemblies.
  - *(Optionally)* `searchPattern` (`string`): A file pattern (e.g., `"*.dll"`) to filter candidate assemblies. Defaults to `"*.dll"`.
- **Returns:** `Task<IEnumerable<Plugin>>` – A collection of successfully loaded `Plugin` instances from the directory.
- **Purpose:** Scans a directory for assemblies, attempts to load each as a plugin, and returns those that were loaded successfully. Assemblies that fail to load are skipped (errors are typically logged, not thrown to the caller).
- **Exceptions:**
  - `ArgumentException` when `directoryPath` is null, empty, or does not point to an existing directory.

---

### async Task<Plugin> ReloadPluginAsync
- **Parameters:**
  - `pluginName` (`string`): The unique name of the plugin to reload.
  - *(Optionally)* `pluginPath` (`string`): An updated path to the plugin assembly. If omitted, the original path is reused.
- **Returns:** `Task<Plugin>` – A new `Plugin` instance representing the reloaded plugin.
- **Purpose:** Performs an atomic unload-then-load cycle for the named plugin. The old `PluginAssemblyLoadContext` is disposed, and a fresh context is created from the assembly at the given (or original) path. This enables hot-reloading of plugin code without restarting the host process.
- **Exceptions:**
  - `InvalidOperationException` when no plugin with the given name is currently loaded.
  - `PluginLoadException` when the reloaded assembly fails to load.
  - `PluginUnloadException` when the existing plugin cannot be cleanly unloaded before reload.

## Usage

### Example 1: Load, inspect, and unload a single plugin

```csharp
var service = new PluginLoaderService();

// Load a plugin from a known path
Plugin plugin = await service.LoadPluginAsync(
    pluginPath: @"C:\Plugins\MyPlugin.dll",
    pluginName: "MyPlugin"
);

Console.WriteLine($"Loaded: {plugin.Name} v{plugin.Version}");

// Check if it is still tracked
bool isLoaded = await service.IsPluginLoadedAsync("MyPlugin");
Console.WriteLine($"Is loaded: {isLoaded}");

// Unload when no longer needed
bool unloaded = await service.UnloadPluginAsync("MyPlugin");
Console.WriteLine($"Unloaded: {unloaded}");
```

### Example 2: Bulk load from a directory and reload a specific plugin

```csharp
var service = new PluginLoaderService();

// Load all compatible plugins from a directory
IEnumerable<Plugin> plugins = await service.LoadPluginsFromDirectoryAsync(
    directoryPath: @"C:\Plugins\Release",
    searchPattern: "*.plugin.dll"
);

Console.WriteLine($"Loaded {plugins.Count()} plugins from directory.");

// Later, hot-reload a specific plugin after an update
Plugin reloaded = await service.ReloadPluginAsync(
    pluginName: "PaymentProcessor",
    pluginPath: @"C:\Plugins\Release\PaymentProcessor.v2.plugin.dll"
);

Console.WriteLine($"Reloaded: {reloaded.Name} v{reloaded.Version}");
```

## Notes

- **Isolation:** Each plugin is loaded into its own `PluginAssemblyLoadContext`, preventing type collisions and allowing independent dependency versions. Unloading a plugin disposes its context, but actual assembly unloading depends on the GC collecting all references.
- **Unload reliability:** `UnloadPluginAsync` returns `false` for unknown names rather than throwing. A `PluginUnloadException` is thrown only when the context disposal itself fails, which typically indicates a reference leak from the host or another plugin.
- **Directory loading resilience:** `LoadPluginsFromDirectoryAsync` does not throw for individual assembly failures. It logs errors internally and returns only the successfully loaded plugins. Callers should compare the returned count against the expected number of assemblies to detect partial failures.
- **Reload atomicity:** `ReloadPluginAsync` unloads the existing plugin before loading the new one. If the load step fails, the plugin ends up in an unloaded state—there is no automatic rollback to the previous version. Callers should implement retry or fallback logic if continuity is critical.
- **Thread safety:** All public methods are asynchronous and designed to be called from multiple threads. Internal registries use synchronization mechanisms to prevent corruption during concurrent load/unload/reload operations. However, external coordination is still required if multiple callers attempt to manage the same plugin name simultaneously (e.g., two concurrent reloads of the same plugin may race).
- **Name uniqueness:** Plugin names are the primary key in the internal registry. Loading a second plugin with the same name throws `InvalidOperationException`. Renaming or fully unloading the original is required before reuse.
