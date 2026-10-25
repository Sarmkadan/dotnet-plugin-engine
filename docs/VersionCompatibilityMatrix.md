# VersionCompatibilityMatrix

`VersionCompatibilityMatrix` maps every known plugin version to the host engine versions it was checked against, and records a `CompatibilityStatus` for each pairing. `CompatibilityStatus` is the status enum used by the matrix. `MarketplaceSearchFilter` and `MarketplaceSortOrder` describe marketplace search criteria and result ordering. All four types live in `src/PluginEngine/Marketplace/MarketplaceModels.cs` under the `PluginEngine.Marketplace` namespace.

## CompatibilityStatus

Compatibility status between a plugin version and a host engine version.

| Member | Meaning |
|--------|---------|
| `Compatible` | Fully supported and tested against this engine version. |
| `Incompatible` | Known to be broken or unsupported on this engine version. |
| `Deprecated` | Works, but the combination is no longer maintained or recommended. |
| `Unknown` | Compatibility has not been verified for this pairing. |

---

## VersionCompatibilityMatrix

Cross-reference matrix that maps every published plugin version to the set of host engine versions and the `CompatibilityStatus` for each pairing. The type is a `sealed class`.

### Properties

- `Guid PluginId { get; init; }`: The plugin identifier this matrix belongs to.
- `string PluginName { get; init; }`: The plugin display name. Defaults to an empty string.
- `DateTime GeneratedAtUtc { get; init; }`: When this matrix was generated. Defaults to `DateTime.UtcNow` at construction.
- `IReadOnlyDictionary<string, IReadOnlyDictionary<string, CompatibilityStatus>> Entries { get; }`: A read-only view of all recorded entries, keyed by plugin version and then by engine version. Each access builds a new snapshot dictionary, so the returned view does not change when `Record` is called later.

The internal storage is a `Dictionary<string, Dictionary<string, CompatibilityStatus>>` created with `StringComparer.OrdinalIgnoreCase`. Both key levels are therefore case-insensitive.

---

### `public void Record(string pluginVersion, string engineVersion, CompatibilityStatus status)`

Records the compatibility status for a plugin version and engine version pair. Recording the same pair again replaces the previous status.

- **Parameters**
  - `pluginVersion`: The plugin version string, for example `"2.1.0"`.
  - `engineVersion`: The host engine version string, for example `"10.0"`.
  - `status`: The resolved `CompatibilityStatus`.
- **Returns**
  - `void`.
- **Throws**
  - `ArgumentNullException`: If `pluginVersion` or `engineVersion` is `null`.

---

### `public CompatibilityStatus GetStatus(string pluginVersion, string engineVersion)`

Returns the compatibility status for the given pairing.

- **Parameters**
  - `pluginVersion`: The plugin version to look up.
  - `engineVersion`: The engine version to look up.
- **Returns**
  - The recorded `CompatibilityStatus`. Returns `CompatibilityStatus.Unknown` when the plugin version or the pairing has not been recorded.
- **Throws**
  - `ArgumentNullException`: If `pluginVersion` or `engineVersion` is `null`.

---

### `public IReadOnlyList<string> GetCompatiblePluginVersions(string engineVersion)`

Returns every plugin version whose recorded status for the given engine version is `CompatibilityStatus.Compatible`. Versions recorded as `Deprecated`, `Incompatible`, or `Unknown` are excluded.

- **Parameters**
  - `engineVersion`: The engine version to filter by.
- **Returns**
  - A new list of matching plugin version strings. The list is empty when no version is compatible. The order follows the internal dictionary enumeration order and is not guaranteed to be sorted.
- **Throws**
  - `ArgumentNullException`: If `engineVersion` is `null`.

---

### `public override string ToString()`

- **Returns**
  - A string in the format `"{PluginName} compatibility matrix ({count} recorded pairs)"`, where `count` is the total number of recorded plugin version and engine version pairs.

---

## MarketplaceSortOrder

Sort order options for marketplace search results.

| Member | Meaning |
|--------|---------|
| `Relevance` | Sorted by search relevance score. This is the default. |
| `Downloads` | Sorted by total download count, descending. |
| `Rating` | Sorted by average community rating, descending. |
| `LastUpdated` | Sorted by last published date, descending. |
| `Name` | Sorted alphabetically by plugin name. |

---

## MarketplaceSearchFilter

Search and filter criteria passed to marketplace queries. The type is a `sealed class` with settable properties. Fluent helpers are available in `MarketplaceSearchFilterExtensions`.

### Fields

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Query` | `string?` | `null` | Free-text search query. |
| `Tags` | `List<string>` | empty list | Tags that results must contain at least one of. |
| `MinVersion` | `string?` | `null` | Minimum acceptable plugin version. |
| `TargetFramework` | `string?` | `null` | Required target framework moniker, for example `"net10.0"`. |
| `OnlyVerified` | `bool` | `false` | When `true`, returns only marketplace-verified plugins. |
| `Page` | `int` | `1` | 1-based result page number. |
| `PageSize` | `int` | `20` | Results per page. The registry caps this at 100. |
| `SortOrder` | `MarketplaceSortOrder` | `Relevance` | Desired sort order for returned results. |

`PluginMarketplaceService.SearchAsync` forwards only `Query` (or an empty string when it is `null`) and `PageSize` to the registry. The other fields are carried on the filter for callers and other `IPluginMarketplaceService` implementations, and `PluginMarketplaceService` does not apply them.

### `public override string ToString()`

- **Returns**
  - A string in the format `"MarketplaceSearchFilter: Query='{query}', Tags=[{tags}], Page={page}/{pageSize}, SortOrder={sortOrder}, OnlyVerified={onlyVerified}"`. A `null` query is shown as `<null>` and an empty tag list as `<none>`.

---

## Matrix caching in PluginMarketplaceService

`PluginMarketplaceService.GetCompatibilityMatrixAsync` builds and caches a `VersionCompatibilityMatrix` for each plugin.

- **Cache store:** the injected `IMemoryCache`.
- **Cache key:** `mp_matrix_{pluginId}`.
- **Time to live:** 30 minutes, set as `AbsoluteExpirationRelativeToNow` through the private static field `MatrixCacheTtl = TimeSpan.FromMinutes(30)`. Cache hits do not extend the expiration.
- **Cache hit:** a non-null matrix stored under the key is returned immediately with the message `Matrix served from cache.`.
- **Cache miss:** the service calls the registry for plugin information and the version list. It records every returned plugin version against each engine version in `KnownEngineVersions` (`"8.0"`, `"9.0"`, `"10.0"`). The matrix `PluginName` is the registry name, or `pluginId.ToString()` when the registry returns no plugin.
- **Storage:** only a successfully built matrix is cached. Exceptions are logged and returned through `PluginOperationResult.FromException`, so a failed build is not cached.

### Status heuristic

`PluginMarketplaceService` assigns each pairing a status from the plugin version's stability flags, its publication age, and the engine major version:

| Plugin version | Engine version | Status |
|----------------|----------------|--------|
| Prerelease | Major >= 10 | `Compatible` |
| Prerelease | Major < 10 | `Unknown` |
| Neither stable nor prerelease | Any | `Unknown` |
| Stable, published 365 days ago or less | Any | `Compatible` |
| Stable, older than 365 and up to 730 days | Major >= 9 | `Compatible` |
| Stable, older than 365 and up to 730 days | Major < 9 | `Deprecated` |
| Stable, older than 730 days | Major >= 10 | `Deprecated` |
| Stable, older than 730 days | Major < 10 | `Incompatible` |

An engine version that does not parse as a `Version` is recorded as `Unknown`.

### CheckCompatibilityAsync

`CheckCompatibilityAsync` calls `GetCompatibilityMatrixAsync` and then `GetStatus`, so it shares the same cache. If the matrix call fails, it returns that failure's message and error code, using `500` only when the error code is `null`.

---

## Usage

```csharp
using PluginEngine.Marketplace;

var matrix = new VersionCompatibilityMatrix
{
    PluginId = pluginId,
    PluginName = "Sample Plugin"
};

matrix.Record("2.1.0", "10.0", CompatibilityStatus.Compatible);
matrix.Record("2.0.0", "10.0", CompatibilityStatus.Deprecated);
matrix.Record("2.1.0", "9.0", CompatibilityStatus.Incompatible);

CompatibilityStatus status = matrix.GetStatus("2.1.0", "10.0");   // Compatible
CompatibilityStatus missing = matrix.GetStatus("1.0.0", "10.0");  // Unknown

IReadOnlyList<string> versions = matrix.GetCompatiblePluginVersions("10.0"); // Contains "2.1.0"

var filter = new MarketplaceSearchFilter
{
    Query = "logging",
    OnlyVerified = true,
    SortOrder = MarketplaceSortOrder.Downloads,
    PageSize = 10
};

IPluginMarketplaceService marketplace = /* resolved from DI */;
var result = await marketplace.GetCompatibilityMatrixAsync(pluginId);
```
