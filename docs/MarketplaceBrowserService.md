# MarketplaceBrowserService

`MarketplaceBrowserService` implements `IMarketplaceBrowserService` by combining marketplace searches with in-memory caching. It exposes the built-in marketplace categories, download-sorted trending entries, verified featured entries, category browsing, and an aggregated home-page snapshot.

All types documented here are in the `PluginEngine.Marketplace` namespace.

## Signatures

| Member | Signature | Result |
| --- | --- | --- |
| Constructor | `MarketplaceBrowserService(IPluginMarketplaceService marketplace, IMemoryCache cache, ILogger<MarketplaceBrowserService> logger)` | Creates the service; throws `ArgumentNullException` when any dependency is `null`. |
| Categories | `Task<PluginOperationResult<List<MarketplaceCategory>>> GetCategoriesAsync(CancellationToken cancellationToken = default)` | A successful result containing the built-in categories. |
| Trending | `Task<PluginOperationResult<List<MarketplaceEntry>>> GetTrendingAsync(int limit = 10, CancellationToken cancellationToken = default)` | Marketplace entries sorted by downloads. |
| Featured | `Task<PluginOperationResult<List<MarketplaceEntry>>> GetFeaturedAsync(CancellationToken cancellationToken = default)` | Verified marketplace entries sorted by rating. |
| Browse category | `Task<PluginOperationResult<List<MarketplaceEntry>>> BrowseCategoryAsync(string categoryId, MarketplaceSearchFilter? filter = null, CancellationToken cancellationToken = default)` | The result returned by the underlying marketplace search, or a failure result. |
| Home page | `Task<PluginOperationResult<MarketplaceHomePage>> GetHomePageAsync(CancellationToken cancellationToken = default)` | A successful aggregated snapshot, or a failure result if assembly throws. |

Methods return `PluginOperationResult<T>`. Successful data is held in its `Data` property. Search failures from the underlying service are returned directly where noted; exceptions caught by the browser are converted with `PluginOperationResult<T>.FromException`.

## Caching

The service uses absolute expiration relative to the time each value is cached.

| Data | Cache key | TTL | Notes |
| --- | --- | --- | --- |
| Categories | `mp_categories` | 6 hours | The same key is used for the built-in category list. |
| Trending | `mp_trending_{limit}` | 15 minutes | `limit` is clamped before constructing the key, so values from 1 through 50 have separate cache entries. |
| Featured | `mp_featured` | 1 hour | Stores the verified, rating-sorted search data. |
| Home page | `mp_homepage` | 10 minutes | Stores the assembled `MarketplaceHomePage` independently of the component caches. |

`BrowseCategoryAsync` does not add a browser-level cache entry; it delegates directly to `IPluginMarketplaceService.SearchAsync`.

The cached values are mutable objects and lists, and cache hits return those stored instances. The implementation does not clone them.

## Methods

### Constructor

The constructor stores an `IPluginMarketplaceService`, `IMemoryCache`, and logger. It validates each argument and throws `ArgumentNullException` immediately for a null dependency.

### `GetCategoriesAsync`

Returns eight built-in categories: `analytics`, `authentication`, `caching`, `database`, `logging`, `messaging`, `security`, and `ui`. Each category supplies an ID, display name, description, and icon. `PluginCount` is not assigned by this method, so it retains its default value of `0`.

On a cache miss, the method copies the built-in list into a new list, caches it for 6 hours, and returns a successful result. It performs no asynchronous work and returns through `Task.FromResult`. The cancellation token is accepted but is not inspected or passed elsewhere.

### `GetTrendingAsync`

Clamps `limit` to the inclusive range **1–50** with `Math.Clamp`; the default is `10`. Thus values below 1 behave as 1, and values above 50 behave as 50. The clamped value is used both as `MarketplaceSearchFilter.PageSize` and as part of the cache key.

On a cache miss, it calls `SearchAsync` with:

- `SortOrder = MarketplaceSortOrder.Downloads`
- `PageSize = limit` after clamping

If the search result is unsuccessful, that result is returned without caching. On success, a null `Data` value becomes an empty list, which is cached for 15 minutes. Exceptions are logged and converted to failure results with `FromException`.

### `GetFeaturedAsync`

On a cache miss, calls `SearchAsync` with:

- `OnlyVerified = true`
- `SortOrder = MarketplaceSortOrder.Rating`
- `PageSize = 6`

If the search result is unsuccessful, that result is returned without caching. On success, null data becomes an empty list and is cached for 1 hour. Exceptions are logged and converted to failure results with `FromException`.

### `BrowseCategoryAsync`

Returns a failure result with message `Category ID is required.` and status code `400` when `categoryId` is null, empty, or whitespace.

Otherwise, it uses the supplied filter or creates a new `MarketplaceSearchFilter`. If the filter's `Tags` list does not already contain the category ID using an ordinal, case-insensitive comparison, it replaces `Tags` with a new list having `categoryId` first followed by the existing tags. This means a caller-supplied filter can be mutated. The effective filter and cancellation token are then passed to `SearchAsync`, and its result is returned unchanged. Exceptions are converted to failure results with `FromException`.

### `GetHomePageAsync`

On a cache miss, starts these three operations before awaiting all of them:

- `GetCategoriesAsync(cancellationToken)`
- `GetTrendingAsync(5, cancellationToken)`
- `GetFeaturedAsync(cancellationToken)`

It builds a `MarketplaceHomePage` from each result's `Data`. Missing data, including data absent from an unsuccessful component result, is replaced with an empty list. The method does not propagate a component's unsuccessful result by itself; after aggregation it returns a successful home-page result. The snapshot is cached for 10 minutes.

Any exception that escapes the aggregation block is logged and converted to a failure result with `FromException`.

## Result models

### `MarketplaceCategory`

`MarketplaceCategory` is a mutable class describing a category.

| Property | Type | Initial value | Meaning |
| --- | --- | --- | --- |
| `Id` | `string` | `string.Empty` | Unique category slug, such as `logging`. |
| `Name` | `string` | `string.Empty` | Human-readable category name. |
| `Description` | `string` | `string.Empty` | Short category description. |
| `Icon` | `string` | `string.Empty` | Display icon name or emoji. |
| `PluginCount` | `int` | `0` | Total plugin count for the category. |

### `MarketplaceHomePage`

`MarketplaceHomePage` is a mutable aggregate returned by `GetHomePageAsync`.

| Property | Type | Initial value | Meaning |
| --- | --- | --- | --- |
| `Featured` | `List<MarketplaceEntry>` | Empty list | Curated featured entries. |
| `Trending` | `List<MarketplaceEntry>` | Empty list | Trending entries. |
| `Categories` | `List<MarketplaceCategory>` | Empty list | Available categories. |
| `GeneratedAtUtc` | `DateTime` | `DateTime.UtcNow` | UTC time at which the object initializer runs. |

When `GetHomePageAsync` assembles a new page, it does not explicitly set `GeneratedAtUtc`, so the property initializer records the construction time. A cached page retains that original value.

## Example

The following class consumes the interface, checks the operation result, and requests a category page. Passing `100` to `GetTrendingAsync` results in an underlying page size of `50` because of limit clamping.

```csharp
using PluginEngine.Marketplace;

public sealed class MarketplaceView(IMarketplaceBrowserService browser)
{
    public async Task ShowAsync(CancellationToken cancellationToken = default)
    {
        var trendingResult = await browser.GetTrendingAsync(100, cancellationToken);
        if (!trendingResult.Success)
        {
            Console.WriteLine(trendingResult.Message);
            return;
        }

        foreach (var entry in trendingResult.Data ?? [])
            Console.WriteLine($"{entry.Name}: {entry.Downloads} downloads");

        var loggingResult = await browser.BrowseCategoryAsync(
            "logging",
            new MarketplaceSearchFilter
            {
                OnlyVerified = true,
                SortOrder = MarketplaceSortOrder.Rating,
                PageSize = 10
            },
            cancellationToken);

        if (loggingResult.Success)
            Console.WriteLine($"Logging results: {loggingResult.Data?.Count ?? 0}");
    }
}
```
