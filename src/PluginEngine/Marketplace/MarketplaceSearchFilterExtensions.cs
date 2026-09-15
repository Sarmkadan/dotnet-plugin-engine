#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace PluginEngine.Marketplace;

/// <summary>
/// Fluent helper extensions for <see cref="MarketplaceSearchFilter"/>.
/// </summary>
public static class MarketplaceSearchFilterExtensions
{
    /// <summary>
    /// Sets the free-text search query.
    /// </summary>
    /// <param name="filter">The filter to modify.</param>
    /// <param name="query">The search query (can be null or empty to clear).</param>
    /// <returns>The same filter instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
    public static MarketplaceSearchFilter WithQuery(this MarketplaceSearchFilter filter, string? query)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        filter.Query = query;
        return filter;
    }

    /// <summary>
    /// Sets the tags that results must contain at least one of.
    /// </summary>
    /// <param name="filter">The filter to modify.</param>
    /// <param name="tags">The tags (can be null or empty to clear).</param>
    /// <returns>The same filter instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
    public static MarketplaceSearchFilter WithTags(this MarketplaceSearchFilter filter, params string[]? tags)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        filter.Tags = tags?.ToList() ?? new List<string>();
        return filter;
    }

    /// <summary>
    /// Sets the desired sort order for returned results.
    /// </summary>
    /// <param name="filter">The filter to modify.</param>
    /// <param name="sortOrder">The sort order to apply.</param>
    /// <returns>The same filter instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
    public static MarketplaceSearchFilter SortedBy(this MarketplaceSearchFilter filter, MarketplaceSortOrder sortOrder)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        filter.SortOrder = sortOrder;
        return filter;
    }

    /// <summary>
    /// Sets the pagination parameters.
    /// </summary>
    /// <param name="filter">The filter to modify.</param>
    /// <param name="page">The 1-based page number (must be >= 1).</param>
    /// <param name="pageSize">The number of results per page (must be between 1 and 100).</param>
    /// <returns>The same filter instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> is less than 1 or <paramref name="pageSize"/> is not between 1 and 100.</exception>
    public static MarketplaceSearchFilter Page(this MarketplaceSearchFilter filter, int page, int pageSize)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page number must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");

        filter.Page = page;
        filter.PageSize = pageSize;
        return filter;
    }

    /// <summary>
    /// Limits results to only marketplace-verified plugins.
    /// </summary>
    /// <param name="filter">The filter to modify.</param>
    /// <returns>The same filter instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
    public static MarketplaceSearchFilter VerifiedOnly(this MarketplaceSearchFilter filter)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        filter.OnlyVerified = true;
        return filter;
    }

    /// <summary>
    /// Determines whether the filter has no query or tags specified.
    /// </summary>
    /// <param name="filter">The filter to check.</param>
    /// <returns>True if the filter has no query and no tags; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
    public static bool IsEmpty(this MarketplaceSearchFilter filter)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        return string.IsNullOrWhiteSpace(filter.Query) && filter.Tags.Count == 0;
    }
}