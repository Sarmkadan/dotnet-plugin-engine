// ... existing content ...

## IMarketplaceBrowserService

The `IMarketplaceBrowserService` provides a rich API for browsing plugins in the marketplace, including categories, trending plugins, and featured plugins. It abstracts the data access and caching for a better performance.

Here's an usage example:
```csharp
using PluginEngine.Marketplace;

// Assuming you have injected IMarketplaceBrowserService
var marketplaces = new MarketplaceBrowserService(marketplaceService, cache, logger);

var categoriesResult = await marketplaces.GetCategoriesAsync();
if (categoriesResult.Success)
{
    foreach (var category in categoriesResult.Data)
    {
        Console.WriteLine($"Category: {category.Name} ({category.PluginCount} plugins)");
    }
}

var trendingResult = await marketplaces.GetTrendingAsync(limit: 10);
if (trendingResult.Success)
{
    Console.WriteLine("Trending plugins:");
    foreach (var plugin in trendingResult.Data)
    {
        Console.WriteLine($"- {plugin.Name} ({plugin.LatestVersion})");
    }
}

var featuredResult = await marketplaces.GetFeaturedAsync();
if (featuredResult.Success)
{
    Console.WriteLine("Featured plugins:");
    foreach (var plugin in featuredResult.Data)
    {
        Console.WriteLine($"- {plugin.Name} ({plugin.LatestVersion})");
    }
}

var homePageResult = await marketplaces.GetHomePageAsync();
if (homePageResult.Success)
{
    var homePage = homePageResult.Data;
    Console.WriteLine($"Home page generated at: {homePage.GeneratedAtUtc}");
    Console.WriteLine($"Featured plugins: {homePage.Featured.Count}");
    Console.WriteLine($"Trending plugins: {homePage.Trending.Count}");
    Console.WriteLine($"Categories: {homePage.Categories.Count}");
}
```