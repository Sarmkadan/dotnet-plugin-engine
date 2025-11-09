// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Middleware;

/// <summary>
/// Middleware for rate-limiting plugin operations.
/// Prevents resource exhaustion by limiting the number of operations per time window.
/// Uses a token bucket algorithm for fair rate limiting across plugins.
/// </summary>
public class RateLimitMiddleware : IPluginMiddleware
{
    private readonly Dictionary<Guid, TokenBucket> _buckets = [];
    private readonly int _maxTokensPerSecond;
    private readonly int _windowSizeSeconds;
    private readonly object _lock = new();

    public RateLimitMiddleware(int maxTokensPerSecond = 100, int windowSizeSeconds = 1)
    {
        _maxTokensPerSecond = maxTokensPerSecond;
        _windowSizeSeconds = windowSizeSeconds;
    }

    public async Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)
    {
        var bucket = GetOrCreateBucket(context.Plugin.Id);

        if (!bucket.TryAcquireToken())
        {
            throw new InvalidOperationException(
                $"Rate limit exceeded for plugin {context.Plugin.Name}. " +
                $"Max {_maxTokensPerSecond} operations per {_windowSizeSeconds} second(s).");
        }

        context.Metadata["rate_limit_remaining"] = bucket.RemainingTokens;

        await next(context);
    }

    private TokenBucket GetOrCreateBucket(Guid pluginId)
    {
        lock (_lock)
        {
            if (_buckets.TryGetValue(pluginId, out var bucket))
            {
                bucket.Refill();
                return bucket;
            }

            var newBucket = new TokenBucket(_maxTokensPerSecond, _windowSizeSeconds);
            _buckets[pluginId] = newBucket;
            return newBucket;
        }
    }

    /// <summary>
    /// Token bucket implementation for rate limiting.
    /// Refills tokens at a constant rate and enforces maximum capacity.
    /// </summary>
    private class TokenBucket
    {
        private readonly int _capacity;
        private readonly int _refillRatePerSecond;
        private int _tokens;
        private long _lastRefillMs;

        public int RemainingTokens => _tokens;

        public TokenBucket(int capacity, int refillIntervalSeconds)
        {
            _capacity = capacity;
            _refillRatePerSecond = capacity / refillIntervalSeconds;
            _tokens = capacity;
            _lastRefillMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public void Refill()
        {
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var elapsedSeconds = (nowMs - _lastRefillMs) / 1000.0;
            var tokensToAdd = (int)(elapsedSeconds * _refillRatePerSecond);

            if (tokensToAdd > 0)
            {
                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                _lastRefillMs = nowMs;
            }
        }

        public bool TryAcquireToken()
        {
            Refill();

            if (_tokens > 0)
            {
                _tokens--;
                return true;
            }

            return false;
        }
    }
}

/// <summary>
/// Extension methods for registering rate limiting middleware.
/// </summary>
public static class RateLimitMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the plugin pipeline.
    /// </summary>
    public static PluginMiddlewarePipeline UseRateLimit(
        this PluginMiddlewarePipeline pipeline,
        int maxTokensPerSecond = 100,
        int windowSizeSeconds = 1)
    {
        return pipeline.Use(next => async context =>
        {
            var middleware = new RateLimitMiddleware(maxTokensPerSecond, windowSizeSeconds);
            await middleware.InvokeAsync(context, next);
        });
    }
}
