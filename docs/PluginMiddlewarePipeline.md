# PluginMiddlewarePipeline

`PluginMiddlewarePipeline` composes middleware components that wrap plugin operations. Each middleware receives a `PluginOperationContext` describing the operation and a `PluginOperationDelegate` that invokes the rest of the pipeline. Middleware is registered in order with `Use`, and `Build` produces a single delegate in which the first registered middleware runs first. Extension methods such as `UseCaching` and `UseRateLimit` register the built-in middleware on the pipeline.

The types are defined in `src/PluginEngine/Middleware/IPluginMiddleware.cs`. The middleware contract itself is `IPluginMiddleware`, whose `InvokeAsync(PluginOperationContext, PluginOperationDelegate)` method is implemented by middleware classes such as `CachingMiddleware` and `RateLimitMiddleware`.

## API

### `public sealed class PluginOperationContext`

The state of one plugin operation as it moves through the pipeline. Middleware reads and writes these properties to share information with other middleware and with the caller.

- **Properties**
  - `public required string OperationType { get; set; }`: The name of the operation, such as `GetMetadata`, `ResolveDependencies`, or `ValidateVersion`. The caching middleware uses this value to decide whether to cache the operation.
  - `public required Plugin Plugin { get; set; }`: The plugin the operation applies to. Rate limiting keys its buckets on `Plugin.Id`.
  - `public Dictionary<string, object> Metadata { get; set; }`: Free-form key-value data shared between middleware. Initialized to an empty dictionary. Built-in middleware writes entries such as `cached` (caching) and `rate_limit_remaining` (rate limiting).
  - `public long StartTimeMs { get; set; }`: The start time of the operation in Unix milliseconds. The pipeline does not set this value; middleware or the caller is responsible for it.
  - `public long? EndTimeMs { get; set; }`: The end time of the operation in Unix milliseconds, or `null` if it has not finished. The pipeline does not set this value.
  - `public Exception? Exception { get; set; }`: The exception raised by the operation, if any. The pipeline does not set this value.
  - `public bool IsSuccessful { get; set; }`: Whether the operation succeeded. Defaults to `false`. The pipeline does not set this value, so the innermost step must set it to `true` on success. The caching middleware stores a result only when this value is `true` and `Exception` is `null`.

---

### `public delegate Task PluginOperationDelegate(PluginOperationContext context)`

The signature of a step in the pipeline. Calling the delegate runs the next middleware, or the terminal step when no middleware remains.

- **Parameters**
  - `context`: The operation context to pass to the next step.
- **Returns**
  - A `Task` that completes when the next step has finished.
- **Throws**
  - Any exception thrown by the next step propagates to the caller unless a middleware catches it.

---

### `public sealed class PluginMiddlewarePipeline`

A builder that stores middleware in registration order and composes it into a `PluginOperationDelegate`. The builder is not thread-safe while middleware is being added; a delegate returned by `Build` is independent of later changes to the builder.

- **Constructors**
  - `public PluginMiddlewarePipeline()`: Creates an empty pipeline. A built empty pipeline completes without doing any work.

---

### `public PluginMiddlewarePipeline Use(Func<PluginOperationDelegate, PluginOperationDelegate> middleware)`

Adds a middleware factory to the end of the pipeline. The factory receives the next delegate in the chain and returns the delegate that runs this middleware and then calls `next`.

- **Parameters**
  - `middleware`: A function that takes the next `PluginOperationDelegate` and returns a `PluginOperationDelegate` for this middleware step.
- **Returns**
  - The same `PluginMiddlewarePipeline` instance, so calls can be chained.
- **Throws**
  - None at registration time. If `middleware` is `null`, a `NullReferenceException` is thrown when the built pipeline runs.

---

### `public PluginOperationDelegate Build()`

Composes all registered middleware into one delegate. The first middleware added runs first, and the last middleware added runs immediately before the terminal step. The terminal step completes without doing any work, so a pipeline that must perform an operation needs a middleware that performs it.

- **Parameters**
  - None.
- **Returns**
  - A `PluginOperationDelegate` that runs every registered middleware in order. The result is a snapshot: later calls to `Use` do not change it.
- **Throws**
  - None.

---

### Built-in extension methods

The following extension methods on `PluginMiddlewarePipeline` are part of the library build and are used in the example below.

- `public static PluginMiddlewarePipeline UseCaching(this PluginMiddlewarePipeline pipeline, TimeSpan? cacheDuration = null)`: Adds a `CachingMiddleware` that uses a new in-memory cache. The cache is created once when `UseCaching` is called and is shared by every invocation of the built pipeline. `cacheDuration` sets the absolute expiration; the default is five minutes. Only the operation types `GetMetadata`, `ResolveDependencies`, and `ValidateVersion` are cached. Throws `ArgumentNullException` if `pipeline` is `null`.
- `public static PluginMiddlewarePipeline UseRateLimit(this PluginMiddlewarePipeline pipeline, int maxTokensPerSecond = 100, int windowSizeSeconds = 1)`: Adds a token-bucket rate limiter that allows `maxTokensPerSecond` operations per `windowSizeSeconds` for each plugin. An operation over the limit throws `InvalidOperationException`. See the note below, because the current implementation does not keep bucket state between invocations.

> **Note:** `UseRateLimit` creates a new `RateLimitMiddleware` inside each invocation of the pipeline delegate. Each invocation therefore starts with a full bucket, and the per-plugin limit is not enforced across calls made through the extension method. Rate limiting only persists if you keep one `RateLimitMiddleware` instance and register it with `Use` yourself.

---

## Usage

### Example: caching and rate limiting in one pipeline

This example places caching before rate limiting. Cache hits return before the rate limiter runs, so they do not consume tokens. The final `Use` call is the innermost step: it performs the work and sets `IsSuccessful`, which the caching middleware requires before it stores a result.

```csharp
using PluginEngine.Middleware;

PluginOperationDelegate pipeline = new PluginMiddlewarePipeline()
    .UseCaching(cacheDuration: TimeSpan.FromMinutes(10))
    .UseRateLimit(maxTokensPerSecond: 50, windowSizeSeconds: 1)
    .Use(next => async context =>
    {
        // Core operation: replace with the real plugin call.
        await Task.Delay(10);
        context.IsSuccessful = true;
    })
    .Build();

var context = new PluginOperationContext
{
    OperationType = "GetMetadata",
    Plugin = plugin, // a Plugin obtained from IPluginManagerService
};

try
{
    await pipeline(context);

    bool fromCache = context.Metadata.ContainsKey("cached");
    Console.WriteLine($"Succeeded: {context.IsSuccessful}, served from cache: {fromCache}");
}
catch (InvalidOperationException ex)
{
    // Thrown by the rate limiter when the plugin has used its tokens for the current window.
    Console.WriteLine(ex.Message);
}
```
