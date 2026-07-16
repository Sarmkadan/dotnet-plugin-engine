# dotnet-plugin-engine

[... existing content ...]

## MemoryPluginCache

The `MemoryPluginCache` class provides an in-memory cache implementation for plugin data, supporting automatic expiration and eviction. It stores data in memory using .NET's `IMemoryCache` and provides thread-safe operations for getting, setting, removing, and clearing cache entries.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class CacheDemo
{
    private readonly MemoryPluginCache _cache;
    private readonly ILogger<CacheDemo> _logger;

    public CacheDemo(MemoryPluginCache cache, ILogger<CacheDemo> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        // Get a value from cache
        var cachedValue = await _cache.GetAsync<string>("my-key");
        _logger.LogInformation($"Cached value: {cachedValue}");

        // Set a value in cache with expiration
        await _cache.SetAsync("my-key", "Hello, World!", TimeSpan.FromMinutes(30));

        // Remove a value from cache
        await _cache.RemoveAsync("my-key");

        // Clear entire cache
        await _cache.ClearAsync();

        // Get cache statistics
        var stats = await _cache.GetStatisticsAsync();
        _logger.LogInformation($"Cache hits: {stats.TotalHits}, misses: {stats.TotalMisses}, entries: {stats.CurrentEntries}");
    }
}
```

The `MemoryPluginCache` ensures efficient data caching for plugins with features like automatic expiration, sliding expiration, and detailed performance statistics.

```csharp

```

## DependencyGraphAnalyzer

`DependencyGraphAnalyzer` helps visualise and analyse plugin dependency graphs. It can generate a textual graph representation, produce a detailed `DependencyAnalysisReport`, and find plugins that depend on a given plugin.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PluginEngine.Utils.Helpers;

public class AnalyzerDemo
{
    private readonly DependencyGraphAnalyzer _analyzer;

    public AnalyzerDemo(IDependencyResolutionService resolver, ILogger<DependencyGraphAnalyzer> logger)
    {
        _analyzer = new DependencyGraphAnalyzer(resolver, logger);
    }

    public async Task RunAsync(Plugin rootPlugin, IEnumerable<Plugin> allPlugins)
    {
        // Visualise the dependency graph
        string graph = await _analyzer.GenerateGraphVisualizationAsync(rootPlugin);
        Console.WriteLine(graph);

        // Analyse the plugin's dependencies
        DependencyAnalysisReport report = await _analyzer.AnalyzeAsync(rootPlugin);
        Console.WriteLine($"Plugin: {report.PluginName}");
        Console.WriteLine($"Complexity: {report.GetComplexityLevel()}");
        Console.WriteLine($"Issues: {string.Join(\", \", report.Issues)}");

        // Find plugins that depend on the root plugin
        List<Guid> dependents = await _analyzer.FindDependentsAsync(allPlugins, rootPlugin.Id);
        Console.WriteLine($"Number of dependents: {dependents.Count}");
    }
}
```
