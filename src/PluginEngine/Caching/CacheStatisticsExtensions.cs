#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Caching;

/// <summary>
/// Provides convenience extension methods for <see cref="CacheStatistics"/>.
/// </summary>
public static class CacheStatisticsExtensions
{
    /// <summary>
    /// Determines whether the cache is considered effective based on its hit rate.
    /// </summary>
    /// <param name="stats">The cache statistics to evaluate.</param>
    /// <param name="minHitRate">The minimum hit rate required for the cache to be considered effective. Defaults to 0.5.</param>
    /// <returns><c>true</c> if the cache hit rate meets or exceeds <paramref name="minHitRate"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stats"/> is <c>null</c>.</exception>
    public static bool IsEffective(this CacheStatistics stats, double minHitRate = 0.5)
    {
        ArgumentNullException.ThrowIfNull(stats);
        return stats.HitRate >= minHitRate;
    }

    /// <summary>
    /// Gets the total number of cache requests (hits plus misses).
    /// </summary>
    /// <param name="stats">The cache statistics to evaluate.</param>
    /// <returns>The total number of cache requests.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stats"/> is <c>null</c>.</exception>
    public static long GetTotalRequests(this CacheStatistics stats)
    {
        ArgumentNullException.ThrowIfNull(stats);
        return stats.TotalHits + stats.TotalMisses;
    }

    /// <summary>
    /// Returns a human-readable summary of the cache statistics.
    /// </summary>
    /// <param name="stats">The cache statistics to summarize.</param>
    /// <returns>A string summarizing the cache statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stats"/> is <c>null</c>.</exception>
    public static string ToSummaryString(this CacheStatistics stats)
    {
        ArgumentNullException.ThrowIfNull(stats);
        return $"Requests={GetTotalRequests(stats)}, Hits={stats.TotalHits}, Misses={stats.TotalMisses}, "
            + $"Entries={stats.CurrentEntries}, Memory={stats.TotalMemoryBytes} B, HitRate={stats.HitRate:P1}";
    }
}