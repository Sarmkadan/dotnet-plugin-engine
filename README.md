// ... existing content ...

## PluginLoadingBenchmarks

The `PluginLoadingBenchmarks` class provides a set of benchmarks for measuring the performance of plugin loading operations. It includes tests for loading a single plugin, loading all plugins from a directory, unloading and reloading plugins, and more.

### Usage Example

```csharp
var benchmark = new PluginLoadingBenchmarks();
await benchmark.GlobalSetup();
await benchmark.Load_SinglePlugin();
await benchmark.Load_AllPluginsFromDirectory();
await benchmark.Unload_Plugin();
await benchmark.Reload_Plugin();
await benchmark.GetAllLoadedPlugins();
await benchmark.IsPluginLoaded();
await benchmark.Load_PluginsConcurrently();
await benchmark.Load_WithDependencyResolution();
await benchmark.Load_WithValidation();
await benchmark.GlobalCleanup();
```
