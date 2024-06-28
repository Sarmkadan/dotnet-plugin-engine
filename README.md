## PluginEngineCoreBenchmarks

The `PluginEngineCoreBenchmarks` class provides a set of benchmarking methods to measure the performance of the plugin engine. It includes methods to initialize the engine, load and unload plugins, and retrieve health and status information.

### Usage Example

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class PluginEngineBenchmarks
{
    [Benchmark]
    public async Task Initialize_Engine()
    {
        await new PluginEngineCoreBenchmarks().Initialize_Engine();
    }

    [Benchmark]
    public async Task LoadAllPlugins_ThroughEngine()
    {
        await new PluginEngineCoreBenchmarks().LoadAllPlugins_ThroughEngine();
    }

    [Benchmark]
    public async Task GetHealthInfo()
    {
        await new PluginEngineCoreBenchmarks().GetHealthInfo();
    }

    [Benchmark]
    public async Task UnloadAllPlugins_ThroughEngine()
    {
        await new PluginEngineCoreBenchmarks().UnloadAllPlugins_ThroughEngine();
    }

    [Benchmark]
    public async Task GetLoadedPlugin_ThroughEngine()
    {
        await new PluginEngineCoreBenchmarks().GetLoadedPlugin_ThroughEngine();
    }

    [Benchmark]
    public async Task Initialize_EngineWithExecution()
    {
        await new PluginEngineCoreBenchmarks().Initialize_EngineWithExecution();
    }

    [Benchmark]
    public async Task GetHealthInfo_WithPlugins()
    {
        await new PluginEngineCoreBenchmarks().GetHealthInfo_WithPlugins();
    }

    [Benchmark]
    public async Task SequentialPluginOperations()
    {
        await new PluginEngineCoreBenchmarks().SequentialPluginOperations();
    }
}
```

## DependencyResolutionBenchmarks

The `DependencyResolutionBenchmarks` class evaluates the performance of the plugin engine’s dependency resolution logic across a variety of graph structures. It includes benchmarks for empty graphs, linear chains, diamond patterns, circular dependencies, large graphs, version constraints, plugin metadata dependencies, deep circular dependencies, and missing dependencies.

### Usage Example

```csharp
using DotNetPluginEngine.Benchmarks;

var benchmarks = new DependencyResolutionBenchmarks();

// Optional: perform any global setup required by the benchmarks
benchmarks.GlobalSetup();

// Run individual benchmark methods
benchmarks.Resolve_EmptyGraph();
benchmarks.Resolve_LinearChain();
benchmarks.Resolve_DiamondPattern();
benchmarks.Resolve_CircularDependency();
benchmarks.Resolve_LargeGraph();
benchmarks.Resolve_VersionConstraints();
benchmarks.Resolve_PluginMetadataDependencies();
benchmarks.Resolve_CircularDependencyDeep();
benchmarks.Resolve_MissingDependencies();
```

## PluginExecutionBenchmarks

The `PluginExecutionBenchmarks` class provides a set of benchmarking methods to measure the performance of the plugin engine's execution logic. It includes methods to initialize and cleanup the engine, execute plugin operations, retrieve plugin details, and check plugin health.

### Usage Example

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class PluginExecutionBenchmarks
{
    [Benchmark]
    public async Task GlobalSetup()
    {
        await new PluginExecutionBenchmarks().GlobalSetup();
    }

    [Benchmark]
    public async Task PluginLifecycle_Initialize()
    {
        await new PluginExecutionBenchmarks().PluginLifecycle_Initialize();
    }

    [Benchmark]
    public async Task PluginLifecycle_Cleanup()
    {
        await new PluginExecutionBenchmarks().PluginLifecycle_Cleanup();
    }

    [Benchmark]
    public async Task ExecutePluginOperation_Single()
    {
        await new PluginExecutionBenchmarks().ExecutePluginOperation_Single();
    }

    [Benchmark]
    public async Task ExecuteOperations_Batch()
    {
        await new PluginExecutionBenchmarks().ExecuteOperations_Batch();
    }

    [Benchmark]
    public async Task GetPluginDetails()
    {
        await new PluginExecutionBenchmarks().GetPluginDetails();
    }

    [Benchmark]
    public async Task CheckPluginHealth()
    {
        await new PluginExecutionBenchmarks().CheckPluginHealth();
    }

    [Benchmark]
    public async Task MultipleLifecycleOperations()
    {
        await new PluginExecutionBenchmarks().MultipleLifecycleOperations();
    }
}
```