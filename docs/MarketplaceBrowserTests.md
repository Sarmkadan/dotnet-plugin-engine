# MarketplaceBrowserTests

`MarketplaceBrowserTests` contains the unit test suite for the marketplace browsing functionality within the plugin engine. It validates the behavior of category retrieval, trending plugin queries, category browsing, featured plugin requests, and homepage data aggregation, ensuring correct delegation, caching, parameter clamping, and error handling.

## API

### GetCategoriesAsync_ReturnsBuiltInCategories
Verifies that calling `GetCategoriesAsync` returns the expected set of built-in marketplace categories. The test asserts that the returned collection matches the predefined category list and is not empty.

### GetCategoriesAsync_CachesResult_SecondCallReturnsSameData
Ensures that `GetCategoriesAsync` caches its result. The test invokes the method twice in succession and confirms that the second call returns the same object reference or equivalent data as the first, without re-fetching from the underlying source.

### GetTrendingAsync_DelegatesToMarketplaceWithDownloadsSortOrder
Confirms that `GetTrendingAsync` delegates to the marketplace service with a sort order based on download count. The test verifies that the correct sorting parameter is passed to the underlying marketplace client.

### GetTrendingAsync_ClampedLimit_NeverExceeds50
Validates that any requested limit for trending plugins is clamped to a maximum of 50. The test supplies a limit greater than 50 and asserts that the actual number of returned items does not exceed 50.

### BrowseCategoryAsync_PassesCategoryAsTag
Checks that `BrowseCategoryAsync` forwards the supplied category identifier as a tag filter to the marketplace query. The test ensures the category ID is correctly mapped to the tag parameter in the underlying request.

### BrowseCategoryAsync_EmptyCategoryId_ReturnsFailure
Verifies that providing an empty or null category identifier to `BrowseCategoryAsync` results in a failure response. The test asserts that the method does not proceed with an invalid request and returns an appropriate error result.

### GetFeaturedAsync_RequestsVerifiedAndRatedPlugins
Ensures that `GetFeaturedAsync` queries the marketplace for plugins that are both verified and highly rated. The test validates that the filtering criteria sent to the marketplace include verification status and rating thresholds.

### GetHomePageAsync_ReturnsAggregatedData
Confirms that `GetHomePageAsync` returns a properly aggregated homepage result containing multiple sections (such as featured, trending, and categories). The test asserts that the returned object is non-null and contains the expected composite data structure.

## Usage

```csharp
// Testing that trending results are clamped to a maximum of 50
[Test]
public async Task Trending_LargeLimit_IsClamped()
{
    var browser = new MarketplaceBrowser(marketplaceClientMock.Object);
    var result = await browser.GetTrendingAsync(limit: 200);
    
    Assert.That(result.Plugins.Count, Is.LessThanOrEqualTo(50));
}
```

```csharp
// Testing that an empty category ID produces a failure response
[Test]
public async Task Browse_EmptyCategory_HandledGracefully()
{
    var browser = new MarketplaceBrowser(marketplaceClientMock.Object);
    var result = await browser.BrowseCategoryAsync(categoryId: "");
    
    Assert.That(result.IsSuccess, Is.False);
    Assert.That(result.Error, Is.Not.Null);
}
```

## Notes

- **Caching behavior**: `GetCategoriesAsync` caches results internally. Tests that rely on cache state should account for potential stale data if the cache is not cleared between test runs.
- **Limit clamping**: The 50-item maximum for trending queries is enforced regardless of the input value. Callers passing a limit below 50 receive exactly that many items (subject to availability); values above 50 are silently reduced.
- **Error handling**: `BrowseCategoryAsync` with an empty category ID returns a failure result rather than throwing an exception. Callers should check the success flag before consuming the returned data.
- **Thread safety**: The test methods themselves are asynchronous and designed for single-threaded test execution. The underlying marketplace browser implementation may involve caching and shared state; concurrent access scenarios are not covered by this test suite and should be evaluated separately if required.
