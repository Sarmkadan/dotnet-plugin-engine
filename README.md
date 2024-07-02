## PluginException

The `PluginException` class represents a custom exception thrown by the plugin engine. It provides additional context and information about the error, including an error code, entity ID, and context dictionary.

### Usage Example

```csharp
try
{
    // Code that may throw a PluginException
}
catch (PluginException ex)
{
    Console.WriteLine($"Error code: {ex.ErrorCode}");
    Console.WriteLine($"Entity ID: {ex.EntityId}");
    Console.WriteLine($"Context: {ex.Context}");
    Console.WriteLine(ex.ToString());
}
```

## VersionMismatchException

The `VersionMismatchException` is thrown when version constraints between components are not satisfied. It provides detailed information about the expected and actual versions, along with the component type and name that caused the mismatch.

### Usage Example

```csharp
using PluginEngine.Exceptions;

// Validate plugin version compatibility
if (plugin.Version != expectedVersion)
{
    throw new VersionMismatchException(
        message: $"Plugin version mismatch detected",
        expectedVersion: expectedVersion,
        actualVersion: plugin.Version,
        componentType: "Plugin",
        componentName: plugin.Name
    );
}

// Or when validating assembly dependencies
if (assembly.GetName().Version?.ToString() != requiredAssemblyVersion)
{
    throw new VersionMismatchException(
        message: $"Assembly version constraint violated",
        expectedVersion: requiredAssemblyVersion,
        actualVersion: assembly.GetName().Version?.ToString() ?? "unknown",
        componentType: "Assembly",
        componentName: assembly.GetName().Name ?? "unknown"
    );
}

// Catch and handle the exception
try
{
    pluginEngine.LoadPlugin(pluginPath);
}
catch (VersionMismatchException ex)
{
    Console.WriteLine($"Version mismatch error: {ex.Message}");
    Console.WriteLine(ex.ToString());
    Console.WriteLine($"Expected: {ex.ExpectedVersion}");
    Console.WriteLine($"Actual: {ex.ActualVersion}");
    Console.WriteLine($"Component: {ex.ComponentType} - {ex.ComponentName}");
}
```

## PluginDiscoveryBenchmarks

The `PluginDiscoveryBenchmarks` class evaluates the performance of the plugin engine's discovery logic across various scenarios. It includes benchmarks for discovering plugins in empty directories, large plugin sets, and validating plugin files.

### Usage Example

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class PluginDiscoveryBenchmarks
{
    [Benchmark]
    public async Task GlobalSetup()
    {
        await new PluginDiscoveryBenchmarks().GlobalSetup();
    }

    [Benchmark]
    public async Task Discover_EmptyDirectory()
    {
        await new PluginDiscoveryBenchmarks().Discover_EmptyDirectory();
    }

    [Benchmark]
    public async Task Discover_50Plugins()
    {
        await new PluginDiscoveryBenchmarks().Discover_50Plugins();
    }

    [Benchmark]
    public async Task Discover_200Plugins()
    {
        await new PluginDiscoveryBenchmarks().Discover_200Plugins();
    }

    [Benchmark]
    public async Task GetPluginMetadata_Single()
    {
        await new PluginDiscoveryBenchmarks().GetPluginMetadata_Single();
    }

    [Benchmark]
    public void ValidatePluginFiles()
    {
        new PluginDiscoveryBenchmarks().ValidatePluginFiles();
    }

    [Benchmark]
    public void FilterValidPlugins()
    {
        new PluginDiscoveryBenchmarks().FilterValidPlugins();
    }
}
```

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

## PluginEngine

The `PluginEngine` class is the core component of the plugin engine, responsible for managing plugins, executing operations, and providing status information.

### Usage Example

```csharp
using DotNetPluginEngine;

var engine = new PluginEngine();

await engine.InitializeAsync();

await engine.LoadAllPluginsAsync();

var status = await engine.GetStatusAsync();

Console.WriteLine(status);

await engine.UnloadAllPluginsAsync();

var healthInfo = await engine.GetHealthInfoAsync();

Console.WriteLine(healthInfo);
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