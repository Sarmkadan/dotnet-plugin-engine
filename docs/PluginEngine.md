# PluginEngine

The `PluginEngine` class serves as the primary orchestration component for managing the lifecycle, discovery, and execution of plugins within the `dotnet-plugin-engine` framework. It provides a structured mechanism to initialize the plugin environment, perform bulk loading and unloading operations, and monitor the operational status and health metrics of the engine.

## API

### PluginEngine()
Initializes a new instance of the `PluginEngine` class.

### Task InitializeAsync()
Prepares the engine and its dependencies for operation. This method must be called before attempting to load any plugins. 
- **Throws:** `InvalidOperationException` if the engine is already initialized.

### Task ShutdownAsync()
Performs a graceful shutdown of the engine, ensuring all active plugins are unloaded and resources are released.
- **Throws:** `InvalidOperationException` if the engine is not currently initialized or is already shut down.

### Task<Services.Abstractions.PluginManagerStatus> GetStatusAsync()
Queries the engine for its current operational status.
- **Returns:** A `Services.Abstractions.PluginManagerStatus` value representing the current state (e.g., Initialized, Running, ShutDown).

### Task<int> LoadAllPluginsAsync()
Scans the configured plugin directories for assemblies, validates them, and loads them into the engine.
- **Returns:** The total number of plugins successfully loaded.
- **Throws:** `InvalidOperationException` if called before the engine is initialized.

### Task UnloadAllPluginsAsync()
Unloads all currently loaded plugins and facilitates the cleanup of plugin-specific resources.
- **Throws:** `InvalidOperationException` if called before the engine is initialized.

### Task<string> GetHealthInfoAsync()
Retrieves a summary of the engine's current health and performance metrics.
- **Returns:** A string containing diagnostic information regarding the engine's state.

## Usage

```csharp
// Basic lifecycle management
var engine = new PluginEngine();
await engine.InitializeAsync();

int loadedCount = await engine.LoadAllPluginsAsync();
Console.WriteLine($"Successfully loaded {loadedCount} plugins.");

// Perform plugin operations...

await engine.UnloadAllPluginsAsync();
await engine.ShutdownAsync();
```

```csharp
// Monitoring engine status and health
var engine = new PluginEngine();
await engine.InitializeAsync();
await engine.LoadAllPluginsAsync();

var status = await engine.GetStatusAsync();
if (status == Services.Abstractions.PluginManagerStatus.Running)
{
    string healthInfo = await engine.GetHealthInfoAsync();
    Console.WriteLine($"Engine Health: {healthInfo}");
}
```

## Notes

- **Thread Safety:** While the `PluginEngine` is designed for asynchronous operation, it is not inherently thread-safe for concurrent modifications. Operations that alter the state of the engine (e.g., `InitializeAsync`, `ShutdownAsync`, `LoadAllPluginsAsync`, `UnloadAllPluginsAsync`) should be serialized by the caller to avoid race conditions.
- **State Dependency:** Many methods are dependent on the engine's initialization state. Calling `LoadAllPluginsAsync` or `UnloadAllPluginsAsync` prior to a successful `InitializeAsync` will result in an `InvalidOperationException`.
- **Resource Management:** Ensure that `ShutdownAsync` is called at the end of the engine's lifecycle to prevent memory leaks associated with loaded plugin assemblies and associated dependencies.
