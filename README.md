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

## AssemblyLoadContextInfoExtensions

`AssemblyLoadContextInfoExtensions` provides a collection of extension methods for `AssemblyLoadContextInfo` objects. These helpers let you inspect the load‑context’s memory consumption, age, inactivity period, health status, and even search for loaded assemblies by name pattern, making it easier to monitor and manage plugin lifetimes.

### Usage Example

```csharp
using System;
using PluginEngine.Domain.Entities; // Adjust the namespace if necessary

// Assume we have an AssemblyLoadContextInfo instance named ctxInfo
if (ctxInfo.HasMemoryExceeded())
{
    Console.WriteLine("Memory limit exceeded.");
}

double ageMinutes = ctxInfo.GetAgeInMinutes();
double inactivityMinutes = ctxInfo.GetInactivityMinutes();
bool isStale = ctxInfo.IsStale();
bool isHealthy = ctxInfo.IsHealthy();

Console.WriteLine($"Age: {ageMinutes:F2} min, Inactivity: {inactivityMinutes:F2} min");
Console.WriteLine($"Is stale: {isStale}, Is healthy: {isHealthy}");

string detailedReport = ctxInfo.GetDetailedStatusReport();
Console.WriteLine(detailedReport);

string memoryUsage = ctxInfo.GetMemoryUsageString();
Console.WriteLine($"Memory usage: {memoryUsage}");

foreach (var assemblyName in ctxInfo.FindAssembliesByPattern("MyPlugin.*"))
{
    Console.WriteLine($"Found assembly: {assemblyName}");
}
```
