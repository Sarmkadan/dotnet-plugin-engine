# PluginDiscoveryBenchmarksExtensions

Provides extension methods for executing benchmark suites that measure the performance of plugin discovery and metadata extraction operations in the **dotnet-plugin-engine** library. These methods are intended for use in performance testing scenarios where consistent, repeatable measurements of discovery-related workloads are required.

## API

### RunDiscoveryBenchmarks
**Purpose**: Runs a series of benchmarks that measure the time taken to discover plugins using a supplied discovery mechanism.  
**Parameters**:  
- `discoveryProvider`: An instance implementing `IPluginDiscovery` that performs the actual plugin discovery.  
- `options` (optional): A `BenchmarkOptions` object that configures iteration count, warm‑up period, and reporting details.  
- `cancellationToken` (optional): A `System.Threading.CancellationToken` used to cancel the benchmark execution.  
**Return Value**: A `Task` that completes when all benchmark iterations have finished. The task does not return a value; results are typically logged or written to an output sink configured via `options`.  
**Exceptions**:  
- `ArgumentNullException` if `discoveryProvider` is `null`.  
- `InvalidOperationException` if the discovery provider cannot be initialized or enters an invalid state during execution.  
- Any exception thrown by the discovery provider’s `DiscoverAsync` method is propagated unchanged.

### RunMetadataBenchmarks
**Purpose**: Executes benchmarks focused on measuring the performance of metadata extraction from discovered plugins.  
**Parameters**:  
- `metadataExtractor`: An instance implementing `IPluginMetadataExtractor` used to read plugin metadata.  
- `options` (optional): A `BenchmarkOptions` object controlling benchmark configuration.  
- `cancellationToken` (optional): A `System.Threading.CancellationToken` for cancellation support.  
**Return Value**: A `Task` that completes when the metadata extraction benchmarks finish.  
**Exceptions**:  
- `ArgumentNullException` if `metadataExtractor` is `null`.  
- `InvalidOperationException` if the extractor fails to load metadata for a plugin.  
- Exceptions from the extractor’s `ExtractMetadataAsync` method are propagated.

### RunFullDiscoveryBenchmark
**Purpose**: Performs an end‑to‑end benchmark that combines plugin discovery and metadata extraction in a single workflow.  
**Parameters**:  
- `discoveryProvider`: An `IPluginDiscovery` implementation for locating plugins.  
- `metadataExtractor`: An `IPluginMetadataExtractor` implementation for reading metadata from discovered plugins.  
- `options` (optional): A `BenchmarkOptions` object to tune the benchmark run.  
- `cancellationToken` (optional): A `System.Threading.CancellationToken` for cancellation.  
**Return Value**: A `Task` that completes when the full discovery‑and‑metadata benchmark suite has executed.  
**Exceptions**:  
- `ArgumentNullException` if either `discoveryProvider` or `metadataExtractor` is `null`.  
- `InvalidOperationException` if any stage of the combined workflow fails.  
- Exceptions from either the discovery or extraction steps are propagated.

### RunConcurrentDiscoveryBenchmarks
**Purpose**: Runs discovery benchmarks with multiple concurrent invocations to assess scalability and thread‑safety of the discovery provider under load.  
**Parameters**:  
- `discoveryProvider`: An `IPluginDiscovery` implementation to be tested concurrently.  
- `degreeOfParallelism`: An `int` specifying the number of concurrent discovery operations to execute.  
- `options` (optional): A `BenchmarkOptions` object for benchmark configuration.  
- `cancellationToken` (optional): A `System.Threading.CancellationToken` for cancellation.  
**Return Value**: A `Task` that completes when all concurrent benchmark iterations have finished.  
**Exceptions**:  
- `ArgumentNullException` if `discoveryProvider` is `null`.  
- `ArgumentOutOfRangeException` if `degreeOfParallelism` is less than 1.  
- `InvalidOperationException` if the discovery provider cannot support concurrent calls (e.g., internal state is not thread‑safe).  
- Any exception from the discovery provider are propagated.

## Usage

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.PluginEngine.Discovery;
using DotNet.PluginEngine.Benchmarks;

public class Program
{
    public static async Task Main()
    {
        // Assume we have a concrete discovery implementation.
        IPluginDiscovery discovery = new AssemblyPluginDiscovery(AppDomain.CurrentDomain.BaseDirectory);

        // Run basic discovery benchmarks with default options.
        await PluginDiscoveryBenchmarksExtensions.RunDiscoveryBenchmarks(
            discoveryProvider: discovery,
            cancellationToken: CancellationToken.None);

        Console.WriteLine("Discovery benchmarks completed.");
    }
}
```

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.PluginEngine.Discovery;
using DotNet.PluginEngine.Metadata;
using DotNet.PluginEngine.Benchmarks;

public class BenchmarkHarness
{
    public static async Task RunAllBenchmarks()
    {
        var discovery = new AssemblyPluginDiscovery("plugins");
        var metadata = new JsonMetadataExtractor();

        // Execute a full discovery + metadata benchmark suite.
        await PluginDiscoveryBenchmarksExtensions.RunFullDiscoveryBenchmark(
            discoveryProvider: discovery,
            metadataExtractor: metadata,
            options: new BenchmarkOptions { IterationCount = 5, WarmUpIterations = 1 },
            cancellationToken: CancellationToken.None);

        // Test concurrent discovery with a degree of parallelism matching the environment.
        await PluginDiscoveryBenchmarksExtensions.RunConcurrentDiscoveryBenchmarks(
            discoveryProvider: discovery,
            degreeOfParallelism: Environment.ProcessorCount,
            options: new BenchmarkOptions { IterationCount = 3 },
            cancellationToken: CancellationToken.None);
    }
}
```

## Notes

- All methods are **static** and do not retain state between invocations; thread‑safety depends entirely on the supplied `discoveryProvider` and/or `metadataExtractor` instances. If those objects are not thread‑safe, calling the benchmark methods concurrently from multiple threads may result in undefined behavior or exceptions.
- Passing `null` for any required argument will cause an immediate `ArgumentNullException` before any asynchronous work begins.
- The benchmark methods swallow no exceptions; any failure in the underlying discovery or metadata extraction logic is propagated to the caller, allowing test harnesses to capture and report failures as needed.
- Cancellation is cooperative: if the supplied `CancellationToken` is triggered, the methods will attempt to halt further benchmark iterations and exit as soon as the current operation completes. Work already in progress will continue to completion unless the underlying APIs themselves honor the token.
- For meaningful results, ensure that the plugin directories or assemblies supplied to the discovery provider remain unchanged for the duration of the benchmark run, as modifications during execution can introduce measurement noise.
