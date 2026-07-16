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
