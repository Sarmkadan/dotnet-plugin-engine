# RateLimitMiddleware

A middleware component for ASP.NET Core pipelines that enforces token-bucket-based rate limiting on incoming HTTP requests. It tracks request rates per client and rejects or delays requests that exceed configured limits, preventing resource exhaustion while allowing bursts within defined capacity.

## API

### `RateLimitMiddleware`

Initializes a new instance of the rate-limiting middleware with a token bucket and optional configuration.

| Constructor | Description |
|-------------|-------------|
| `public RateLimitMiddleware(TokenBucket bucket, Func<HttpContext, string>? keySelector = null)` | Creates a middleware instance. The `bucket` parameter defines the rate-limiting capacity and refill behavior. The optional `keySelector` extracts a client identifier (e.g., IP address or API key) from the request; if omitted, the client is treated anonymously. |

---

### `public async Task InvokeAsync(HttpContext context)`

Processes an HTTP request, enforcing rate limits using the configured token bucket.

| Method | Description |
|--------|-------------|
| `public async Task InvokeAsync(HttpContext context)` | Invokes the middleware pipeline. If the client has insufficient tokens, the request is rejected with HTTP 429 (Too Many Requests). Otherwise, the request proceeds and a token is deducted. Throws `ArgumentNullException` if `context` is `null`. |

---

### `TokenBucket`

Represents a token-bucket rate-limiting mechanism with capacity, refill rate, and current token count.

| Property | Description |
|----------|-------------|
| `public int Capacity { get; }` | Maximum number of tokens the bucket can hold. |
| `public int Tokens { get; }` | Current number of available tokens. |
| `public TimeSpan RefillInterval { get; }` | Interval at which tokens are refilled. |
| `public int RefillAmount { get; }` | Number of tokens added per refill cycle. |

| Method | Description |
|--------|-------------|
| `public TokenBucket(int capacity, int refillAmount, TimeSpan refillInterval)` | Initializes a new token bucket with the specified capacity, refill amount, and interval. Throws `ArgumentOutOfRangeException` if `capacity`, `refillAmount`, or `refillInterval` are invalid. |

---

### `public void Refill()`

Adds tokens to the bucket according to its refill parameters.

| Method | Description |
|--------|-------------|
| `public void Refill()` | Increases the current token count by `RefillAmount`, not exceeding `Capacity`. Thread-safe. |

---
### `public bool TryAcquireToken()`

Attempts to consume a token from the bucket.

| Method | Description |
|--------|-------------|
| `public bool TryAcquireToken()` | Deducts one token if available; returns `true` if successful, otherwise `false`. Thread-safe. |

---
### `public static PluginMiddlewarePipeline UseRateLimit(this PluginMiddlewarePipeline pipeline, TokenBucket bucket, Func<HttpContext, string>? keySelector = null)`

Registers the rate-limiting middleware in a plugin pipeline.

| Method | Description |
|--------|-------------|
| `public static PluginMiddlewarePipeline UseRateLimit(this PluginMiddlewarePipeline pipeline, TokenBucket bucket, Func<HttpContext, string>? keySelector = null)` | Adds the `RateLimitMiddleware` to the pipeline with the given `bucket` and optional `keySelector`. Returns the modified pipeline for chaining. Throws `ArgumentNullException` if `pipeline` or `bucket` is `null`. |

## Usage

### Basic Rate Limiting
