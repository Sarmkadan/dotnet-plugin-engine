# DependencyResolutionBenchmarks

Provides a set of benchmark methods for measuring the performance of the dependency resolution engine under various graph topologies and constraint scenarios. The class is intended to be used with a benchmarking framework (e.g., BenchmarkDotNet) to compare resolution algorithms, detect regressions, and evaluate the impact of changes to the plugin engine’s resolver.

## API

### `public void GlobalSetup`
Prepares shared state required by all resolution benchmarks. This method initializes the plugin container, registers common services, and loads any baseline metadata needed for the subsequent scenario methods. It must be invoked before any `Resolve_*` method is called; otherwise the resolver operates on an uninitialized container and may throw.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if called after a benchmark iteration has already begun or if the underlying container cannot be created.

### `public void Resolve_EmptyGraph`
Measures resolution performance when the dependency graph contains no edges (i.e., each plugin has no dependencies). This scenario tests the overhead of traversing an empty graph and returning immediate results.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if `GlobalSetup` has not been called.  
  - `DependencyResolutionException` if the resolver encounters an unexpected state (should not occur in a correctly configured empty graph).

### `public void Resolve_LinearChain`
Evaluates resolution time for a linear depth *n* depends‑on relationships that form a single straight line (A → B → C → …). The length of the chain is defined by the benchmark configuration and exercises iterative depth‑first traversal.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if `GlobalSetup` is missing.  
  - `DependencyResolutionException` if a cycle is erroneously introduced or a node cannot be resolved.

### `public void Resolve_DiamondPattern`
Benchmarks the resolver on a classic diamond dependency graph (A depends on B and C; both B and C depend on D). This tests handling of shared dependencies and ensures that each node is resolved only once.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if `GlobalSetup` has not been run.  
  - `DependencyResolutionException` if the shared dependency cannot be satisfied.

### `public void Resolve_CircularDependency`
Measures performance when the resolver encounters a simple circular dependency (A → B → A). The benchmark expects the resolver to detect the cycle and either break it according to policy or throw an appropriate exception.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if `GlobalSetup` is absent.  
  - `CircularDependencyException` (or derived) when the resolver detects the loop, which is the expected outcome for this scenario.

### `public void Resolve_LargeGraph`
Stress‑tests the resolver with a densely connected graph containing many nodes and edges (size configurable via benchmark parameters). This scenario evaluates memory usage and algorithmic scalability.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if `GlobalSetup` has not been invoked.  
  - `OutOfMemoryException` in extreme cases where the graph exceeds available memory (propagated from internal allocations).  
  - `DependencyResolutionException` for any resolution failure unrelated to size.

### `public void Resolve_VersionConstraints`
Assesses the impact of version range evaluation on resolution time. Plugins are registered with overlapping version constraints, requiring the resolver to perform compatibility checks.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if `GlobalSetup` is missing.  
  - `VersionResolutionException` when no compatible version set can be found (expected in certain parameterized runs).  
  - `DependencyResolutionException` for other resolution errors.

### `public void Resolve_PluginMetadataDependencies`
Benchmarks resolution that depends on plugin metadata attributes (e.g., capabilities, features) rather than plain assembly references. This tests the metadata lookup subsystem alongside the core graph traversal.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if `GlobalSetup` has not been called.  
  - `MetadataResolutionException` when required metadata is missing or malformed.  
  - `DependencyResolutionException` for general resolution problems.

### `public void Resolve_CircularDependencyDeep`
Similar to `Resolve_CircularDependency` but with a longer, deeper cycle (e.g., A → B → C → … → Z → A). This measures the resolver’s ability to detect cycles in larger strongly‑connected components.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if `GlobalSetup` is absent.  
  - `CircularDependencyException` (or derived) upon cycle detection.  
  - `DependencyResolutionException` for any other resolution failure.

### `public void Resolve_MissingDependencies`
Evaluates behavior when a plugin declares a dependency that cannot be satisfied by any registered plugin. The benchmark expects the resolver to report a missing dependency according to its error‑handling policy.

- **Parameters:** None  
- **Return:** `void`  
- **Throws:**  
  - `InvalidOperationException` if `GlobalSetup` has not been run.  
  - `MissingDependencyException` (or derived) when the unsatisfied dependency is encountered, which is the anticipated result.  
  - `DependencyResolutionException` for unrelated errors.

## Usage

### Example 1: Running the benchmarks with BenchmarkDotNet
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DotNetPluginEngine.Benchmarks;

[MemoryDiagnoser]
public class Program
{
    private static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<DependencyResolutionBenchmarks>();
    }
}
```
The `DependencyResolutionBenchmarks` class is discovered by BenchmarkDotNet; its `GlobalSetup` method is automatically invoked before each benchmark iteration, and each `Resolve_*` method is measured individually.

### Example 2: Manual invocation for ad‑hoc testing
```csharp
using DotNetPluginEngine.Benchmarks;

var benchmarks = new DependencyResolutionBenchmarks();

// Prepare the shared state once.
benchmarks.GlobalSetup();

// Execute a specific scenario and measure elapsed time manually.
var sw = System.Diagnostics.Stopwatch.StartNew();
benchmarks.Resolve_LinearChain();
sw.Stop();

Console.WriteLine($"Linear chain resolution took {sw.ElapsedMilliseconds} ms");
```
In this approach, the caller is responsible for calling `GlobalSetup` exactly once before any resolution method, and for handling any exceptions that the resolver may throw.

## Notes
- The class is **not thread‑safe**. `GlobalSetup` mutates internal container state that is shared across all `Resolve_*` methods. Concurrent calls from multiple threads without external synchronization can lead to race conditions, inconsistent state, or incorrect measurements.
- Each `Resolve_*` method assumes that `GlobalSetup` has been called successfully in the current thread or invocation context. Calling a resolve method prior to setup will result in an `InvalidOperationException`.
- The benchmarks do not clean up registered plugins or metadata between iterations; the container state persists for the lifetime of the instance. If isolation between runs is required, instantiate a new `DependencyResolutionBenchmarks` object for each test scenario.
- Exception types mentioned (e.g., `CircularDependencyException`, `MissingDependencyException`, `VersionResolutionException`) are those thrown by the underlying resolver in the `dotnet-plugin-engine` codebase; the benchmark methods themselves only propagate these exceptions.
- Memory consumption reported by benchmarks reflects allocations made during resolution, including temporary graph traversal structures. The `Resolve_LargeGraph` scenario may trigger `OutOfMemoryException` if the configured graph size exceeds the process’s available memory; this is considered a valid outcome for stress‑testing.
