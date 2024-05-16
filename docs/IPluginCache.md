# IPluginCache

Provides metrics and statistics about plugin caching operations within the `dotnet-plugin-engine` system. Implementations of this interface track cache hits, misses, current entry count, and memory usage to enable monitoring and optimization of plugin resolution performance.

## API

### `TotalHits`
- **Purpose**: Gets the total number of cache hits recorded since the cache was initialized.
- **Type**: `long`
- **Return Value**: The cumulative count of successful cache lookups.
- **Thread Safety**: Safe for concurrent reads; implementations must ensure atomic updates during concurrent access.

### `TotalMisses`
- **Purpose**: Gets the total number of cache misses recorded since the cache was initialized.
- **Type**: `long`
- **Return Value**: The cumulative count of unsuccessful cache lookups.
- **Thread Safety**: Safe for concurrent reads; implementations must ensure atomic updates during concurrent access.

### `CurrentEntries`
- **Purpose**: Gets the current number of entries stored in the cache.
- **Type**: `int`
- **Return Value**: The number of active cache entries.
- **Thread Safety**: Safe for concurrent reads; implementations must ensure atomic updates during concurrent access.

### `TotalMemoryBytes`
- **Purpose**: Gets the total estimated memory usage of all cache entries in bytes.
- **Type**: `long`
- **Return Value**: The cumulative memory footprint of cached data.
- **Thread Safety**: Safe for concurrent reads; implementations must ensure atomic updates during concurrent access.

## Usage
