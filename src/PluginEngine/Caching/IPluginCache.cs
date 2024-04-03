#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Caching;

/// <summary>
/// Defines caching contract for plugin-related operations.
/// Provides abstraction for implementing different caching backends.
/// </summary>
public interface IPluginCache
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Removes a cached value.
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Clears all cache entries.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Gets cache statistics for monitoring.
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();
}

/// <summary>
/// Cache statistics for monitoring and diagnostics.
/// </summary>
public sealed class CacheStatistics
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public int CurrentEntries { get; set; }
    public long TotalMemoryBytes { get; set; }

    public double HitRate => TotalHits + TotalMisses > 0
        ? (double)TotalHits / (TotalHits + TotalMisses)
        : 0;
}
