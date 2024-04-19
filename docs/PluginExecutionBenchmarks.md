# PluginExecutionBenchmarks

`PluginExecutionBenchmarks` is a benchmarking harness for the `dotnet-plugin-engine` runtime, designed to measure and validate the performance characteristics of plugin lifecycle operations. It provides a controlled environment for executing initialization, cleanup, single-operation, batch-operation, detail retrieval, health-check, and combined lifecycle scenarios against the plugin engine, enabling developers to establish baseline metrics and detect regressions.

## API

### GlobalSetup
```csharp
public async Task GlobalSetup()
```
Prepares the shared benchmarking infrastructure before any iteration runs. This method is invoked once per benchmark session and is responsible for initializing the plugin engine host, loading required assemblies, and establishing any global state that all benchmark methods depend on. It returns a `Task` representing the asynchronous setup operation. Throws `InvalidOperationException` if the engine cannot be initialized or required dependencies are missing.

### GlobalCleanup
```csharp
public async Task GlobalCleanup()
```
Tears down the shared infrastructure after all benchmark iterations have completed. This method disposes of the plugin engine host, releases unmanaged resources, and resets global state. It returns a `Task` representing the asynchronous cleanup operation. Throws `ObjectDisposedException` if called on an already-disposed context; implementations should guard against double-disposal.

### PluginLifecycle_Initialize
```csharp
public async Task PluginLifecycle_Initialize()
```
Benchmarks the initialization phase of a plugin’s lifecycle. This includes loading the plugin assembly, resolving its dependencies, and invoking its startup logic. The method returns a `Task` that completes when initialization finishes. Throws `PluginLoadException` if the target plugin cannot be found or its dependencies fail to resolve, and `TimeoutException` if initialization exceeds the configured time limit.

### PluginLifecycle_Cleanup
```csharp
public async Task PluginLifecycle_Cleanup()
```
Benchmarks the cleanup phase of a plugin’s lifecycle. This covers shutting down the plugin, releasing its resources, and unloading its assembly. The method returns a `Task` that completes when cleanup finishes. Throws `PluginUnloadException` if the plugin refuses to shut down or leaves dangling resources, and `TimeoutException` if cleanup exceeds the configured time limit.

### ExecutePluginOperation_Single
```csharp
public async Task ExecutePluginOperation_Single()
```
Measures the execution time of a single, isolated plugin operation. The operation is dispatched to an already-initialized plugin and the method awaits its completion. Returns a `Task` representing the asynchronous execution. Throws `PluginExecutionException` if the operation fails during processing, and `InvalidOperationException` if no plugin has been initialized prior to calling this method.

### ExecuteOperations_Batch
```csharp
public async Task ExecuteOperations_Batch()
```
Benchmarks the execution of multiple plugin operations in a batch, measuring throughput and aggregate latency. The operations are dispatched sequentially or concurrently depending on the engine configuration. Returns a `Task` that completes when all operations in the batch have finished. Throws `AggregateException` wrapping individual `PluginExecutionException` instances if one or more operations fail, and `InvalidOperationException` if the plugin is not in a valid state for batch execution.

### GetPluginDetails
```csharp
public async Task GetPluginDetails()
```
Benchmarks the retrieval of metadata and detail information from a loaded plugin. This includes querying version, capabilities, configuration, and runtime state. Returns a `Task` that completes when the details have been fetched. Throws `PluginQueryException` if the plugin does not respond to detail requests or returns malformed data, and `TimeoutException` if the query exceeds the configured time limit.

### CheckPluginHealth
```csharp
public async Task CheckPluginHealth()
```
Benchmarks a health-check probe against a running plugin. The method sends a diagnostic request and awaits a status response indicating whether the plugin is healthy, degraded, or unavailable. Returns a `Task` representing the asynchronous health-check operation. Throws `PluginHealthException` if the plugin reports an unhealthy state or fails to respond, and `TimeoutException` if the health check times out.

### MultipleLifecycleOperations
```csharp
public async Task MultipleLifecycleOperations()
```
Benchmarks a composite scenario that interleaves multiple lifecycle operations—such as initialize, execute, health-check, and cleanup—within a single iteration. This method measures the overhead of state transitions and repeated setup/teardown cycles. Returns a `Task` that completes when all lifecycle operations have finished. Throws `AggregateException` wrapping any exceptions thrown by individual lifecycle steps, and `InvalidOperationException` if the engine enters an unrecoverable state during the sequence.

## Usage

### Example 1: Running a single-operation benchmark with BenchmarkDotNet
```csharp
using BenchmarkDotNet.Running;

public static class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<PluginExecutionBenchmarks>();
        Console.WriteLine($"Mean single-op time: {summary.Reports.First().ResultStatistics.Mean} ns");
    }
}
```
This example delegates execution to BenchmarkDotNet, which calls `GlobalSetup` once, then repeatedly invokes `ExecutePluginOperation_Single` to collect timing samples, and finally calls `GlobalCleanup`.

### Example 2: Manual invocation for integration testing
```csharp
var benchmarks = new PluginExecutionBenchmarks();

try
{
    await benchmarks.GlobalSetup();

    await benchmarks.PluginLifecycle_Initialize();
    await benchmarks.ExecutePluginOperation_Single();
    await benchmarks.CheckPluginHealth();
    await benchmarks.GetPluginDetails();
    await benchmarks.ExecuteOperations_Batch();
    await benchmarks.MultipleLifecycleOperations();
    await benchmarks.PluginLifecycle_Cleanup();
}
finally
{
    await benchmarks.GlobalCleanup();
}
```
This pattern is useful outside of automated benchmarking runners, such as in continuous-integration smoke tests where each lifecycle phase is exercised explicitly and failures are inspected individually.

## Notes

- **Execution order dependency**: `ExecutePluginOperation_Single`, `ExecuteOperations_Batch`, `GetPluginDetails`, and `CheckPluginHealth` assume a plugin has been initialized via `PluginLifecycle_Initialize`. Calling them before initialization throws `InvalidOperationException`.
- **State leakage between iterations**: When used with a benchmarking framework, `GlobalSetup` and `GlobalCleanup` are called once, while individual benchmark methods may be invoked many times. Implementations must ensure that iterative methods do not mutate shared state in ways that skew subsequent measurements (e.g., exhausting a connection pool or accumulating in-memory logs).
- **Timeout behavior**: Several methods document `TimeoutException`. The default timeout values are engine-configurable; benchmarks should be tuned so that timeouts do not fire during normal operation, otherwise results will be discarded as outliers.
- **Thread safety**: The public members are designed to be called sequentially by a benchmarking host. They are not thread-safe for concurrent invocation from multiple threads. Parallel execution of `ExecuteOperations_Batch` refers to internal dispatch of operations within the plugin engine, not concurrent calls to the benchmark method itself.
- **AggregateException handling**: `ExecuteOperations_Batch` and `MultipleLifecycleOperations` may throw `AggregateException`. Callers should flatten and inspect inner exceptions to determine which specific operation or lifecycle step failed.
- **Resource cleanup guarantees**: `GlobalCleanup` should be called in a `finally` block or via an `IDisposable` pattern when used outside of a benchmarking runner. Failure to call `GlobalCleanup` may leave plugin assemblies loaded and native resources allocated until process exit.
