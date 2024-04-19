# PluginDiscoveryBenchmarks

`PluginDiscoveryBenchmarks` is a benchmark suite designed to evaluate the performance and behavior of plugin discovery mechanisms within the `dotnet-plugin-engine` framework. It provides methods to simulate various plugin loading scenarios, including empty directories, varying plugin counts, metadata retrieval, and validation/filtering operations. This class is typically used in performance testing to identify bottlenecks and optimize plugin discovery workflows.

## API

### `GlobalSetup`

Initializes the necessary resources and environment required for executing plugin discovery benchmarks. This method must be called before any benchmark operations to ensure consistent test conditions.

- **Parameters**: None  
- **Return Value**: `Task`  
- **Exceptions**: May throw exceptions if resource initialization fails (e.g., file system access errors, configuration issues).

### `GlobalCleanup`

Releases resources allocated during `GlobalSetup` and resets the environment to its pre-benchmark state. Ensures no residual state affects subsequent tests.

- **Parameters**: None  
- **Return Value**: `void`  
- **Exceptions**: May throw exceptions if resource disposal fails (e.g., locked files, invalid handles).

### `Discover_EmptyDirectory`

Benchmarks the performance of discovering plugins in an empty directory. Measures baseline overhead for directory scanning and plugin enumeration logic.

- **Parameters**: None  
- **Return Value**: `Task`  
- **Exceptions**: Throws if the target directory does not exist or is inaccessible.

### `Discover_50Plugins`

Benchmarks plugin discovery with a directory containing 50 plugins. Evaluates scalability and performance under moderate load.

- **Parameters**: None  
- **Return Value**: `Task`  
- **Exceptions**: Throws if the target directory or plugin files are missing or corrupted.

### `Discover_200Plugins`

Benchmarks plugin discovery with a directory containing 200 plugins. Tests performance under high-load conditions to identify potential bottlenecks.

- **Parameters**: None  
- **Return Value**: `Task`  
- **Exceptions**: Throws if the target directory or plugin files are missing or corrupted.

### `GetPluginMetadata_Single`

Retrieves metadata for a single plugin. Used to benchmark metadata extraction performance and validate individual plugin configurations.

- **Parameters**: None  
- **Return Value**: `Task`  
- **Exceptions**: Throws if the plugin file is invalid or metadata cannot be parsed.

### `ValidatePluginFiles`

Validates all plugin files in the configured directory. Checks for structural integrity and compatibility without loading plugins into memory.

- **Parameters**: None  
- **Return Value**: `Task`  
- **Exceptions**: Throws if file validation encounters critical errors (e.g., missing dependencies, invalid formats).

### `FilterValidPlugins`

Filters discovered plugins to retain only valid entries based on predefined criteria (e.g., metadata validity, file integrity). Benchmarks filtering efficiency.

- **Parameters**: None  
- **Return Value**: `Task`  
- **Exceptions**: Throws if filtering logic encounters unexpected data states (e.g., null references, invalid metadata).

## Usage

```csharp
// Example 1: Running a single benchmark scenario
var benchmark = new PluginDiscoveryBenchmarks();
await benchmark.GlobalSetup();
try
{
    await benchmark.Discover_50Plugins();
}
finally
{
    benchmark.GlobalCleanup();
}
```

```csharp
// Example 2: Iterating through multiple benchmark scenarios
var benchmark = new PluginDiscoveryBenchmarks();
await benchmark.GlobalSetup();

try
{
    await benchmark.Discover_EmptyDirectory();
    await benchmark.Discover_50Plugins();
    await benchmark.Discover_200Plugins();
    await benchmark.GetPluginMetadata_Single();
    await benchmark.ValidatePluginFiles();
    await benchmark.FilterValidPlugins();
}
finally
{
    benchmark.GlobalCleanup();
}
```

## Notes

- **Edge Cases**:  
  - `Discover_EmptyDirectory` may throw if the target directory path is misconfigured or inaccessible.  
  - `Discover_50Plugins` and `Discover_200Plugins` assume preconfigured plugin directories; missing files will cause exceptions.  
  - `FilterValidPlugins` behavior is undefined if no valid plugins exist in the configured directory.  

- **Thread Safety**:  
  - Methods are not thread-safe. Concurrent calls to `GlobalSetup` or `GlobalCleanup` may lead to race conditions or resource conflicts.  
  - Instance methods operate on shared state; external synchronization is required for parallel execution.  
  - `GlobalCleanup` must not be called concurrently with other benchmark methods to prevent premature resource disposal.
