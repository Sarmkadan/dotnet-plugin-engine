#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Marketplace;
using PluginEngine.Results;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Unit tests for <see cref="MarketplaceBrowserService"/>.
/// </summary>
public sealed class MarketplaceBrowserTests
{
    private readonly Mock<IPluginMarketplaceService> _mockMarketplace = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly Mock<ILogger<MarketplaceBrowserService>> _mockLogger = new();

    private MarketplaceBrowserService CreateService() =>
        new(_mockMarketplace.Object, _cache, _mockLogger.Object);

    // ── GetCategoriesAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetCategoriesAsync_ReturnsBuiltInCategories()
    {
        var service = CreateService();

        var result = await service.GetCategoriesAsync();

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().BeGreaterThan(0);
        result.Data.Should().Contain(c => c.Id == "logging");
        result.Data.Should().Contain(c => c.Id == "database");
    }

    [Fact]
    public async Task GetCategoriesAsync_CachesResult_SecondCallReturnsSameData()
    {
        var service = CreateService();

        var first  = await service.GetCategoriesAsync();
        var second = await service.GetCategoriesAsync();

        first.Data.Should().BeEquivalentTo(second.Data);
        // Marketplace is never called — data comes from built-in list
        _mockMarketplace.Verify(m => m.SearchAsync(It.IsAny<MarketplaceSearchFilter>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── GetTrendingAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetTrendingAsync_DelegatesToMarketplaceWithDownloadsSortOrder()
    {
        var entries = new List<MarketplaceEntry>
        {
            new() { Id = Guid.NewGuid(), Name = "TopPlugin", LatestVersion = "1.0.0" }
        };

        _mockMarketplace
            .Setup(m => m.SearchAsync(
                It.Is<MarketplaceSearchFilter>(f => f.SortOrder == MarketplaceSortOrder.Downloads),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess(entries, "ok"));

        var service = CreateService();
        var result  = await service.GetTrendingAsync(5);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].Name.Should().Be("TopPlugin");
    }

    [Fact]
    public async Task GetTrendingAsync_ClampedLimit_NeverExceeds50()
    {
        MarketplaceSearchFilter? capturedFilter = null;

        _mockMarketplace
            .Setup(m => m.SearchAsync(It.IsAny<MarketplaceSearchFilter>(), It.IsAny<CancellationToken>()))
            .Callback<MarketplaceSearchFilter, CancellationToken>((f, _) => capturedFilter = f)
            .ReturnsAsync(PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess([], "ok"));

        var service = CreateService();
        await service.GetTrendingAsync(999);

        capturedFilter.Should().NotBeNull();
        capturedFilter!.PageSize.Should().Be(50);
    }

    // ── BrowseCategoryAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task BrowseCategoryAsync_PassesCategoryAsTag()
    {
        MarketplaceSearchFilter? capturedFilter = null;

        _mockMarketplace
            .Setup(m => m.SearchAsync(It.IsAny<MarketplaceSearchFilter>(), It.IsAny<CancellationToken>()))
            .Callback<MarketplaceSearchFilter, CancellationToken>((f, _) => capturedFilter = f)
            .ReturnsAsync(PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess([], "ok"));

        var service = CreateService();
        await service.BrowseCategoryAsync("logging");

        capturedFilter!.Tags.Should().Contain("logging");
    }

    [Fact]
    public async Task BrowseCategoryAsync_EmptyCategoryId_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.BrowseCategoryAsync(string.Empty);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(400);
    }

    // ── GetFeaturedAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetFeaturedAsync_RequestsVerifiedAndRatedPlugins()
    {
        MarketplaceSearchFilter? capturedFilter = null;

        _mockMarketplace
            .Setup(m => m.SearchAsync(It.IsAny<MarketplaceSearchFilter>(), It.IsAny<CancellationToken>()))
            .Callback<MarketplaceSearchFilter, CancellationToken>((f, _) => capturedFilter = f)
            .ReturnsAsync(PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess([], "ok"));

        var service = CreateService();
        await service.GetFeaturedAsync();

        capturedFilter!.OnlyVerified.Should().BeTrue();
        capturedFilter.SortOrder.Should().Be(MarketplaceSortOrder.Rating);
    }

    // ── GetHomePageAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetHomePageAsync_ReturnsAggregatedData()
    {
        _mockMarketplace
            .Setup(m => m.SearchAsync(It.IsAny<MarketplaceSearchFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess([], "ok"));

        var service = CreateService();
        var result  = await service.GetHomePageAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Categories.Should().NotBeEmpty();
        result.Data.GeneratedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
