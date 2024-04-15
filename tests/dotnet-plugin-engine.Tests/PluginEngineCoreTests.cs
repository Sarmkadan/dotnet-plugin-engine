#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for PluginEngine core functionality
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PluginEngine.Configuration;
using PluginEngine.Domain.Entities;
using PluginEngine.Exceptions;
using PluginEngine.Services.Abstractions;
using Xunit;

namespace PluginEngine.Tests;

public sealed class PluginEngineCoreTests : IDisposable
{
    private readonly Mock<IPluginManagerService> _pluginManagerMock = new();
    private readonly Mock<IPluginLoaderService> _pluginLoaderMock = new();
    private readonly Mock<IDependencyResolutionService> _dependencyResolverMock = new();
    private readonly Mock<IVersioningService> _versioningMock = new();
    private readonly Mock<IHotReloadService> _hotReloadMock = new();
    private readonly PluginEngineOptions _options;
    private readonly string _testDirectory;

    public PluginEngineCoreTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _options = new PluginEngineOptions { PluginDirectory = _testDirectory };
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    private PluginEngine CreateEngine()
    {
        return new PluginEngine(
            _pluginManagerMock.Object,
            _pluginLoaderMock.Object,
            _dependencyResolverMock.Object,
            _versioningMock.Object,
            _hotReloadMock.Object,
            _options
        );
    }

    // ── Constructor Tests ────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithNullPluginManager_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PluginEngine(
            null!,
            _pluginLoaderMock.Object,
            _dependencyResolverMock.Object,
            _versioningMock.Object,
            _hotReloadMock.Object,
            _options
        );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("pluginManagerService");
    }

    [Fact]
    public void Constructor_WithNullPluginLoader_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PluginEngine(
            _pluginManagerMock.Object,
            null!,
            _dependencyResolverMock.Object,
            _versioningMock.Object,
            _hotReloadMock.Object,
            _options
        );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("pluginLoaderService");
    }

    [Fact]
    public void Constructor_WithNullDependencyResolver_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PluginEngine(
            _pluginManagerMock.Object,
            _pluginLoaderMock.Object,
            null!,
            _versioningMock.Object,
            _hotReloadMock.Object,
            _options
        );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dependencyResolutionService");
    }

    [Fact]
    public void Constructor_WithNullVersioningService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PluginEngine(
            _pluginManagerMock.Object,
            _pluginLoaderMock.Object,
            _dependencyResolverMock.Object,
            null!,
            _hotReloadMock.Object,
            _options
        );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("versioningService");
    }

    [Fact]
    public void Constructor_WithNullHotReloadService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PluginEngine(
            _pluginManagerMock.Object,
            _pluginLoaderMock.Object,
            _dependencyResolverMock.Object,
            _versioningMock.Object,
            null!,
            _options
        );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("hotReloadService");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PluginEngine(
            _pluginManagerMock.Object,
            _pluginLoaderMock.Object,
            _dependencyResolverMock.Object,
            _versioningMock.Object,
            _hotReloadMock.Object,
            null!
        );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    // ── InitializeAsync Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_CallsPluginManagerInitialize()
    {
        // Arrange
        var engine = CreateEngine();
        _pluginManagerMock
            .Setup(x => x.InitializeAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await engine.InitializeAsync();

        // Assert
        _pluginManagerMock.Verify();
    }

    [Fact]
    public async Task InitializeAsync_WithCancellationToken_PassesTokenToManager()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var engine = CreateEngine();
        _pluginManagerMock
            .Setup(x => x.InitializeAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await engine.InitializeAsync(cts.Token);

        // Assert
        _pluginManagerMock.Verify(x => x.InitializeAsync(cts.Token), Times.Once);
    }

    // ── ShutdownAsync Tests ────────────────────────────────────────────────────

    [Fact]
    public async Task ShutdownAsync_CallsPluginManagerShutdown()
    {
        // Arrange
        var engine = CreateEngine();
        _pluginManagerMock
            .Setup(x => x.ShutdownAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await engine.ShutdownAsync();

        // Assert
        _pluginManagerMock.Verify();
    }

    [Fact]
    public async Task ShutdownAsync_WithCancellationToken_PassesTokenToManager()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var engine = CreateEngine();
        _pluginManagerMock
            .Setup(x => x.ShutdownAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await engine.ShutdownAsync(cts.Token);

        // Assert
        _pluginManagerMock.Verify(x => x.ShutdownAsync(cts.Token), Times.Once);
    }

    // ── GetStatusAsync Tests ───────────────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_ReturnsStatusFromPluginManager()
    {
        // Arrange
        var expectedStatus = new PluginManagerStatus
        {
            IsInitialized = true,
            InitializedAt = DateTime.UtcNow,
            TotalPlugins = 5,
            LoadedPlugins = 3,
            ActivePlugins = 2,
            FailedPlugins = 1,
            LastError = null
        };

        var engine = CreateEngine();
        _pluginManagerMock
            .Setup(x => x.GetStatusAsync())
            .ReturnsAsync(expectedStatus)
            .Verifiable();

        // Act
        var result = await engine.GetStatusAsync();

        // Assert
        result.Should().BeSameAs(expectedStatus);
        _pluginManagerMock.Verify();
    }

    // ── LoadAllPluginsAsync Tests ──────────────────────────────────────────────

    [Fact]
    public async Task LoadAllPluginsAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var engine = CreateEngine();
        _options.PluginDirectory = "/nonexistent/directory/path";

        // Act
        var act = () => engine.LoadAllPluginsAsync();

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public async Task LoadAllPluginsAsync_CallsPluginLoaderWithCorrectDirectory()
    {
        // Arrange
        var engine = CreateEngine();
        var plugins = new List<Plugin> { new Plugin { Id = Guid.NewGuid(), Name = "TestPlugin" } };

        _pluginLoaderMock
            .Setup(x => x.LoadPluginsFromDirectoryAsync(_options.PluginDirectory, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugins)
            .Verifiable();

        // Act
        var result = await engine.LoadAllPluginsAsync();

        // Assert
        result.Should().Be(1);
        _pluginLoaderMock.Verify();
    }

    [Fact]
    public async Task LoadAllPluginsAsync_WithCancellationToken_PassesTokenToLoader()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var engine = CreateEngine();
        var plugins = new List<Plugin>();

        _pluginLoaderMock
            .Setup(x => x.LoadPluginsFromDirectoryAsync(_options.PluginDirectory, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugins);

        // Act
        await engine.LoadAllPluginsAsync(cts.Token);

        // Assert
        _pluginLoaderMock.Verify(x => x.LoadPluginsFromDirectoryAsync(_options.PluginDirectory, cts.Token), Times.Once);
    }

    // ── UnloadAllPluginsAsync Tests ────────────────────────────────────────────

    [Fact]
    public async Task UnloadAllPluginsAsync_CallsUnloadOnEachPlugin()
    {
        // Arrange
        var engine = CreateEngine();
        var plugins = new List<Plugin>
        {
            new Plugin { Id = Guid.NewGuid(), Name = "Plugin1" },
            new Plugin { Id = Guid.NewGuid(), Name = "Plugin2" },
            new Plugin { Id = Guid.NewGuid(), Name = "Plugin3" }
        };

        _pluginLoaderMock
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugins)
            .Verifiable();

        _pluginLoaderMock
            .Setup(x => x.UnloadPluginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await engine.UnloadAllPluginsAsync();

        // Assert
        _pluginLoaderMock.Verify(x => x.UnloadPluginAsync(plugins[0].Id, It.IsAny<CancellationToken>()), Times.Once);
        _pluginLoaderMock.Verify(x => x.UnloadPluginAsync(plugins[1].Id, It.IsAny<CancellationToken>()), Times.Once);
        _pluginLoaderMock.Verify(x => x.UnloadPluginAsync(plugins[2].Id, It.IsAny<CancellationToken>()), Times.Once);
        _pluginLoaderMock.Verify();
    }

    [Fact]
    public async Task UnloadAllPluginsAsync_WithCancellationToken_PassesTokenToOperations()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var engine = CreateEngine();
        var plugins = new List<Plugin> { new Plugin { Id = Guid.NewGuid(), Name = "Plugin1" } };

        _pluginLoaderMock
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugins);

        _pluginLoaderMock
            .Setup(x => x.UnloadPluginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await engine.UnloadAllPluginsAsync(cts.Token);

        // Assert
        _pluginLoaderMock.Verify(x => x.GetAllLoadedPluginsAsync(cts.Token), Times.Once);
        _pluginLoaderMock.Verify(x => x.UnloadPluginAsync(plugins[0].Id, cts.Token), Times.Once);
    }

    // ── GetHealthInfoAsync Tests ───────────────────────────────────────────────

    [Fact]
    public async Task GetHealthInfoAsync_ReturnsFormattedHealthReport()
    {
        // Arrange
        var status = new PluginManagerStatus
        {
            IsInitialized = true,
            InitializedAt = new DateTime(2024, 1, 1, 12, 0, 0),
            TotalPlugins = 10,
            LoadedPlugins = 8,
            ActivePlugins = 5,
            FailedPlugins = 2,
            LastError = "Test error"
        };

        var stats = new PluginManagerStatistics
        {
            TotalPlugins = 10,
            LoadedPlugins = 8,
            ActivePlugins = 5,
            FailedPlugins = 2,
            TotalMemoryUsageBytes = 1024 * 1024,
            TotalLoadContexts = 3,
            LastOperationTime = new DateTime(2024, 1, 1, 12, 5, 0),
            AverageLoadTimeMs = 125.5
        };

        var engine = CreateEngine();
        _pluginManagerMock
            .Setup(x => x.GetStatusAsync())
            .ReturnsAsync(status);

        _pluginManagerMock
            .Setup(x => x.GetStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await engine.GetHealthInfoAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Plugin Engine Health Report");
        result.Should().Contain("Initialized: True");
        result.Should().Contain("Total Plugins: 10");
        result.Should().Contain("Loaded Plugins: 8");
        result.Should().Contain("Active Plugins: 5");
        result.Should().Contain("Failed Plugins: 2");
        result.Should().Contain("Memory Usage: 1048576 bytes");
    }

    // ── Property Access Tests ────────────────────────────────────────────────────

    [Fact]
    public void PluginManager_ReturnsConfiguredPluginManager()
    {
        // Arrange
        var engine = CreateEngine();

        // Act
        var result = engine.PluginManager;

        // Assert
        result.Should().BeSameAs(_pluginManagerMock.Object);
    }

    [Fact]
    public void PluginLoader_ReturnsConfiguredPluginLoader()
    {
        // Arrange
        var engine = CreateEngine();

        // Act
        var result = engine.PluginLoader;

        // Assert
        result.Should().BeSameAs(_pluginLoaderMock.Object);
    }

    [Fact]
    public void DependencyResolver_ReturnsConfiguredDependencyResolver()
    {
        // Arrange
        var engine = CreateEngine();

        // Act
        var result = engine.DependencyResolver;

        // Assert
        result.Should().BeSameAs(_dependencyResolverMock.Object);
    }

    [Fact]
    public void VersioningService_ReturnsConfiguredVersioningService()
    {
        // Arrange
        var engine = CreateEngine();

        // Act
        var result = engine.VersioningService;

        // Assert
        result.Should().BeSameAs(_versioningMock.Object);
    }

    [Fact]
    public void HotReloader_ReturnsConfiguredHotReloadService()
    {
        // Arrange
        var engine = CreateEngine();

        // Act
        var result = engine.HotReloader;

        // Assert
        result.Should().BeSameAs(_hotReloadMock.Object);
    }

    [Fact]
    public void Options_ReturnsConfiguredOptions()
    {
        // Arrange
        var engine = CreateEngine();

        // Act
        var result = engine.Options;

        // Assert
        result.Should().BeSameAs(_options);
    }
}
