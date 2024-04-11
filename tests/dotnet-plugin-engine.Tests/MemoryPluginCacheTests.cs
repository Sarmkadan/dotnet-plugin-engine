#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Caching;
using Xunit;

namespace PluginEngine.Tests;

public sealed class MemoryPluginCacheTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<MemoryPluginCache>> _mockLogger;
    private readonly MemoryPluginCache _sut;

    public MemoryPluginCacheTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<MemoryPluginCache>>();
        _sut = new MemoryPluginCache(_memoryCache, _mockLogger.Object);
    }

    public void Dispose() => _memoryCache.Dispose();

    [Fact]
    public async Task GetAsync_WhenKeyNotCached_ReturnsDefault()
    {
        var result = await _sut.GetAsync<string>("missing-key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsStoredValue()
    {
        await _sut.SetAsync("key1", "hello");

        var result = await _sut.GetAsync<string>("key1");

        result.Should().Be("hello");
    }

    [Fact]
    public async Task SetAsync_WithComplexObject_ReturnsStoredObject()
    {
        var obj = new { Name = "TestPlugin", Version = "1.0.0" };
        await _sut.SetAsync("complex", obj);

        var result = await _sut.GetAsync<object>("complex");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveAsync_AfterSet_ValueIsNoLongerReturned()
    {
        await _sut.SetAsync("to-remove", 42);
        await _sut.RemoveAsync("to-remove");

        var result = await _sut.GetAsync<int?>("to-remove");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ForNonExistentKey_DoesNotThrow()
    {
        var act = () => _sut.RemoveAsync("nonexistent");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetStatisticsAsync_AfterCacheHit_RecordsHit()
    {
        await _sut.SetAsync("stat-key", "value");
        await _sut.GetAsync<string>("stat-key");

        var stats = await _sut.GetStatisticsAsync();

        stats.TotalHits.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_AfterCacheMiss_RecordsMiss()
    {
        await _sut.GetAsync<string>("never-set");

        var stats = await _sut.GetStatisticsAsync();

        stats.TotalMisses.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CacheStatistics_HitRate_CalculatesCorrectly()
    {
        await _sut.SetAsync("hr-key", "v");
        await _sut.GetAsync<string>("hr-key");   // hit
        await _sut.GetAsync<string>("hr-key");   // hit
        await _sut.GetAsync<string>("no-key");   // miss

        var stats = await _sut.GetStatisticsAsync();

        stats.HitRate.Should().BeApproximately(2.0 / 3.0, precision: 0.01);
    }

    [Fact]
    public async Task ClearAsync_ResetsHitAndMissCounters()
    {
        await _sut.GetAsync<string>("any");
        await _sut.ClearAsync();

        var stats = await _sut.GetStatisticsAsync();

        stats.TotalHits.Should().Be(0);
        stats.TotalMisses.Should().Be(0);
    }

    [Fact]
    public async Task SetAsync_WithExplicitExpiration_DoesNotThrow()
    {
        var act = () => _sut.SetAsync("expiring", "value", TimeSpan.FromMinutes(5));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetAsync_WithoutExpiration_UsesDefaultAndStoresValue()
    {
        await _sut.SetAsync<int>("no-exp", 99);

        var result = await _sut.GetAsync<int>("no-exp");

        result.Should().Be(99);
    }

    [Fact]
    public async Task GetAsync_WithIntegerValue_ReturnsCorrectType()
    {
        await _sut.SetAsync("int-key", 123);

        var result = await _sut.GetAsync<int>("int-key");

        result.Should().Be(123);
    }

    [Fact]
    public async Task CacheStatistics_HitRate_IsZeroWithNoAccesses()
    {
        var stats = await _sut.GetStatisticsAsync();

        stats.HitRate.Should().Be(0);
    }
}
