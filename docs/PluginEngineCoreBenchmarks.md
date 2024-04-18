# PluginEngineCoreBenchmarks

This type contains a set of asynchronous benchmark methods that measure the performance of core operations in the plugin engine, such as engine initialization, plugin loading/unloading, and health/status queries. It is intended to be used with a benchmarking framework (e.g., BenchmarkDotNet) to obtain reliable timing data for the plugin engine’s public API.

## API

### GlobalSetup
**Purpose:** Performs one‑time initialization required before any benchmark runs, such as creating a shared plugin engine instance and registering test plugins.  
**Parameters:** None.  
**Return value:** `Task` that completes when the setup finishes.  
**Throws:**  
- `InvalidOperationException` if the engine cannot be created (e.g., missing dependencies).  
- `PluginRegistrationException` if a test plugin fails to register.

### GlobalCleanup
**Purpose:** Releases resources allocated in `GlobalSetup`, disposing of the plugin engine and unregistering test plugins.  
**Parameters:** None.  
**Return value:** `Task` that completes when cleanup finishes.  
**Throws:**  
- `ObjectDisposedException` if the engine has already been disposed.  
- Any exception thrown by the engine’s dispose logic is propagated.

### Initialize_Engine
**Purpose:** Measures the time to create and initialize a fresh plugin engine instance for each iteration.  
**Parameters:** None.  
**Return value:** `Task` that completes when the engine is fully initialized.  
**Throws:**  
- `InvalidOperationException` if initialization fails due to configuration errors.  
- `FileNotFoundException` if required plugin assemblies cannot be located.

### LoadAllPlugins_ThroughEngine
**Purpose:** Measures the time to load all known plugins via the engine’s public load API.  
**Parameters:** None.  
**Return value:** `Task` that completes when all plugins have been loaded.  
**Throws:**  
- `PluginLoadException` if any plugin fails to load.  
- `InvalidOperationException` if the engine is not initialized.

### GetHealthInfo
**Purpose:** Measures the time to retrieve health information from an idle engine (no plugins loaded).  
**Parameters:** None.  
**Return value:** `Task<HealthInfo>` containing the health snapshot.  
**Throws:**  
- `InvalidOperationException` if the engine is not initialized.  
- Any exception from the underlying health check is wrapped and rethrown.

### GetStatus
**Purpose:** Measures the time to obtain the current operational status of the engine.  
**Parameters:** None.  
**Return value:** `Task<EngineStatus>` indicating the status.  
**Throws:**  
- `InvalidOperationException` if the engine is not initialized.

### UnloadAllPlugins_ThroughEngine
**Purpose:** Measures the time to unload all currently loaded plugins via the engine’s unload API.  
**Parameters:** None.  
**Return value:** `Task` that completes when all plugins have been unloaded.  
**Throws:**  
- `PluginUnloadException` if any plugin fails to unload.  
- `InvalidOperationException` if the engine is not initialized or no plugins are loaded.

### GetLoadedPlugin_ThroughEngine
**Purpose:** Measures the time to retrieve a specific loaded plugin by its identifier.  
**Parameters:** None (the benchmark uses a predefined plugin ID set in `GlobalSetup`).  
**Return value:** `Task<PluginHandle>` representing the plugin handle.  
**Throws:**  
- `KeyNotFoundException` if the plugin with the given ID is not loaded.  
- `InvalidOperationException` if the engine is not initialized.

### Initialize_EngineWithExecution
**Purpose:** Measures the combined time to initialize the engine and execute a trivial plugin operation (e.g., invoking a no‑op method).  
**Parameters:** None.  
**Return value:** `Task` that completes after initialization and execution.  
**Throws:**  
- `InvalidOperationException` if initialization fails.  
- `PluginExecutionException` if the test plugin throws during execution.

### GetHealthInfo_WithPlugins
**Purpose:** Measures the time to retrieve health information while plugins are loaded.  
**Parameters:** None.  
**Return value:** `Task<HealthInfo>` containing the health snapshot under load.  
**Throws:**  
- `InvalidOperationException` if the engine is not initialized.  
- Any exception from the health check is propagated.

### SequentialPluginOperations
**Purpose:** Measures the time to perform a sequence of plugin‑related operations (load, get health, unload) in a deterministic order for each iteration.  
**Parameters:** None.  
**Return value:** `Task` that completes after the sequence finishes.  
**Throws:**  
- `InvalidOperationException` if the engine is not initialized at the start of the sequence.  
- Any exception from load, health check, or unload is propagated and stops the sequence.

## Usage

```csharp
using BenchmarkDotNet.Running;
using DotNetPluginEngine.Benchmarks;

// Run all benchmarks in the PluginEngineCoreBenchmarks class.
var summary = BenchmarkRunner.Run<PluginEngineCoreBenchmarks>();
```

```csharp
using BenchmarkDotNet.Running;
using DotNetPluginEngine.Benchmarks;

// Configure BenchmarkDotNet to run only the initialization benchmarks.
var config = DefaultConfig.Instance
    .WithOptions(ConfigOptions.DisableOptimizationsValidator);
var summary = BenchmarkRunner.Run<PluginEngineCoreBenchmarks>(config);
``` 

## Notes

- The benchmark class assumes a single shared engine instance for the lifetime of the benchmark run; `GlobalSetup` and `GlobalCleanup` are executed exactly once per process.  
- Individual benchmark methods are **not** thread‑safe; they must not be invoked concurrently because they mutate the engine state (load/unload plugins, re‑initialize the engine, etc.).  
- If a benchmark throws an exception, BenchmarkDotNet will mark the iteration as failed and continue with the next iteration unless the failure is deemed unrecoverable (e.g., engine disposal).  
- The `GetLoadedPlugin_ThroughEngine` benchmark relies on a plugin being loaded by a prior benchmark; running it in isolation without first executing a load benchmark will result in a `KeyNotFoundException`.  
- Execution time measurements include any asynchronous overhead; they do not include the time taken by `GlobalSetup` or `GlobalCleanup`.  
- The class does not inherit from any benchmark‑specific base type; it relies on the benchmarking framework’s convention of treating public `Task` or `Task<T>` methods as benchmark operations.  
- No static state is modified outside of the engine instance, so running multiple different benchmark classes in the same process is safe‑wide will not interfere, provided each class manages its own engine lifecycle.
