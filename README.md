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

## IPluginMarketplaceService

The `IPluginMarketplaceService` is the core contract for interacting with the plugin marketplace. It provides methods for searching plugins, retrieving plugin details, checking version compatibility, and installing plugins from the marketplace.

Here's a realistic usage example:

```csharp
using PluginEngine.Marketplace;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddMemoryCache();
services.AddLogging(configure => configure.AddConsole());
services.AddPluginEngineMarketplace();

var serviceProvider = services.BuildServiceProvider();
var marketplaceService = serviceProvider.GetRequiredService<IPluginMarketplaceService>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

// Example 1: Search for plugins matching a query
var searchResult = await marketplaceService.SearchAsync(
    new MarketplaceSearchFilter { Query = "logging", PageSize = 20 }
);

if (searchResult.Success)
{
    Console.WriteLine($"Found {searchResult.Data?.Count} plugins matching 'logging'");
    foreach (var entry in searchResult.Data ?? [])
    {
        Console.WriteLine($"- {entry.Name} (v{entry.LatestVersion}) by {entry.Author}");
    }
}

// Example 2: Get detailed information about a specific plugin
guid samplePluginId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");
var entryResult = await marketplaceService.GetEntryAsync(samplePluginId);

if (entryResult.Success && entryResult.Data != null)
{
    var pluginEntry = entryResult.Data;
    Console.WriteLine($"Plugin: {pluginEntry.Name}");
    Console.WriteLine($"Description: {pluginEntry.Description}");
    Console.WriteLine($"Available versions: {string.Join(", ", pluginEntry.AvailableVersions?.Select(v => v.Version) ?? [])}");
}

// Example 3: Check compatibility between a plugin version and engine version
var compatibilityResult = await marketplaceService.CheckCompatibilityAsync(
    samplePluginId,
    "1.2.3",
    "9.0"
);

if (compatibilityResult.Success && compatibilityResult.Data != null)
{
    Console.WriteLine($"Compatibility: {compatibilityResult.Data}");
}

// Example 4: Install a plugin to a target directory
var installResult = await marketplaceService.InstallAsync(
    samplePluginId,
    "1.2.3",
    @"./plugins/my-plugin"
);

if (installResult.Success)
{
    Console.WriteLine($"Plugin installed successfully: {installResult.Message}");
}
else
{
    logger.LogError("Installation failed: {Message}", installResult.Message);
}
```
## RateLimitMiddleware

The `RateLimitMiddleware` provides rate limiting for plugin operations to prevent resource exhaustion. It uses a token bucket algorithm to enforce fair rate limiting across different plugins, allowing a configurable number of operations per time window.

Here's a realistic usage example:

```csharp
using PluginEngine.Middleware;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPluginEngine();

var serviceProvider = services.BuildServiceProvider();
var pipeline = serviceProvider.GetRequiredService<PluginMiddlewarePipeline>();

// Configure rate limiting: 50 operations per second with a 2-second window
pipeline.UseRateLimit(maxTokensPerSecond: 50, windowSizeSeconds: 2);

// The middleware will now enforce rate limits on all plugin operations
// Each plugin gets its own token bucket, and tokens refill at the configured rate

// You can also configure it globally with default values (100 operations per second)
pipeline.UseRateLimit();
```

## CachingMiddleware

The `CachingMiddleware` caches successful plugin operation results to reduce repeated processing and improve performance. It automatically caches operations like `GetMetadata`, `ResolveDependencies`, and `ValidateVersion`, and provides methods to invalidate cache entries either by plugin or globally.

Here's a realistic usage example:

```csharp
using PluginEngine.Middleware;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection with memory cache
var services = new ServiceCollection();
services.AddMemoryCache();
services.AddPluginEngine();

var serviceProvider = services.BuildServiceProvider();
var pipeline = serviceProvider.GetRequiredService<PluginMiddlewarePipeline>();

// Add caching middleware with default 5-minute cache duration
pipeline.UseCaching();

// You can also customize the cache duration
pipeline.UseCaching(TimeSpan.FromMinutes(10));

// To manually invalidate cache for a specific plugin
var pluginId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
var cachingMiddleware = pipeline.Middlewares.OfType<CachingMiddleware>().FirstOrDefault();
cachingMiddleware?.InvalidatePluginCache(pluginId);
```

## IPluginMiddleware

The `IPluginMiddleware` interface defines the contract for plugin operation middleware components. Middleware processes plugin operations before and after execution, enabling cross-cutting concerns like logging, error handling, performance tracking, and validation. Middleware components are composed into a pipeline using `PluginMiddlewarePipeline`, allowing flexible middleware composition.


Here's a realistic usage example implementing a custom logging middleware:


```csharp
using PluginEngine.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

// Custom middleware that logs plugin operation timing and exceptions
public class LoggingMiddleware : IPluginMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    
    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        context.StartTimeMs = stopwatch.ElapsedMilliseconds;
        
        try
        {
            await next(context);
            context.IsSuccessful = true;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.IsSuccessful = false;
            _logger.LogError(ex, "Plugin operation failed: {OperationType}", context.OperationType);
            throw;
        }
        finally
        {
            context.EndTimeMs = stopwatch.ElapsedMilliseconds;
            
            var durationMs = context.EndTimeMs - context.StartTimeMs;
            _logger.LogInformation(
                "Plugin operation {OperationType} for {PluginName} completed in {DurationMs}ms. Status: {(context.IsSuccessful ? "Success" : "Failed")}",
                context.OperationType,
                context.Plugin.Name,
                durationMs,
                context.IsSuccessful ? "Success" : "Failed"
            );
        }
    }
}

// Usage in a plugin engine pipeline
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddPluginEngine();

var serviceProvider = services.BuildServiceProvider();
var pipeline = serviceProvider.GetRequiredService<PluginMiddlewarePipeline>();

// Add your custom logging middleware
pipeline.Use(async (next) => 
    new LoggingMiddleware(serviceProvider.GetRequiredService<ILogger<LoggingMiddleware>>())(context, next)
);

// Or use the middleware directly with the pipeline's Use method
pipeline.Use<LoggingMiddleware>();

// The middleware pipeline can now process plugin operations with logging
```
