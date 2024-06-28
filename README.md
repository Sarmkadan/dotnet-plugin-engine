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

