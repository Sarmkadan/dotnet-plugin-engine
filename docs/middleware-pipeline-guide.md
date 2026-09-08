# Middleware pipeline guide

The middleware pipeline wraps plugin operations in delegates. Each middleware receives the current `PluginOperationContext` and a `PluginOperationDelegate` for the next stage. A middleware can run code before and after `next`, stop the chain by not calling it, or let an exception propagate.

## Core types

| API | Signature | Behavior visible in the source |
| --- | --- | --- |
| `IPluginMiddleware.InvokeAsync` | `Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)` | Defines a middleware component. Implementations decide whether and when to invoke `next(context)`. |
| `PluginOperationDelegate` | `delegate Task PluginOperationDelegate(PluginOperationContext context)` | Represents one executable stage, including a fully built pipeline. |
| `PluginMiddlewarePipeline.Use` | `PluginMiddlewarePipeline Use(Func<PluginOperationDelegate, PluginOperationDelegate> middleware)` | Appends a delegate factory and returns the same pipeline for fluent registration. |
| `PluginMiddlewarePipeline.Build` | `PluginOperationDelegate Build()` | Starts with a terminal delegate that returns `Task.CompletedTask`, then wraps it with registrations in reverse order. The first registered middleware therefore executes first. |
| `UseCaching` | `PluginMiddlewarePipeline UseCaching(TimeSpan? cacheDuration = null)` | Adds one shared `CachingMiddleware` backed by a new in-memory cache. The default absolute duration is five minutes. |
| `UseRateLimit` | `PluginMiddlewarePipeline UseRateLimit(int maxTokensPerSecond = 100, int windowSizeSeconds = 1)` | Adds rate limiting, but its extension creates a new `RateLimitMiddleware` for every pipeline invocation. |
| `UseLogging` | `PluginMiddlewarePipeline UseLogging()` | Adds console logging around the next stage and records timing and success or failure on the context. This source file is excluded from the library build. |
| `UseErrorHandling` | `PluginMiddlewarePipeline UseErrorHandling(bool continueOnError = false)` | Adds console error logging and records the exception on the context. By default it rethrows; when `continueOnError` is `true`, it suppresses the exception. This source file is excluded from the library build. |

`Build()` does not accept a terminal operation. To run application code at the end of the chain, register it with `Use`, as shown below. If no registered middleware calls application code, the built-in terminal delegate simply completes.

## Operation context

`PluginOperationContext` is a mutable object shared by every stage:

| Property | Type | Initial behavior |
| --- | --- | --- |
| `OperationType` | `string` | Required when the context is initialized. Caching compares it with exact, ordinal names. |
| `Plugin` | `Plugin` | Required when the context is initialized. The supplied middleware reads its `Id`, `Name`, and `Version`. |
| `Metadata` | `Dictionary<string, object>` | Starts as an empty dictionary and can carry values between stages. |
| `StartTimeMs` | `long` | Set by `LoggingMiddleware` immediately before it calls the next stage. |
| `EndTimeMs` | `long?` | Set by `LoggingMiddleware` after completion or failure. |
| `Exception` | `Exception?` | Set by logging or error handling when they catch an exception. |
| `IsSuccessful` | `bool` | Set by logging after normal completion, by caching on a cache hit, and to `false` by logging or error handling on failure. |

Because all stages receive the same instance, changes made by an inner stage are visible to outer stages after `await next(context)` returns.

## Registration and execution order

Registrations are stored in call order and composed in reverse by `Build()`. Given `A`, `B`, and a terminal operation registered in that order, execution nests as `A -> B -> operation -> B -> A`. A stage that returns without calling `next` short-circuits everything inside it.

The pipeline remains mutable after `Build()`. A delegate already returned by `Build()` keeps the composition that existed at build time; call `Build()` again to include later registrations.

## Supplied extensions

### Caching

`UseCaching` constructs one `CachingMiddleware` when the extension is called, so its cache is shared by subsequent invocations of that built pipeline. Only operation types `GetMetadata`, `ResolveDependencies`, and `ValidateVersion` are cacheable.

The key contains the plugin ID, operation type, and the sorted **names** of metadata entries; metadata values are not included. On a hit, caching sets `Metadata["cached"]` and `IsSuccessful`, then returns without calling the rest of the pipeline. After a miss, it stores an entry only when `IsSuccessful` is `true` and `Exception` is `null`. Entries use the configured absolute duration and a one-minute sliding expiration. Exceptions remove the candidate entry and are rethrown.

### Rate limiting

`RateLimitMiddleware` maintains a token bucket per plugin ID. An accepted operation receives the remaining count in `Metadata["rate_limit_remaining"]`; a rejected operation throws `InvalidOperationException` before calling the next stage.

The `UseRateLimit` extension constructs that middleware inside the invocation delegate. Consequently, its per-plugin bucket dictionary is recreated for every invocation; persistent rate-limit state exists only when the same `RateLimitMiddleware` instance is invoked directly or registered through a custom `Use` wrapper.

### Logging

`LoggingMiddleware` logs start and completion through `ILogger<LoggingMiddleware>`. It records Unix-millisecond start and end values, marks normal completion successful, and on failure records the exception, marks failure, logs it, and rethrows it. `UseLogging` creates a console logger and a new middleware instance for each invocation.

### Error handling

`ErrorHandlingMiddleware` catches both `PluginException` and general exceptions, records the exception, marks the context unsuccessful, and logs an error. Plugin exceptions receive details based on their subtype. It rethrows unless constructed with `continueOnError: true`. `UseErrorHandling` creates a console logger and a new middleware instance for each invocation.

## Composed example

This example uses the extensions included in the current library build and adds the operation itself as the innermost `Use` registration:

```csharp
using PluginEngine.Domain.Entities;
using PluginEngine.Middleware;

var plugin = new Plugin
{
    Id = Guid.NewGuid(),
    Name = "Catalog",
    Version = "1.0.0"
};

var context = new PluginOperationContext
{
    OperationType = "GetMetadata",
    Plugin = plugin
};

PluginOperationDelegate operation = new PluginMiddlewarePipeline()
    .UseCaching(TimeSpan.FromMinutes(10))
    .UseRateLimit(maxTokensPerSecond: 50, windowSizeSeconds: 1)
    .Use(next => async currentContext =>
    {
        // This is the innermost operation. There is no later work in this example.
        currentContext.Metadata["result"] = "catalog metadata";
        currentContext.IsSuccessful = true;
        await next(currentContext);
    })
    .Build();

await operation(context);
```

On the first cacheable invocation, caching calls rate limiting and the operation. If the context is successful and has no exception, caching stores an entry. A later invocation with the same generated key is stopped by caching before rate limiting or the operation executes.

## Build exclusions

The library project explicitly excludes these middleware source files from compilation:

- `Middleware/ErrorHandlingMiddleware.cs`
- `Middleware/LoggingMiddleware.cs`

This means `ErrorHandlingMiddleware`, `LoggingMiddleware`, `UseErrorHandling`, and `UseLogging` are present in the repository source but are not members of the currently built `PluginEngine` assembly. They cannot be used by a consumer of that build unless the project inclusion rules change. `IPluginMiddleware.cs`, `CachingMiddleware.cs`, and `RateLimitMiddleware.cs` are included in the library build.

The test project also excludes `CachingMiddlewareTests.cs`, `CachingMiddlewareValidationTests.cs`, and `ErrorHandlingMiddlewareTests.cs` from its own compilation. Those exclusions affect test coverage, not which public middleware types are emitted by the library.
