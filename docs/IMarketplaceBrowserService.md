# IMarketplaceBrowserService

The `IMarketplaceBrowserService` interface defines the contract for accessing and browsing plugin marketplace data within the .NET Plugin Engine. It provides properties for caching static metadata such as categories and featured listings, alongside asynchronous methods for retrieving dynamic content like trending plugins, category-specific browsable items, and the complete marketplace home page configuration. Implementations of this service are responsible for aggregating `MarketplaceEntry` and `MarketplaceCategory` data, handling network or local storage retrieval, and wrapping results in `PluginOperationResult` to indicate success or failure states.

## API

### Properties

#### `string Id`
Gets the unique identifier for the marketplace instance represented by this service. This value is typically used to distinguish between different marketplace sources (e.g., official vs. community repositories).

#### `string Name`
Gets the human-readable display name of the marketplace.

#### `string Description`
Gets a textual description providing context or details about the marketplace's scope and content policies.

#### `string Icon`
Gets the URI or path string pointing to the marketplace's icon asset.

#### `int PluginCount`
Gets the total number of plugins currently available in this marketplace. This value may be based on the last successful data refresh.

#### `List<MarketplaceEntry> Featured`
Gets a list of plugins currently marked as "featured" by the marketplace curators. This list is typically populated during the service initialization or the last home page refresh.

#### `List<MarketplaceEntry> Trending`
Gets a list of plugins currently identified as "trending" based on download statistics or user activity. Like `Featured`, this reflects the state at the time of the last data synchronization.

#### `List<MarketplaceCategory> Categories`
Gets the complete list of available plugin categories. This collection is used to populate navigation filters and is generally static between service refreshes.

#### `DateTime GeneratedAtUtc`
Gets the UTC timestamp indicating when the current cached data (lists and counts) was last generated or synchronized. Consumers can use this to determine data freshness.

#### `MarketplaceBrowserService`
*Note: Based on the provided signature, this member appears as a property returning the concrete type `MarketplaceBrowserService`. In typical interface definitions, this may represent a typed reference to the underlying implementation or a specific configuration object exposed by the interface.*
Gets the underlying concrete implementation instance or configuration context associated with this interface.

### Methods

#### `Task<PluginOperationResult<List<MarketplaceCategory>>> GetCategoriesAsync()`
Retrieves the list of available plugin categories asynchronously.
*   **Parameters**: None.
*   **Return Value**: A `Task` wrapping a `PluginOperationResult`. On success, the `Result` property contains a `List<MarketplaceCategory>`. On failure, the `IsSuccess` property is false, and error details are provided within the result object.
*   **Exceptions**: Throws standard async exceptions (e.g., `OperationCanceledException`) if the cancellation token is triggered. Network or I/O errors are captured within the `PluginOperationResult` rather than thrown directly.

#### `Task<PluginOperationResult<List<MarketplaceEntry>>> GetTrendingAsync()`
Fetches the current list of trending plugins asynchronously.
*   **Parameters**: None.
*   **Return Value**: A `Task` wrapping a `PluginOperationResult`. On success, the `Result` contains a `List<MarketplaceEntry>` sorted by trending metrics.
*   **Exceptions**: Throws standard async exceptions for cancellation. Data retrieval failures are returned via the `PluginOperationResult`.

#### `Task<PluginOperationResult<List<MarketplaceEntry>>> GetFeaturedAsync()`
Fetches the current list of featured plugins asynchronously.
*   **Parameters**: None.
*   **Return Value**: A `Task` wrapping a `PluginOperationResult`. On success, the `Result` contains a `List<MarketplaceEntry>` designated as featured.
*   **Exceptions**: Throws standard async exceptions for cancellation. Data retrieval failures are returned via the `PluginOperationResult`.

#### `Task<PluginOperationResult<List<MarketplaceEntry>>> BrowseCategoryAsync(string categoryId)`
Retrieves a list of plugins belonging to a specific category.
*   **Parameters**:
    *   `categoryId`: The unique identifier of the category to browse.
*   **Return Value**: A `Task` wrapping a `PluginOperationResult`. On success, the `Result` contains a `List<MarketplaceEntry>` matching the specified category. If the category ID is invalid or empty, the result may indicate failure or return an empty list depending on implementation specifics.
*   **Exceptions**: Throws standard async exceptions for cancellation. Invalid arguments or remote errors are encapsulated in the `PluginOperationResult`.

#### `Task<PluginOperationResult<MarketplaceHomePage>> GetHomePageAsync()`
Retrieves the comprehensive data model for the marketplace home page.
*   **Parameters**: None.
*   **Return Value**: A `Task` wrapping a `PluginOperationResult`. On success, the `Result` contains a `MarketplaceHomePage` object, which typically aggregates featured, trending, and category data into a single view model.
*   **Exceptions**: Throws standard async exceptions for cancellation. Aggregation errors are returned via the `PluginOperationResult`.

## Usage

### Example 1: Initializing and Displaying Marketplace Metadata
This example demonstrates how to inject the service, check the data freshness using `GeneratedAtUtc`, and display basic marketplace information along with the cached categories.

```csharp
public class MarketplaceDashboard
{
    private readonly IMarketplaceBrowserService _marketplaceService;

    public MarketplaceDashboard(IMarketplaceBrowserService marketplaceService)
    {
        _marketplaceService = marketplaceService;
    }

    public async Task RenderDashboardAsync()
    {
        // Access static properties directly
        Console.WriteLine($"Marketplace: {_marketplaceService.Name}");
        Console.WriteLine($"Total Plugins: {_marketplaceService.PluginCount}");
        Console.WriteLine($"Data Generated At: {_marketplaceService.GeneratedAtUtc}");

        // Use cached categories if available, otherwise fetch fresh data
        var categories = _marketplaceService.Categories;
        
        if (categories == null || !categories.Any())
        {
            var result = await _marketplaceService.GetCategoriesAsync();
            if (result.IsSuccess)
            {
                categories = result.Value;
            }
            else
            {
                Console.WriteLine($"Failed to load categories: {result.ErrorMessage}");
                return;
            }
        }

        foreach (var category in categories)
        {
            Console.WriteLine($"- {category.Name}");
        }
    }
}
```

### Example 2: Browsing Content and Handling Results
This example illustrates fetching trending plugins and browsing a specific category, properly handling the `PluginOperationResult` pattern to ensure robust error handling without relying on try-catch blocks for logical failures.

```csharp
public class PluginExplorer
{
    private readonly IMarketplaceBrowserService _browser;

    public PluginExplorer(IMarketplaceBrowserService browser)
    {
        _browser = browser;
    }

    public async Task ExploreAsync(string targetCategoryId)
    {
        // Fetch trending plugins
        var trendingResult = await _browser.GetTrendingAsync();
        if (trendingResult.IsSuccess)
        {
            Console.WriteLine("Trending Plugins:");
            foreach (var plugin in trendingResult.Value.Take(5))
            {
                Console.WriteLine($"  * {plugin.Name} by {plugin.Author}");
            }
        }
        else
        {
            Console.WriteLine($"Could not retrieve trending plugins: {trendingResult.ErrorMessage}");
        }

        // Browse a specific category
        var categoryResult = await _browser.BrowseCategoryAsync(targetCategoryId);
        if (categoryResult.IsSuccess)
        {
            var plugins = categoryResult.Value;
            Console.WriteLine($"Found {plugins.Count} plugins in category {targetCategoryId}.");
            
            // Process plugins...
        }
        else
        {
            Console.WriteLine($"Error browsing category: {categoryResult.ErrorMessage}");
        }
    }
}
```

## Notes

*   **Data Consistency**: The properties `Featured`, `Trending`, and `Categories` represent a snapshot of data at the time of the last synchronization (indicated by `GeneratedAtUtc`). They may not reflect real-time changes in the remote repository. For the most up-to-date information, prefer calling the corresponding `Get...Async` methods.
*   **Error Handling Pattern**: All asynchronous methods return `Task<PluginOperationResult<T>>`. Consumers should inspect the `IsSuccess` property of the result before accessing the `Value` or `Result` data. Network failures, timeouts, and logical errors (e.g., category not found) are generally encapsulated within the result object rather than thrown as exceptions, though standard .NET async exceptions (like `OperationCanceledException`) may still be thrown.
*   **Thread Safety**: Implementations of `IMarketplaceBrowserService` should be treated as thread-safe for read operations on properties. However, if multiple threads trigger `Get...Async` methods simultaneously, race conditions regarding cache invalidation or concurrent network requests may occur depending on the specific implementation of `MarketplaceBrowserService`. It is recommended to serialize write/update operations or rely on the internal concurrency controls of the concrete service.
*   **Nullability**: While the `List<T>` properties are initialized in valid implementations, consumers should verify that `Featured`, `Trending`, and `Categories` are not null before enumeration, especially if the service has not yet completed its initial data load.
