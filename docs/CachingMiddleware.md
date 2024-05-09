# CachingMiddleware

The `CachingMiddleware` component within the `dotnet-plugin-engine` project provides a mechanism to intercept plugin operations and manage their execution results based on temporal validity. It functions as a specialized middleware that requires specific context regarding the operation type, the target plugin identity, and the timestamp of the cached data to determine whether to serve a cached response or proceed with fresh execution. This middleware exposes both instance-level methods for direct cache invalidation and asynchronous invocation, as well as a static extension method for seamless integration into the plugin processing pipeline.

## API

### Constructors

#### `public CachingMiddleware()`
Initializes a new instance of the `CachingMiddleware` class. As this class utilizes `required` properties, the instance must be fully initialized with `OperationType`, `PluginId`, and `CachedAtMs` immediately after construction or via object initializer syntax before the object can be used.

### Properties

#### `public required string OperationType`
Gets or sets the specific type of operation being cached (e.g., "Execute", "Validate", "Initialize"). This property is mandatory and serves as a key component in distinguishing cache entries for different actions performed by the same plugin.

#### `public required Guid PluginId`
Gets or sets the unique identifier of the plugin associated with this middleware instance. This property is mandatory and ensures that cache isolation is maintained between different plugins within the engine.

#### `public required long CachedAtMs`
Gets or sets the Unix timestamp in milliseconds representing when the data was originally cached. This property is mandatory and is used internally to calculate the age of the cache entry against configured expiration policies.

### Methods

#### `public async Task InvokeAsync()`
Executes the middleware logic asynchronously. This method evaluates the current cache state based on the `CachedAtMs` and `OperationType`. If a valid cache entry exists, it returns the cached result immediately; otherwise, it proceeds to execute the underlying plugin operation and updates the cache.
*   **Returns**: A `Task` representing the asynchronous operation.
*   **Throws**: May throw exceptions propagated from the underlying plugin execution if cache miss occurs, or serialization exceptions if cache retrieval fails.

#### `public void InvalidatePluginCache()`
Immediately clears any cached data associated with the current `PluginId` and `OperationType` combination. This method is synchronous and ensures that subsequent calls to `InvokeAsync` will force a fresh execution of the plugin logic rather than serving stale data.
*   **Returns**: None.
*   **Throws**: Generally does not throw unless the underlying cache store is in an invalid state.

### Static Members

#### `public static PluginMiddlewarePipeline UseCaching`
A static property or method (depending on implementation context of `PluginMiddlewarePipeline`) that configures the middleware pipeline to include caching capabilities. This member acts as the entry point for registering `CachingMiddleware` into the engine's request processing flow.
*   **Returns**: A `PluginMiddlewarePipeline` instance configured with caching enabled.
*   **Usage**: Typically invoked during the application startup or plugin host configuration phase.

## Usage

### Example 1: Direct Instantiation and Invocation
This example demonstrates creating a specific middleware instance for a "DataFetch" operation, setting the required context properties, and invoking it asynchronously.

```csharp
using System;
using System.Threading.Tasks;
using DotNetPluginEngine;

public class PluginExecutor
{
    public async Task ExecuteWithCacheAsync(Guid targetPluginId)
    {
        var middleware = new CachingMiddleware
        {
            OperationType = "DataFetch",
            PluginId = targetPluginId,
            CachedAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        try
        {
            // Invokes the logic, serving from cache if valid
            await middleware.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Plugin execution failed: {ex.Message}");
        }
    }
}
```

### Example 2: Cache Invalidation and Pipeline Configuration
This example shows how to forcibly clear the cache for a specific operation and how to register the caching capability into the global middleware pipeline using the static helper.

```csharp
using System;
using DotNetPluginEngine;

public class CacheManager
{
    private readonly CachingMiddleware _middleware;

    public CacheManager(Guid pluginId)
    {
        _middleware = new CachingMiddleware
        {
            OperationType = "HeavyComputation",
            PluginId = pluginId,
            CachedAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }

    public void RefreshCache()
    {
        // Force next invocation to bypass cache
        _middleware.InvalidatePluginCache();
    }

    public static void ConfigurePipeline()
    {
        // Register caching into the engine's pipeline
        var pipeline = PluginMiddlewarePipeline.UseCaching;
        
        // Further pipeline configuration would follow here
        Console.WriteLine("Caching middleware registered.");
    }
}
```

## Notes

*   **Initialization Requirements**: Because `OperationType`, `PluginId`, and `CachedAtMs` are marked as `required`, any attempt to instantiate `CachingMiddleware` without initializing these properties via an object initializer or constructor logic will result in a compile-time error.
*   **Thread Safety**: The `InvalidatePluginCache` method is synchronous and void-returning, suggesting it performs an immediate side-effect on the cache store. While the method itself does not return a task, care should be taken when calling it concurrently with `InvokeAsync` from multiple threads, as race conditions may occur where a cache invalidation happens precisely during a cache read/write cycle. External synchronization may be required for high-concurrency scenarios.
*   **Timestamp Precision**: The `CachedAtMs` property accepts a `long` representing milliseconds. Ensure that the provided timestamp aligns with the system clock used by the underlying cache storage engine to prevent premature expiration or infinite validity due to clock skew.
*   **Operation Specificity**: Cache entries are scoped by the combination of `PluginId` and `OperationType`. Invalidating the cache for a "Read" operation will not affect cached results for a "Write" operation on the same plugin, ensuring granular control over data consistency.
