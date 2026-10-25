# MarketplaceBrowserModels

Models and service contract for marketplace browsing: `MarketplaceCategory`, `MarketplaceHomePage`, and `IMarketplaceBrowserService`. The default implementation, `MarketplaceBrowserService`, aggregates data from `IPluginMarketplaceService` and caches results in `IMemoryCache`.

## Types

### `public sealed class MarketplaceCategory`

A named plugin category in the marketplace.

- **Properties**
  - `string Id`: Unique category slug (e.g., `"logging"`). Used as the `categoryId` argument of `BrowseCategoryAsync`.
  - `string Name`: Human-readable category name.
  - `string Description`: Short description of the category.
  - `string Icon`: Icon name or emoji used for display.
  - `int PluginCount`: Total number of plugins in the category. Built-in categories are defined with the default value `0`.
- **Defaults**
  - String properties default to `string.Empty`.

---

### `public sealed class MarketplaceHomePage`

Aggregated home-page snapshot combining featured plugins, trending plugins, and categories.

- **Properties**
  - `List<MarketplaceEntry> Featured`: Curated featured plugins. Defaults to an empty list.
  - `List<MarketplaceEntry> Trending`: Currently trending plugins. Defaults to an empty list.
  - `List<MarketplaceCategory> Categories`: All available categories. Defaults to an empty list.
  - `DateTime GeneratedAtUtc`: When the snapshot was created (UTC). Set to `DateTime.UtcNow` at construction.

---

## Built-in Categories

`MarketplaceBrowserService` returns the following categories from `GetCategoriesAsync`. The list is static and does not query the registry, so `PluginCount` is `0` for every entry.

| `Id` | `Name` | `Description` | `Icon` |
|------|--------|---------------|--------|
| `analytics` | Analytics | Data analysis and reporting | 📊 |
| `authentication` | Authentication | Identity and access management | 🔐 |
| `caching` | Caching | Caching and memoization | ⚡ |
| `database` | Database | Database connectivity and ORMs | 🗄️ |
| `logging` | Logging | Logging and observability | 📝 |
| `messaging` | Messaging | Event bus and message brokers | ✉️ |
| `security` | Security | Encryption and security auditing | 🛡️ |
| `ui` | UI | User interface components | 🖥️ |

## Cache TTLs

Each result is stored in `IMemoryCache` with an absolute expiration relative to the time it was stored.

| Operation | Cache key | TTL |
|-----------|-----------|-----|
| `GetCategoriesAsync` | `mp_categories` | 6 hours |
| `GetTrendingAsync` | `mp_trending_{limit}` (after clamping) | 15 minutes |
| `GetFeaturedAsync` | `mp_featured` | 1 hour |
| `GetHomePageAsync` | `mp_homepage` | 10 minutes |

`BrowseCategoryAsync` is not cached.

---

## `IMarketplaceBrowserService`

`public interface IMarketplaceBrowserService`

Discovery-oriented API built on top of `IPluginMarketplaceService`. `MarketplaceBrowserService` is registered as a singleton by `AddPluginMarketplace`, which also calls `AddMemoryCache()`.

### `Task<PluginOperationResult<List<MarketplaceCategory>>> GetCategoriesAsync(CancellationToken cancellationToken = default)`

Returns the full list of built-in categories.

- **Parameters**
  - `cancellationToken`: Token to cancel the operation. Not used, because the list is produced in memory.
- **Returns**
  - A successful result containing a copy of the built-in category list, with the message `"Returned {count} categories."`. Cached for 6 hours.
- **Throws**
  - None. The method does not catch exceptions and does not call the registry.

---

### `Task<PluginOperationResult<List<MarketplaceEntry>>> GetTrendingAsync(int limit = 10, CancellationToken cancellationToken = default)`

Returns plugins ordered by download count, descending.

- **Parameters**
  - `limit`: Maximum number of results. The value is clamped to the range 1–50 before use, so values outside that range do not produce an error. Default is `10`.
  - `cancellationToken`: Token passed to `IPluginMarketplaceService.SearchAsync`.
- **Returns**
  - A successful result with the trending entries, cached for 15 minutes under the clamped limit. If the search fails, the failed result from `SearchAsync` is returned and nothing is cached.
- **Throws**
  - None. Exceptions are logged and returned as a failed result via `PluginOperationResult.FromException`.

---

### `Task<PluginOperationResult<List<MarketplaceEntry>>> GetFeaturedAsync(CancellationToken cancellationToken = default)`

Returns curated featured plugins: verified plugins sorted by rating.

- **Parameters**
  - `cancellationToken`: Token passed to `IPluginMarketplaceService.SearchAsync`.
- **Returns**
  - A successful result with up to 6 entries, cached for 1 hour. If the search fails, the failed result is returned and nothing is cached.
- **Throws**
  - None. Exceptions are returned as a failed result.

---

### `Task<PluginOperationResult<List<MarketplaceEntry>>> BrowseCategoryAsync(string categoryId, MarketplaceSearchFilter? filter = null, CancellationToken cancellationToken = default)`

Browses plugins in a single category, with optional sorting and pagination.

- **Parameters**
  - `categoryId`: Category slug to browse (e.g., `"logging"`).
  - `filter`: Optional search and pagination parameters. If `null`, a default `MarketplaceSearchFilter` is used. `categoryId` is added as the first entry in `Tags` unless `Tags` already contains it (compared case-insensitively). The passed-in filter instance is modified.
  - `cancellationToken`: Token passed to `IPluginMarketplaceService.SearchAsync`.
- **Returns**
  - The search result from `IPluginMarketplaceService.SearchAsync`. Not cached.
  - A failed result with the message `"Category ID is required."` and error code `400` when `categoryId` is `null`, empty, or whitespace. The search is not executed.
- **Throws**
  - None. Exceptions are returned as a failed result.

---

### `Task<PluginOperationResult<MarketplaceHomePage>> GetHomePageAsync(CancellationToken cancellationToken = default)`

Returns a combined home-page snapshot in one call.

- **Parameters**
  - `cancellationToken`: Token passed to the categories, trending, and featured lookups.
- **Returns**
  - A successful result with the message `"Home page assembled."`, or `"Home page served from cache."` when a cached snapshot is returned. The snapshot is cached for 10 minutes.
  - Internally, `GetCategoriesAsync`, `GetTrendingAsync(5)`, and `GetFeaturedAsync` run in parallel. The trending list in the page therefore has at most 5 entries.
  - A failed sub-request contributes an empty list to the page, and the page is still returned as successful and cached. Check the sub-results separately if completeness matters.
- **Throws**
  - None. Exceptions are returned as a failed result.

---

## Usage

```csharp
var result = await browser.GetHomePageAsync(cancellationToken);
if (result.Success && result.Data is { } page)
{
    foreach (var category in page.Categories)
    {
        Console.WriteLine($"{category.Icon} {category.Name}");
    }
}
```
