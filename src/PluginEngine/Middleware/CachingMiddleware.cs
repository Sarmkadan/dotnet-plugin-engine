#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Middleware;

/// <summary>
/// Middleware for caching plugin operation results.
/// Caches successful operations to reduce repeated processing.
/// Automatically invalidates cache entries based on time or explicit invalidation.
/// </summary>
public sealed class CachingMiddleware : IPluginMiddleware
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;
    private readonly HashSet<string> _cachableOperations;
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _keysByPlugin = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingMiddleware"/> class.
    /// </summary>
    /// <param name="cache">The backing cache.</param>
    /// <param name="cacheDuration">How long entries stay cached. Defaults to five minutes.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cache"/> is null.</exception>
    public CachingMiddleware(IMemoryCache cache, TimeSpan? cacheDuration = null)
    {
        ArgumentNullException.ThrowIfNull(cache);

        _cache = cache;
        _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(5);
        _cachableOperations = new(StringComparer.Ordinal) { "GetMetadata", "ResolveDependencies", "ValidateVersion" };
    }

    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="next"/> is null.</exception>
    public async Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        // Only cache specific operations
        if (!_cachableOperations.Contains(context.OperationType))
        {
            await next(context);
            return;
        }

        var cacheKey = GenerateCacheKey(context);

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            context.Metadata["cached"] = true;
            context.IsSuccessful = true;
            return;
        }

        try
        {
            // Execute operation
            await next(context);

            // Cache successful result
            if (context.IsSuccessful && context.Exception is null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheDuration,
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                };

                var pluginId = context.Plugin.Id;

                cacheOptions.RegisterPostEvictionCallback(static (evictedKey, _, _, state) =>
                {
                    if (state is ConcurrentDictionary<string, byte> keys && evictedKey is string k)
                        keys.TryRemove(k, out _);
                }, _keysByPlugin.GetOrAdd(pluginId, static _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal)));

                _cache.Set(cacheKey, new CacheEntry
                {
                    OperationType = context.OperationType,
                    PluginId = pluginId,
                    CachedAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }, cacheOptions);

                _keysByPlugin.GetOrAdd(pluginId, static _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal))[cacheKey] = 0;
            }
        }
        catch
        {
            // Ensure cache is not set on error
            _cache.Remove(cacheKey);
            if (_keysByPlugin.TryGetValue(context.Plugin.Id, out var trackedKeys))
                trackedKeys.TryRemove(cacheKey, out _);
            throw;
        }
    }

    private static string GenerateCacheKey(PluginOperationContext context)
    {
        var operationSuffix = context.Metadata.Count > 0
            ? string.Join("_", context.Metadata.Keys.OrderBy(k => k))
            : string.Empty;

        return $"plugin_op_{context.Plugin.Id}_{context.OperationType}_{operationSuffix}";
    }

    /// <summary>
    /// Invalidates every cached operation result belonging to a specific plugin.
    /// </summary>
    /// <param name="pluginId">The plugin whose cached results should be dropped.</param>
    public void InvalidatePluginCache(Guid pluginId)
    {
        if (!_keysByPlugin.TryRemove(pluginId, out var keys))
            return;

        foreach (var key in keys.Keys)
            _cache.Remove(key);
    }

    private sealed class CacheEntry
    {
        public required string OperationType { get; set; }
        public required Guid PluginId { get; set; }
        public required long CachedAtMs { get; set; }
    }
}

/// <summary>
/// Extension methods for registering caching middleware.
/// </summary>
public static class CachingMiddlewareExtensions
{
    /// <summary>
    /// Adds caching middleware to the plugin pipeline.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/> is null.</exception>
    public static PluginMiddlewarePipeline UseCaching(
        this PluginMiddlewarePipeline pipeline,
        TimeSpan? cacheDuration = null)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        // A single middleware instance is shared by the pipeline; creating one per invocation
        // would allocate a fresh cache every call and guarantee a permanent miss.
        var middleware = new CachingMiddleware(new MemoryCache(new MemoryCacheOptions()), cacheDuration);

        return pipeline.Use(next => context => middleware.InvokeAsync(context, next));
    }
}
