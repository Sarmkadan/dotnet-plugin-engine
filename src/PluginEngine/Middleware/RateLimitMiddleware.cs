#nullable enable
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
public sealed class RateLimitMiddleware : IPluginMiddleware
{
    private readonly Dictionary<Guid, TokenBucket> _buckets = [];
    private readonly int _maxTokensPerSecond;
    private readonly int _windowSizeSeconds;
    private readonly object _lock = new();

    /// <summary>The default number of operations allowed per window.</summary>
    public const int DefaultMaxTokensPerSecond = 100;
    /// <summary>The default length of the rate-limit window, in seconds.</summary>
    public const int DefaultWindowSizeSeconds = 1;

    /// <summary>Initializes a new instance of the <see cref="RateLimitMiddleware"/> class.</summary>
    /// <param name="maxTokensPerSecond">The maximum number of operations allowed per window.</param>
    /// <param name="windowSizeSeconds">The length of the rate-limit window, in seconds.</param>
    public RateLimitMiddleware(int maxTokensPerSecond = DefaultMaxTokensPerSecond, int windowSizeSeconds = DefaultWindowSizeSeconds)
    {
        _maxTokensPerSecond = maxTokensPerSecond;
        _windowSizeSeconds = windowSizeSeconds;
    }

    /// <summary>
    /// Consumes a token for the plugin in the operation context and invokes the next delegate in the pipeline.
    /// </summary>
    /// <param name="context">The operation context. Its plugin determines the rate-limit bucket.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="next"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the plugin has exhausted its rate limit.</exception>
    public async Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

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
        private const int MillisecondsPerSecond = 1000;

        public int RemainingTokens => _tokens;

        /// <summary>Initializes a new bucket that starts full.</summary>
        /// <param name="capacity">The maximum number of tokens the bucket can hold; also the number of tokens added per refill interval.</param>
        /// <param name="refillIntervalSeconds">The length of the refill interval, in seconds.</param>
        public TokenBucket(int capacity, int refillIntervalSeconds)
        {
            _capacity = capacity;
            _refillRatePerSecond = capacity / refillIntervalSeconds;
            _tokens = capacity;
            _lastRefillMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Adds tokens based on the time elapsed since the last refill, capped at the bucket capacity.
        /// The refill timestamp is only advanced when at least one whole token has been added.
        /// </summary>
        public void Refill()
        {
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var elapsedSeconds = (nowMs - _lastRefillMs) / (double)MillisecondsPerSecond;
            var tokensToAdd = (int)(elapsedSeconds * _refillRatePerSecond);

            if (tokensToAdd > 0)
            {
                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                _lastRefillMs = nowMs;
            }
        }

        /// <summary>
        /// Refills the bucket and attempts to consume a single token.
        /// </summary>
        /// <returns><c>true</c> if a token was consumed; <c>false</c> if the bucket is empty.</returns>
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
    /// <param name="pipeline">The pipeline to add the middleware to.</param>
    /// <param name="maxTokensPerSecond">The maximum number of operations allowed per window.</param>
    /// <param name="windowSizeSeconds">The length of the rate-limit window, in seconds.</param>
    /// <returns>The same pipeline instance, to allow chaining.</returns>
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
