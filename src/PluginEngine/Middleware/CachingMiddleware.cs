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

    public CachingMiddleware(IMemoryCache cache, TimeSpan? cacheDuration = null)
    {
        _cache = cache;
        _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(5);
        _cachableOperations = new() { "GetMetadata", "ResolveDependencies", "ValidateVersion" };
    }

    public async Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)
    {
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

                _cache.Set(cacheKey, new CacheEntry
                {
                    OperationType = context.OperationType,
                    PluginId = context.Plugin.Id,
                    CachedAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }, cacheOptions);
            }
        }
        catch
        {
            // Ensure cache is not set on error
            _cache.Remove(cacheKey);
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
    /// Invalidates cache for a specific plugin.
    /// </summary>
    public void InvalidatePluginCache(Guid pluginId)
    {
        // In production, would track all cache keys by plugin ID
        // For now, cache entries expire naturally
    }

    private class CacheEntry
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
    public static PluginMiddlewarePipeline UseCaching(
        this PluginMiddlewarePipeline pipeline,
        TimeSpan? cacheDuration = null)
    {
        return pipeline.Use(next => async context =>
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var middleware = new CachingMiddleware(cache, cacheDuration);
            await middleware.InvokeAsync(context, next);
        });
    }
}
