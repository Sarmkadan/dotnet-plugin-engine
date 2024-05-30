# MemoryPluginCacheTests

`MemoryPluginCacheTests` is a test class that validates the behavior of the in-memory plugin cache implementation. It ensures correctness for fundamental cache operations—storage, retrieval, removal, expiration, and statistics tracking—covering both simple and complex data types, edge cases such as missing keys, and the calculation of cache hit/miss metrics.

## API

### public MemoryPluginCacheTests()
Default constructor. Creates a new test instance. No parameters, no return value, does not throw.

### public void Dispose()
Performs cleanup of resources held by the test class. Called after each test execution to reset state. No parameters, no return value, does not throw.

### public async Task GetAsync_WhenKeyNotCached_ReturnsDefault
Verifies that attempting to retrieve a value for a key that has never been stored returns the default value for the expected type. No parameters. Returns a `Task` representing the asynchronous test operation. Does not throw under normal conditions.

### public async Task SetAsync_ThenGetAsync_ReturnsStoredValue
Confirms that a value stored via `SetAsync` can be successfully retrieved via `GetAsync` with the same key. No parameters. Returns a `Task`. Does not throw.

### public async Task SetAsync_WithComplexObject_ReturnsStoredObject
Ensures that complex objects (e.g., custom classes with multiple properties) are correctly serialized, stored, and deserialized by the cache. No parameters. Returns a `Task`. Does not throw.

### public async Task RemoveAsync_AfterSet_ValueIsNoLongerReturned
Validates that after calling `RemoveAsync` for an existing key, subsequent `GetAsync` calls return the default value rather than the previously stored entry. No parameters. Returns a `Task`. Does not throw.

### public async Task RemoveAsync_ForNonExistentKey_DoesNotThrow
Checks that removing a key that does not exist in the cache completes without throwing an exception. No parameters. Returns a `Task`. Does not throw.

### public async Task GetStatisticsAsync_AfterCacheHit_RecordsHit
Verifies that the cache statistics correctly register a hit when a requested key is found. No parameters. Returns a `Task`. Does not throw.

### public async Task GetStatisticsAsync_AfterCacheMiss_RecordsMiss
Verifies that the cache statistics correctly register a miss when a requested key is not found. No parameters. Returns a `Task`. Does not throw.

### public async Task CacheStatistics_HitRate_CalculatesCorrectly
Ensures that the hit rate reported by cache statistics matches the expected ratio of hits to total accesses. No parameters. Returns a `Task`. Does not throw.

### public async Task ClearAsync_ResetsHitAndMissCounters
Confirms that clearing the cache resets both hit and miss counters to zero. No parameters. Returns a `Task`. Does not throw.

### public async Task SetAsync_WithExplicitExpiration_DoesNotThrow
Tests that specifying an explicit expiration time when storing a value succeeds without throwing. No parameters. Returns a `Task`. Does not throw.

### public async Task SetAsync_WithoutExpiration_UsesDefaultAndStoresValue
Validates that omitting an expiration parameter causes the cache to apply a default expiration policy while still storing the value correctly. No parameters. Returns a `Task`. Does not throw.

### public async Task GetAsync_WithIntegerValue_ReturnsCorrectType
Ensures that when an integer value is stored, retrieval returns the value as the correct integer type without loss or coercion. No parameters. Returns a `Task`. Does not throw.

### public async Task CacheStatistics_HitRate_IsZeroWithNoAccesses
Verifies that the hit rate is zero when no cache accesses have occurred. No parameters. Returns a `Task`. Does not throw.

## Usage

```csharp
// Example 1: Basic store, retrieve, and remove cycle
var cache = new MemoryPluginCache();
await cache.SetAsync("user:42", new UserProfile { Name = "Alice" });
var profile = await cache.GetAsync<UserProfile>("user:42");
Assert.NotNull(profile);
Assert.Equal("Alice", profile.Name);

await cache.RemoveAsync("user:42");
var removed = await cache.GetAsync<UserProfile>("user:42");
Assert.Null(removed);
```

```csharp
// Example 2: Verifying statistics after mixed accesses
var cache = new MemoryPluginCache();
await cache.SetAsync("key1", 100);
await cache.SetAsync("key2", 200);

// Hit
var value = await cache.GetAsync<int>("key1");
// Miss
var missing = await cache.GetAsync<int>("nonexistent");

var stats = await cache.GetStatisticsAsync();
Assert.Equal(1, stats.Hits);
Assert.Equal(1, stats.Misses);
Assert.Equal(0.5, stats.HitRate);
```

## Notes

- All test methods are asynchronous and return `Task`; they should be awaited to ensure proper execution and cleanup.
- The `Dispose` method is called by the test framework between tests to prevent state leakage across test cases.
- Tests for `RemoveAsync` on non-existent keys confirm that the operation is idempotent and does not throw, which is critical for callers that may attempt cleanup without checking existence first.
- Statistics-related tests assume a reset or fresh cache instance to avoid interference from prior operations; `ClearAsync` is explicitly tested to guarantee counter reset behavior.
- Expiration tests only verify that the API accepts explicit and default expiration without throwing; they do not validate time-based eviction logic, which would require time manipulation or waiting.
- The class does not expose thread-safety tests directly, but the underlying cache implementation is expected to support concurrent access. Callers performing parallel `SetAsync`, `GetAsync`, or `RemoveAsync` operations should rely on the implementation’s own synchronization guarantees.
