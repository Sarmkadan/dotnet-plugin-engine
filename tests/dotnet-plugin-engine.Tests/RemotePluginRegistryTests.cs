#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Integration;
using PluginEngine.Utils.Helpers;
using Xunit;

namespace PluginEngine.Tests;

public sealed class RemotePluginRegistryTests
{
    private readonly Mock<HttpPluginClient> _httpClientMock = new();
    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly Mock<ILogger<RemotePluginRegistry>> _loggerMock = new();
    private readonly Mock<VersionHelper> _versionHelperMock = new(MockBehavior.Strict);
    private readonly RemotePluginRegistry _registry;

    public RemotePluginRegistryTests()
    {
        _registry = new RemotePluginRegistry(
            _httpClientMock.Object,
            _cache,
            _loggerMock.Object,
            _versionHelperMock.Object);
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ReturnsPluginsFromHttpClient()
    {
        // Arrange
        var expectedPlugins = new List<PluginInfo>
        {
            new() { Id = Guid.NewGuid(), Name = "TestPlugin1", Version = "1.0.0", DownloadUrl = "https://example.com/plugin1.zip" },
            new() { Id = Guid.NewGuid(), Name = "TestPlugin2", Version = "2.0.0", DownloadUrl = "https://example.com/plugin2.zip" }
        };

        _httpClientMock.Setup(x => x.SearchPluginsAsync("test", 10))
            .ReturnsAsync(expectedPlugins);

        // Act
        var result = await _registry.SearchAsync("test", 10);

        // Assert
        result.Should().BeEquivalentTo(expectedPlugins);
        _cache.Get("registry_search_test_10").Should().BeEquivalentTo(expectedPlugins);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registry search for 'test' returned 2 result(s)")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()!), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithCachedResult_ReturnsCachedPlugins()
    {
        // Arrange
        var cachedPlugins = new List<PluginInfo>
        {
            new() { Id = Guid.NewGuid(), Name = "CachedPlugin", Version = "1.0.0", DownloadUrl = "https://example.com/cached.zip" }
        };

        _cache.Set("registry_search_query_5", cachedPlugins, new MemoryCacheEntryOptions());

        // Act
        var result = await _registry.SearchAsync("query", 5);

        // Assert
        result.Should().BeEquivalentTo(cachedPlugins);
        _httpClientMock.Verify(x => x.SearchPluginsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentException()
    {
        // Act
        var act = () => _registry.SearchAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "because null query should throw ArgumentException");
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ThrowsArgumentException()
    {
        // Act
        var act = () => _registry.SearchAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "because whitespace query should throw ArgumentException");
    }

    [Fact]
    public async Task SearchAsync_WithNonPositiveLimit_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _registry.SearchAsync("test", 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>(
            "because limit must be positive");
    }

    [Fact]
    public async Task SearchAsync_WithNegativeLimit_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _registry.SearchAsync("test", -1);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>(
            "because negative limit should throw ArgumentOutOfRangeException");
    }

    [Fact]
    public async Task GetPluginAsync_WithValidPluginId_ReturnsPluginInfo()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var expectedPlugin = new PluginInfo
        {
            Id = pluginId,
            Name = "TestPlugin",
            Version = "1.0.0",
            DownloadUrl = "https://example.com/test.zip"
        };

        _httpClientMock.Setup(x => x.GetPluginInfoAsync(pluginId))
            .ReturnsAsync(expectedPlugin);

        // Act
        var result = await _registry.GetPluginAsync(pluginId);

        // Assert
        result.Should().BeEquivalentTo(expectedPlugin);
        _cache.Get($"registry_plugin_{pluginId}").Should().BeEquivalentTo(expectedPlugin);
    }

    [Fact]
    public async Task GetPluginAsync_WithCachedResult_ReturnsCachedPlugin()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var cachedPlugin = new PluginInfo
        {
            Id = pluginId,
            Name = "CachedPlugin",
            Version = "1.0.0",
            DownloadUrl = "https://example.com/cached.zip"
        };

        _cache.Set($"registry_plugin_{pluginId}", cachedPlugin, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            SlidingExpiration = TimeSpan.FromMinutes(30)
        });

        // Act
        var result = await _registry.GetPluginAsync(pluginId);

        // Assert
        result.Should().BeEquivalentTo(cachedPlugin);
        _httpClientMock.Verify(x => x.GetPluginInfoAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetPluginAsync_WithNonExistentPlugin_ReturnsNull()
    {
        // Arrange
        var pluginId = Guid.NewGuid();

        _httpClientMock.Setup(x => x.GetPluginInfoAsync(pluginId))
            .ReturnsAsync((PluginInfo?)null);

        // Act
        var result = await _registry.GetPluginAsync(pluginId);

        // Assert
        result.Should().BeNull();
        _cache.Get($"registry_plugin_{pluginId}").Should().BeNull();
    }

    [Fact]
    public async Task GetVersionsAsync_WithValidPluginId_ReturnsVersions()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var expectedVersions = new List<PluginVersionInfo>
        {
            new() { Version = "1.0.0", PublishedAtUtc = DateTime.UtcNow.AddDays(-30), DownloadUrl = "https://example.com/v1.zip" },
            new() { Version = "2.0.0", PublishedAtUtc = DateTime.UtcNow.AddDays(-1), DownloadUrl = "https://example.com/v2.zip" }
        };

        _httpClientMock.Setup(x => x.GetPluginVersionsAsync(pluginId))
            .ReturnsAsync(expectedVersions);

        // Act
        var result = await _registry.GetVersionsAsync(pluginId);

        // Assert
        result.Should().BeEquivalentTo(expectedVersions);
        _cache.Get($"registry_versions_{pluginId}").Should().BeEquivalentTo(expectedVersions);
    }

    [Fact]
    public async Task GetVersionsAsync_WithCachedResult_ReturnsCachedVersions()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var cachedVersions = new List<PluginVersionInfo>
        {
            new() { Version = "1.0.0", PublishedAtUtc = DateTime.UtcNow, DownloadUrl = "https://example.com/v1.zip" }
        };

        _cache.Set($"registry_versions_{pluginId}", cachedVersions, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
        });

        // Act
        var result = await _registry.GetVersionsAsync(pluginId);

        // Assert
        result.Should().BeEquivalentTo(cachedVersions);
        _httpClientMock.Verify(x => x.GetPluginVersionsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetVersionsAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var pluginId = Guid.NewGuid();

        _httpClientMock.Setup(x => x.GetPluginVersionsAsync(pluginId))
            .ReturnsAsync(new List<PluginVersionInfo>());

        // Act
        var result = await _registry.GetVersionsAsync(pluginId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DownloadPluginAsync_WithValidParameters_DownloadsAndReturnsFilePath()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var version = "1.0.0";
        var downloadPath = "/tmp/plugins";
        var pluginInfo = new PluginInfo
        {
            Id = pluginId,
            Name = "DownloadablePlugin",
            Version = version,
            DownloadUrl = "https://example.com/plugin.dll"
        };

        _httpClientMock.Setup(x => x.GetAsync(pluginInfo.DownloadUrl))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("plugin content")
            });

        _httpClientMock.Setup(x => x.GetPluginInfoAsync(pluginId))
            .ReturnsAsync(pluginInfo);

        // Act
        var result = await _registry.DownloadPluginAsync(pluginId, version, downloadPath);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain(downloadPath);
        result.Should().Contain("DownloadablePlugin.1.0.0.dll");
        Directory.Exists(downloadPath).Should().BeTrue();
        File.Exists(result!).Should().BeTrue();
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Downloaded plugin:")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()!), Times.Once);
    }

    [Fact]
    public async Task DownloadPluginAsync_WithNullVersion_ThrowsArgumentException()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var downloadPath = "/tmp/plugins";

        // Act
        var act = () => _registry.DownloadPluginAsync(pluginId, null!, downloadPath);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "because null version should throw ArgumentException");
    }

    [Fact]
    public async Task DownloadPluginAsync_WithEmptyVersion_ThrowsArgumentException()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var downloadPath = "/tmp/plugins";

        // Act
        var act = () => _registry.DownloadPluginAsync(pluginId, "   ", downloadPath);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "because whitespace version should throw ArgumentException");
    }

    [Fact]
    public async Task DownloadPluginAsync_WithNullDownloadPath_ThrowsArgumentException()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var version = "1.0.0";

        // Act
        var act = () => _registry.DownloadPluginAsync(pluginId, version, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "because null download path should throw ArgumentException");
    }

    [Fact]
    public async Task DownloadPluginAsync_WithEmptyDownloadPath_ThrowsArgumentException()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var version = "1.0.0";

        // Act
        var act = () => _registry.DownloadPluginAsync(pluginId, version, "   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "because whitespace download path should throw ArgumentException");
    }

    [Fact]
    public async Task DownloadPluginAsync_WithNonExistentPlugin_ReturnsNull()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var version = "1.0.0";
        var downloadPath = "/tmp/plugins";

        _httpClientMock.Setup(x => x.GetPluginInfoAsync(pluginId))
            .ReturnsAsync((PluginInfo?)null);

        // Act
        var result = await _registry.DownloadPluginAsync(pluginId, version, downloadPath);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DownloadPluginAsync_WithNullDownloadUrl_ReturnsNull()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var version = "1.0.0";
        var downloadPath = "/tmp/plugins";
        var pluginInfo = new PluginInfo
        {
            Id = pluginId,
            Name = "PluginWithoutUrl",
            Version = version,
            DownloadUrl = null
        };

        _httpClientMock.Setup(x => x.GetPluginInfoAsync(pluginId))
            .ReturnsAsync(pluginInfo);

        // Act
        var result = await _registry.DownloadPluginAsync(pluginId, version, downloadPath);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No download URL found for plugin")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()!), Times.Once);
    }

    [Fact]
    public async Task DownloadPluginAsync_WithFailedDownload_ReturnsNull()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var version = "1.0.0";
        var downloadPath = "/tmp/plugins";
        var pluginInfo = new PluginInfo
        {
            Id = pluginId,
            Name = "PluginDownloadFailed",
            Version = version,
            DownloadUrl = "https://example.com/fail.zip"
        };

        _httpClientMock.Setup(x => x.GetAsync(pluginInfo.DownloadUrl))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));

        _httpClientMock.Setup(x => x.GetPluginInfoAsync(pluginId))
            .ReturnsAsync(pluginInfo);

        // Act
        var result = await _registry.DownloadPluginAsync(pluginId, version, downloadPath);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to download plugin")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()!), Times.Once);
    }

    [Fact]
    public async Task PublishPluginAsync_WithValidParameters_PublishesAndReturnsTrue()
    {
        // Arrange
        var filePath = "/tmp/test-plugin.dll";
        var metadata = new PluginPublishMetadata
        {
            PluginName = "TestPlugin",
            Version = "1.0.0",
            Description = "A test plugin",
            Author = "TestAuthor"
        };

        _httpClientMock.Setup(x => x.UploadPluginAsync(filePath, It.IsAny<string>()))
            .ReturnsAsync(true);

        File.WriteAllText(filePath, "test content");

        try
        {
            // Act
            var result = await _registry.PublishPluginAsync(filePath, metadata);

            // Assert
            result.Should().BeTrue();
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Published plugin to registry")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!), Times.Once);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task PublishPluginAsync_WithNullFilePath_ThrowsArgumentException()
    {
        // Arrange
        var metadata = new PluginPublishMetadata
        {
            PluginName = "TestPlugin",
            Version = "1.0.0",
            Description = "A test plugin",
            Author = "TestAuthor"
        };

        // Act
        var act = () => _registry.PublishPluginAsync(null!, metadata);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "because null file path should throw ArgumentException");
    }

    [Fact]
    public async Task PublishPluginAsync_WithEmptyFilePath_ThrowsArgumentException()
    {
        // Arrange
        var metadata = new PluginPublishMetadata
        {
            PluginName = "TestPlugin",
            Version = "1.0.0",
            Description = "A test plugin",
            Author = "TestAuthor"
        };

        // Act
        var act = () => _registry.PublishPluginAsync("   ", metadata);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "because whitespace file path should throw ArgumentException");
    }

    [Fact]
    public async Task PublishPluginAsync_WithNullMetadata_ThrowsArgumentNullException()
    {
        // Arrange
        var filePath = "/tmp/test-plugin.dll";

        // Act
        var act = () => _registry.PublishPluginAsync(filePath, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>(
            "because null metadata should throw ArgumentNullException");
    }

    [Fact]
    public async Task PublishPluginAsync_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var filePath = "/tmp/nonexistent.dll";
        var metadata = new PluginPublishMetadata
        {
            PluginName = "TestPlugin",
            Version = "1.0.0",
            Description = "A test plugin",
            Author = "TestAuthor"
        };

        // Act
        var result = await _registry.PublishPluginAsync(filePath, metadata);

        // Assert
        result.Should().BeFalse();
        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Plugin file not found")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()!), Times.Once);
    }

    [Fact]
    public async Task PublishPluginAsync_WithUploadFailure_ReturnsFalse()
    {
        // Arrange
        var filePath = "/tmp/test-plugin.dll";
        var metadata = new PluginPublishMetadata
        {
            PluginName = "TestPlugin",
            Version = "1.0.0",
            Description = "A test plugin",
            Author = "TestAuthor"
        };

        _httpClientMock.Setup(x => x.UploadPluginAsync(filePath, It.IsAny<string>()))
            .ReturnsAsync(false);

        File.WriteAllText(filePath, "test content");

        try
        {
            // Act
            var result = await _registry.PublishPluginAsync(filePath, metadata);

            // Assert
            result.Should().BeFalse();
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void InvalidateCache_RemovesAllPluginCacheEntries()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var pluginInfo = new PluginInfo
        {
            Id = pluginId,
            Name = "TestPlugin",
            Version = "1.0.0",
            DownloadUrl = "https://example.com/test.zip"
        };
        var versions = new List<PluginVersionInfo>
        {
            new() { Version = "1.0.0", PublishedAtUtc = DateTime.UtcNow, DownloadUrl = "https://example.com/v1.zip" }
        };

        _cache.Set($"registry_plugin_{pluginId}", pluginInfo, new MemoryCacheEntryOptions());
        _cache.Set($"registry_versions_{pluginId}", versions, new MemoryCacheEntryOptions());

        // Verify cache entries exist
        _cache.TryGetValue($"registry_plugin_{pluginId}", out _).Should().BeTrue();
        _cache.TryGetValue($"registry_versions_{pluginId}", out _).Should().BeTrue();

        // Act
        _registry.InvalidateCache(pluginId);

        // Assert
        _cache.TryGetValue($"registry_plugin_{pluginId}", out _).Should().BeFalse();
        _cache.TryGetValue($"registry_versions_{pluginId}", out _).Should().BeFalse();
    }

    [Fact]
    public void PluginVersionInfo_Properties_WorkCorrectly()
    {
        // Arrange
        var publishedAt = DateTime.UtcNow;
        var versionInfo = new PluginVersionInfo
        {
            Version = "2.0.0",
            PublishedAtUtc = publishedAt,
            DownloadUrl = "https://example.com/v2.zip",
            IsStable = true,
            IsPrerelease = false,
            ReleaseNotes = "Major improvements"
        };

        // Assert
        versionInfo.Version.Should().Be("2.0.0");
        versionInfo.PublishedAtUtc.Should().Be(publishedAt);
        versionInfo.DownloadUrl.Should().Be("https://example.com/v2.zip");
        versionInfo.IsStable.Should().BeTrue();
        versionInfo.IsPrerelease.Should().BeFalse();
        versionInfo.ReleaseNotes.Should().Be("Major improvements");
    }

    [Fact]
    public void PluginPublishMetadata_Properties_WorkCorrectly()
    {
        // Arrange
        var metadata = new PluginPublishMetadata
        {
            PluginName = "TestPlugin",
            Version = "1.0.0",
            Description = "A test plugin",
            Author = "TestAuthor",
            Company = "TestCompany",
            Tags = new List<string> { "test", "plugin" },
            LicenseType = "MIT"
        };

        // Assert
        metadata.PluginName.Should().Be("TestPlugin");
        metadata.Version.Should().Be("1.0.0");
        metadata.Description.Should().Be("A test plugin");
        metadata.Author.Should().Be("TestAuthor");
        metadata.Company.Should().Be("TestCompany");
        metadata.Tags.Should().BeEquivalentTo(new List<string> { "test", "plugin" });
        metadata.LicenseType.Should().Be("MIT");
    }
}