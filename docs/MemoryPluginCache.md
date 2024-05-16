# MemoryPluginCache
The `MemoryPluginCache` class is a caching mechanism designed for use within the dotnet-plugin-engine project. It provides a simple, in-memory storage solution for plugins, allowing for efficient retrieval and management of cached data. This cache is particularly useful for storing and retrieving data that is expensive to compute or retrieve, reducing the overhead of repeated computations or requests.

## API
### Constructors
* `public MemoryPluginCache`: Initializes a new instance of the `MemoryPluginCache` class.

### Methods
* `public Task<T?> GetAsync<T>`: Retrieves a cached value of type `T` asynchronously. The method returns a task that resolves to the cached value, or `null` if no value is found.
	+ Parameters: None (type `T` is inferred from the method call).
	+ Return Value: A task that resolves to the cached value of type `T`, or `null` if not found.
	+ Exceptions: May throw exceptions related to asynchronous operations or cache access.
* `public Task SetAsync<T>`: Sets a cached value of type `T` asynchronously.
	+ Parameters: The value to cache (type `T` is inferred from the method call).
	+ Return Value: A task that completes when the value is cached.
	+ Exceptions: May throw exceptions related to asynchronous operations or cache access.
* `public Task RemoveAsync`: Removes a cached value asynchronously.
	+ Parameters: None.
	+ Return Value: A task that completes when the value is removed.
	+ Exceptions: May throw exceptions related to asynchronous operations or cache access.
* `public Task ClearAsync`: Clears all cached values asynchronously.
	+ Parameters: None.
	+ Return Value: A task that completes when the cache is cleared.
	+ Exceptions: May throw exceptions related to asynchronous operations or cache access.
* `public Task<CacheStatistics> GetStatisticsAsync`: Retrieves cache statistics asynchronously.
	+ Parameters: None.
	+ Return Value: A task that resolves to the cache statistics.
	+ Exceptions: May throw exceptions related to asynchronous operations or cache access.

## Usage
The following examples demonstrate how to use the `MemoryPluginCache` class:
```csharp
// Example 1: Basic caching
var cache = new MemoryPluginCache();
await cache.SetAsync("Hello, World!"); // Cache a string
var cachedValue = await cache.GetAsync<string>(); // Retrieve the cached string
Console.WriteLine(cachedValue); // Output: Hello, World!

// Example 2: Caching with statistics
var cache = new MemoryPluginCache();
await cache.SetAsync(42); // Cache an integer
var statistics = await cache.GetStatisticsAsync(); // Retrieve cache statistics
Console.WriteLine($"Cache hits: {statistics.Hits}, Cache misses: {statistics.Misses}");
```

## Notes
* The `MemoryPluginCache` class is designed for in-memory caching, which means that cached data will be lost when the application restarts or the cache is cleared.
* The cache is not thread-safe by default. If multiple threads access the cache concurrently, you may need to implement synchronization mechanisms to prevent data corruption or other concurrency-related issues.
* The `GetAsync` method returns `null` if no value is found in the cache. Be sure to handle this case accordingly in your application logic.
* Cache statistics provide insights into cache performance, including hits, misses, and other metrics. Use these statistics to optimize your caching strategy and improve application performance.
