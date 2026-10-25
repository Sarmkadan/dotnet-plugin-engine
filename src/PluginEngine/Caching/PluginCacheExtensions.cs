#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Caching;

/// <summary>
/// Provides convenience extension methods for <see cref="IPluginCache"/>.
/// </summary>
public static class PluginCacheExtensions
{
    /// <summary>
    /// Returns the cached value for <paramref name="key"/>. If no value is cached, invokes
    /// <paramref name="factory"/>, stores its result in the cache and returns it.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="cache">The cache to read from and write to.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The delegate that produces the value when it is not cached.</param>
    /// <param name="expiration">Optional expiration applied when the value is stored.</param>
    /// <returns>The cached or newly created value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cache"/> or <paramref name="factory"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <c>null</c>, empty or whitespace.</exception>
    public static async Task<T> GetOrCreateAsync<T>(
        this IPluginCache cache,
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var (found, cached) = await cache.TryGetAsync<T>(key);
        if (found)
        {
            return cached!;
        }

        var value = await factory();
        await cache.SetAsync(key, value, expiration);
        return value;
    }

    /// <summary>
    /// Attempts to read the cached value for <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="cache">The cache to read from.</param>
    /// <param name="key">The cache key.</param>
    /// <returns>
    /// A tuple where <c>Found</c> is <c>true</c> when a non-null value is cached, and <c>Value</c> holds that value
    /// (or the default for <typeparamref name="T"/> when not found).
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cache"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <c>null</c>, empty or whitespace.</exception>
    public static async Task<(bool Found, T? Value)> TryGetAsync<T>(this IPluginCache cache, string key)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = await cache.GetAsync<T>(key);
        return (value is not null, value);
    }
}
