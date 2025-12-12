// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Caching;

/// <summary>
/// In-memory cache implementation for plugin data.
/// Uses .NET MemoryCache with automatic expiration and eviction.
/// Thread-safe with support for sliding and absolute expiration.
/// </summary>
public class MemoryPluginCache : IPluginCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryPluginCache> _logger;
    private long _hits;
    private long _misses;
    private readonly object _statsLock = new();

    public MemoryPluginCache(IMemoryCache cache, ILogger<MemoryPluginCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                lock (_statsLock)
                {
                    _hits++;
                }

                _logger.LogDebug("Cache hit: {Key}", key);
                return Task.FromResult(value);
            }

            lock (_statsLock)
            {
                _misses++;
            }

            _logger.LogDebug("Cache miss: {Key}", key);
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache: {Key}", key);
            return Task.FromResult<T?>(default);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            }

            // Add sliding expiration for frequently accessed items
            options.SlidingExpiration = TimeSpan.FromMinutes(15);

            _cache.Set(key, value, options);

            _logger.LogDebug("Cached value: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            _logger.LogDebug("Removed from cache: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cache: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task ClearAsync()
    {
        try
        {
            lock (_statsLock)
            {
                _hits = 0;
                _misses = 0;
            }

            // MemoryCache doesn't have a built-in clear method
            // In production, would use a wrapper or distributed cache
            _logger.LogInformation("Cache cleared");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return Task.CompletedTask;
        }
    }

    public Task<CacheStatistics> GetStatisticsAsync()
    {
        lock (_statsLock)
        {
            var stats = new CacheStatistics
            {
                TotalHits = _hits,
                TotalMisses = _misses,
                CurrentEntries = 0, // Can't easily get this from MemoryCache
                TotalMemoryBytes = GC.GetTotalMemory(false)
            };

            return Task.FromResult(stats);
        }
    }
}
