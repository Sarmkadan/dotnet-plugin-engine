#nullable enable
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
public sealed class MemoryPluginCache : IPluginCache
{
    /// <summary>
    /// The default sliding expiration for cached entries.
    /// </summary>
    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromMinutes(15);

    /// <summary>
    /// The default absolute expiration for cached entries stored without an explicit lifetime.
    /// </summary>
    private static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromHours(1);

    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryPluginCache> _logger;
    private readonly ConcurrentDictionary<string, byte> _keys = new(StringComparer.Ordinal);
    private long _hits;
    private long _misses;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryPluginCache"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cache"/> or <paramref name="logger"/> is null.</exception>
    public MemoryPluginCache(IMemoryCache cache, ILogger<MemoryPluginCache> logger)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    /// <typeparam name="T">The expected type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value, or <c>default</c> if the key is not present.</returns>
    public Task<T?> GetAsync<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                Interlocked.Increment(ref _hits);
                _logger.LogDebug("Cache hit: {Key}", key);
                return Task.FromResult(value);
            }

            Interlocked.Increment(ref _misses);

            _logger.LogDebug("Cache miss: {Key}", key);
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache: {Key}", key);
            return Task.FromResult<T?>(default);
        }
    }

    /// <summary>
    /// Stores a value in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Optional lifetime of the entry. When <c>null</c>, the cache default applies.</param>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = DefaultAbsoluteExpiration;
            }

            // Add sliding expiration for frequently accessed items
            options.SlidingExpiration = DefaultSlidingExpiration;

            // Keep the key index in sync with the underlying cache so entry counts and
            // an explicit clear stay accurate even when entries expire on their own.
            options.RegisterPostEvictionCallback(static (evictedKey, _, _, state) =>
            {
                if (state is ConcurrentDictionary<string, byte> keys && evictedKey is string k)
                {
                    keys.TryRemove(k, out _);
                }
            }, _keys);

            _cache.Set(key, value, options);
            _keys[key] = 0;

            _logger.LogDebug("Cached value: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache: {Key}", key);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Removes the entry with the specified key from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    public Task RemoveAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
            _logger.LogDebug("Removed from cache: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cache: {Key}", key);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Removes all entries from the cache.
    /// </summary>
    public Task ClearAsync()
    {
        try
        {
            Interlocked.Exchange(ref _hits, 0);
            Interlocked.Exchange(ref _misses, 0);

            // IMemoryCache exposes no clear operation, so evict every key this instance owns.
            var removed = 0;
            foreach (var key in _keys.Keys)
            {
                _cache.Remove(key);
                if (_keys.TryRemove(key, out _))
                {
                    removed++;
                }
            }

            _logger.LogInformation("Cache cleared: {Count} entries removed", removed);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Gets a snapshot of the current cache statistics.
    /// </summary>
    /// <returns>The current <see cref="CacheStatistics"/>.</returns>
    public Task<CacheStatistics> GetStatisticsAsync()
    {
        var stats = new CacheStatistics
        {
            TotalHits = Interlocked.Read(ref _hits),
            TotalMisses = Interlocked.Read(ref _misses),
            CurrentEntries = _keys.Count,
            TotalMemoryBytes = GC.GetTotalMemory(false)
        };

        return Task.FromResult(stats);
    }
}
