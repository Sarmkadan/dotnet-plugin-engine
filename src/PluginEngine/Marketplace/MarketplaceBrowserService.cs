#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Marketplace;

/// <summary>
/// Contract for rich marketplace browsing: categories, trending, and featured listings.
/// Builds on top of <see cref="IPluginMarketplaceService"/> to provide discovery-oriented APIs.
/// </summary>
public interface IMarketplaceBrowserService
{
    /// <summary>
    /// Returns the full list of available plugin categories with item counts.
    /// Results are cached for 6 hours.
    /// </summary>
    Task<PluginOperationResult<List<MarketplaceCategory>>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns plugins trending by download count, ordered descending.
    /// </summary>
    /// <param name="limit">Maximum number of results to return (1–50).</param>
    Task<PluginOperationResult<List<MarketplaceEntry>>> GetTrendingAsync(
        int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns curated featured plugins (verified + highest rated).
    /// </summary>
    Task<PluginOperationResult<List<MarketplaceEntry>>> GetFeaturedAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Browses plugins within a specific category with optional sorting and pagination.
    /// </summary>
    /// <param name="categoryId">The category slug to browse (e.g., "logging").</param>
    /// <param name="filter">Optional additional search/pagination parameters.</param>
    Task<PluginOperationResult<List<MarketplaceEntry>>> BrowseCategoryAsync(
        string categoryId,
        MarketplaceSearchFilter? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a combined home-page snapshot (featured + trending + categories) in one call.
    /// Cached for 10 minutes to reduce registry round-trips.
    /// </summary>
    Task<PluginOperationResult<MarketplaceHomePage>> GetHomePageAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a named plugin category in the marketplace.
/// </summary>
public sealed class MarketplaceCategory
{
    /// <summary>Gets or sets the unique category slug (e.g., "logging").</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable category name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a short description of the category.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the icon name or emoji used for display.</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of plugins in this category.</summary>
    public int PluginCount { get; set; }
}

/// <summary>
/// Aggregated home-page response combining featured plugins, trending plugins, and categories.
/// </summary>
public sealed class MarketplaceHomePage
{
    /// <summary>Gets or sets the curated featured plugins.</summary>
    public List<MarketplaceEntry> Featured { get; set; } = [];

    /// <summary>Gets or sets the currently trending plugins.</summary>
    public List<MarketplaceEntry> Trending { get; set; } = [];

    /// <summary>Gets or sets all available plugin categories.</summary>
    public List<MarketplaceCategory> Categories { get; set; } = [];

    /// <summary>Gets or sets when this snapshot was generated (UTC).</summary>
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Default implementation of <see cref="IMarketplaceBrowserService"/> that aggregates data
/// from <see cref="IPluginMarketplaceService"/>, applying multi-level caching to keep
/// discovery-heavy workloads responsive.
/// </summary>
public sealed class MarketplaceBrowserService : IMarketplaceBrowserService
{
    private readonly IPluginMarketplaceService _marketplace;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MarketplaceBrowserService> _logger;

    private static readonly TimeSpan CategoriesTtl = TimeSpan.FromHours(6);
    private static readonly TimeSpan TrendingTtl   = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan FeaturedTtl   = TimeSpan.FromHours(1);
    private static readonly TimeSpan HomePageTtl   = TimeSpan.FromMinutes(10);

    private static readonly List<MarketplaceCategory> BuiltInCategories =
    [
        new() { Id = "analytics",      Name = "Analytics",      Description = "Data analysis and reporting",       Icon = "📊" },
        new() { Id = "authentication", Name = "Authentication", Description = "Identity and access management",    Icon = "🔐" },
        new() { Id = "caching",        Name = "Caching",        Description = "Caching and memoization",           Icon = "⚡" },
        new() { Id = "database",       Name = "Database",       Description = "Database connectivity and ORMs",    Icon = "🗄️" },
        new() { Id = "logging",        Name = "Logging",        Description = "Logging and observability",         Icon = "📝" },
        new() { Id = "messaging",      Name = "Messaging",      Description = "Event bus and message brokers",     Icon = "✉️" },
        new() { Id = "security",       Name = "Security",       Description = "Encryption and security auditing",  Icon = "🛡️" },
        new() { Id = "ui",             Name = "UI",             Description = "User interface components",         Icon = "🖥️" },
    ];

    /// <summary>Initialises a new instance of <see cref="MarketplaceBrowserService"/>.</summary>
    public MarketplaceBrowserService(
        IPluginMarketplaceService marketplace,
        IMemoryCache cache,
        ILogger<MarketplaceBrowserService> logger)
    {
        _marketplace = marketplace ?? throw new ArgumentNullException(nameof(marketplace));
        _cache       = cache       ?? throw new ArgumentNullException(nameof(cache));
        _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<PluginOperationResult<List<MarketplaceCategory>>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue("mp_categories", out List<MarketplaceCategory>? cached) && cached is not null)
            return Task.FromResult(
                PluginOperationResult<List<MarketplaceCategory>>.CreateSuccess(cached, $"Returned {cached.Count} categories."));

        var categories = new List<MarketplaceCategory>(BuiltInCategories);
        _cache.Set("mp_categories", categories, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CategoriesTtl
        });

        _logger.LogDebug("Returning {Count} marketplace categories", categories.Count);
        return Task.FromResult(
            PluginOperationResult<List<MarketplaceCategory>>.CreateSuccess(categories, $"Returned {categories.Count} categories."));
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult<List<MarketplaceEntry>>> GetTrendingAsync(
        int limit = 10, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 50);
        var cacheKey = $"mp_trending_{limit}";

        if (_cache.TryGetValue(cacheKey, out List<MarketplaceEntry>? cached) && cached is not null)
            return PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess(cached, $"Trending: {cached.Count} plugin(s).");

        try
        {
            var searchResult = await _marketplace.SearchAsync(new MarketplaceSearchFilter
            {
                SortOrder = MarketplaceSortOrder.Downloads,
                PageSize  = limit
            }, cancellationToken);

            if (!searchResult.Success)
                return searchResult;

            var trending = searchResult.Data ?? [];
            _cache.Set(cacheKey, trending, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TrendingTtl
            });

            _logger.LogDebug("Fetched {Count} trending plugins", trending.Count);
            return PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess(trending, $"Trending: {trending.Count} plugin(s).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch trending plugins");
            return PluginOperationResult<List<MarketplaceEntry>>.FromException(ex);
        }
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult<List<MarketplaceEntry>>> GetFeaturedAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue("mp_featured", out List<MarketplaceEntry>? cached) && cached is not null)
            return PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess(cached, $"Featured: {cached.Count} plugin(s).");

        try
        {
            var searchResult = await _marketplace.SearchAsync(new MarketplaceSearchFilter
            {
                OnlyVerified = true,
                SortOrder    = MarketplaceSortOrder.Rating,
                PageSize     = 6
            }, cancellationToken);

            if (!searchResult.Success)
                return searchResult;

            var featured = searchResult.Data ?? [];
            _cache.Set("mp_featured", featured, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = FeaturedTtl
            });

            _logger.LogDebug("Fetched {Count} featured plugins", featured.Count);
            return PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess(featured, $"Featured: {featured.Count} plugin(s).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch featured plugins");
            return PluginOperationResult<List<MarketplaceEntry>>.FromException(ex);
        }
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult<List<MarketplaceEntry>>> BrowseCategoryAsync(
        string categoryId,
        MarketplaceSearchFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
            return PluginOperationResult<List<MarketplaceEntry>>.CreateFailure("Category ID is required.", 400);

        try
        {
            var effectiveFilter = filter ?? new MarketplaceSearchFilter();
            if (!effectiveFilter.Tags.Contains(categoryId, StringComparer.OrdinalIgnoreCase))
                effectiveFilter.Tags = [categoryId, ..effectiveFilter.Tags];

            var result = await _marketplace.SearchAsync(effectiveFilter, cancellationToken);

            _logger.LogDebug("Browse '{CategoryId}' returned {Count} plugin(s)",
                categoryId, result.Data?.Count ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to browse category '{CategoryId}'", categoryId);
            return PluginOperationResult<List<MarketplaceEntry>>.FromException(ex);
        }
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult<MarketplaceHomePage>> GetHomePageAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue("mp_homepage", out MarketplaceHomePage? cached) && cached is not null)
            return PluginOperationResult<MarketplaceHomePage>.CreateSuccess(cached, "Home page served from cache.");

        try
        {
            var categoriesTask = GetCategoriesAsync(cancellationToken);
            var trendingTask   = GetTrendingAsync(5, cancellationToken);
            var featuredTask   = GetFeaturedAsync(cancellationToken);

            await Task.WhenAll(categoriesTask, trendingTask, featuredTask);

            var categories = await categoriesTask;
            var trending   = await trendingTask;
            var featured   = await featuredTask;

            var page = new MarketplaceHomePage
            {
                Categories = categories.Data ?? [],
                Trending   = trending.Data   ?? [],
                Featured   = featured.Data   ?? [],
            };

            _cache.Set("mp_homepage", page, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = HomePageTtl
            });

            _logger.LogInformation(
                "Marketplace home page assembled: {F} featured, {T} trending, {C} categories",
                page.Featured.Count, page.Trending.Count, page.Categories.Count);

            return PluginOperationResult<MarketplaceHomePage>.CreateSuccess(page, "Home page assembled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assemble marketplace home page");
            return PluginOperationResult<MarketplaceHomePage>.FromException(ex);
        }
    }
}
