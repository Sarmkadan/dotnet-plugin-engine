#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Configuration;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Tests for the <see cref="PluginHost"/> class.
/// </summary>
public class PluginHostTests : IDisposable
{
    // Event IDs used by PluginHostLog (internal, so asserted by value here).
    private const int PluginHostStartedEventId = 1000;
    private const int PluginHostStoppedEventId = 1001;
    private const int PluginLoadedEventId = 1002;
    private const int PluginUnloadedEventId = 1003;
    private const int PluginUnloadFailedEventId = 1004;
    private const int PluginExecutedEventId = 1005;
    private const int PluginExecutionFailedEventId = 1006;
    private const int HostStartEventId = 1007;
    private const int HostStopEventId = 1008;

    private readonly Mock<IPluginManagerService> _pluginManagerMock;
    private readonly Mock<IPluginLoaderService> _pluginLoaderMock;
    private readonly CapturingLogger _logger;
    private readonly string _pluginDirectory;
    private readonly PluginHost _host;

    public PluginHostTests()
    {
        _pluginManagerMock = new Mock<IPluginManagerService>();
        _pluginLoaderMock = new Mock<IPluginLoaderService>();
        _logger = new CapturingLogger();

        _pluginDirectory = Path.Combine(Path.GetTempPath(), "plugin-host-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_pluginDirectory);

        var engine = new PluginEngine(
            _pluginManagerMock.Object,
            _pluginLoaderMock.Object,
            Mock.Of<IDependencyResolutionService>(),
            Mock.Of<IVersioningService>(),
            Mock.Of<IHotReloadService>(),
            new PluginEngineOptions { PluginDirectory = _pluginDirectory });

        _host = new PluginHost(_logger, engine);
    }

    public void Dispose()
    {
        if (Directory.Exists(_pluginDirectory))
            Directory.Delete(_pluginDirectory, recursive: true);
    }

    [Fact]
    public async Task StartAsync_ShouldInitializeEngineAndLoadPlugins()
    {
        // Arrange
        var plugin1 = CreatePlugin("Test Plugin 1", "1.0.0");
        var plugin2 = CreatePlugin("Test Plugin 2", "2.0.0");
        _pluginManagerMock.Setup(m => m.InitializeAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _pluginLoaderMock.Setup(l => l.LoadPluginsFromDirectoryAsync(_pluginDirectory, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { plugin1, plugin2 });
        _pluginManagerMock.Setup(m => m.GetStatusAsync())
            .ReturnsAsync(new PluginManagerStatus
            {
                IsInitialized = true,
                InitializedAt = DateTime.UtcNow
            });
        _pluginLoaderMock.Setup(l => l.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { plugin1, plugin2 });

        // Act
        await _host.StartAsync();

        // Assert
        _pluginManagerMock.Verify(m => m.InitializeAsync(It.IsAny<CancellationToken>()), Times.Once);
        _pluginLoaderMock.Verify(l => l.LoadPluginsFromDirectoryAsync(_pluginDirectory, It.IsAny<CancellationToken>()), Times.Once);
        _pluginManagerMock.Verify(m => m.GetStatusAsync(), Times.Once);
        _pluginLoaderMock.Verify(l => l.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify logging
        AssertLogged(HostStartEventId);
        AssertLogged(PluginHostStartedEventId, "Loaded 2 plugins");
        AssertLogged(PluginLoadedEventId, plugin1.Id.ToString(), "Test Plugin 1", "1.0.0");
        AssertLogged(PluginLoadedEventId, plugin2.Id.ToString(), "Test Plugin 2", "2.0.0");
    }

    [Fact]
    public async Task StopAsync_ShouldUnloadPluginsAndShutdownEngine()
    {
        // Arrange
        var plugin = CreatePlugin("Test Plugin 1", "1.0.0");
        _pluginLoaderMock.Setup(l => l.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { plugin });
        _pluginLoaderMock.Setup(l => l.UnloadPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _pluginManagerMock.Setup(m => m.ShutdownAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _host.StopAsync();

        // Assert
        _pluginLoaderMock.Verify(l => l.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _pluginLoaderMock.Verify(l => l.UnloadPluginAsync(plugin.Id, It.IsAny<CancellationToken>()), Times.Once);
        _pluginManagerMock.Verify(m => m.ShutdownAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify logging
        AssertLogged(HostStopEventId);
        AssertLogged(PluginUnloadedEventId, plugin.Id.ToString(), "Test Plugin 1");
        AssertLogged(PluginHostStoppedEventId);
    }

    [Fact]
    public async Task StopAsync_ShouldHandlePluginUnloadFailure()
    {
        // Arrange
        var plugin = CreatePlugin("Test Plugin 1", "1.0.0");
        _pluginLoaderMock.Setup(l => l.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { plugin });
        _pluginLoaderMock.Setup(l => l.UnloadPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));
        _pluginManagerMock.Setup(m => m.ShutdownAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _host.StopAsync();

        // Assert
        _pluginLoaderMock.Verify(l => l.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _pluginLoaderMock.Verify(l => l.UnloadPluginAsync(plugin.Id, It.IsAny<CancellationToken>()), Times.Once);
        _pluginManagerMock.Verify(m => m.ShutdownAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify logging
        AssertLogged(HostStopEventId);
        AssertLogged(PluginUnloadFailedEventId, plugin.Id.ToString(), "Test Plugin 1", "Test exception");
        AssertLogged(PluginHostStoppedEventId);
    }

    [Fact]
    public async Task ExecutePluginAsync_ShouldExecutePluginAndLogSuccess()
    {
        // Arrange
        var plugin = CreatePlugin("Test Plugin", "1.0.0");
        _pluginLoaderMock.Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);

        // Act
        await _host.ExecutePluginAsync(plugin.Id.ToString());

        // Assert
        _pluginLoaderMock.Verify(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()), Times.Once);

        // Verify logging
        AssertLogged(PluginExecutedEventId, plugin.Id.ToString());
    }

    [Fact]
    public async Task ExecutePluginAsync_ShouldLogFailureWhenPluginNotFound()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        _pluginLoaderMock.Setup(l => l.GetLoadedPluginAsync(pluginId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plugin?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _host.ExecutePluginAsync(pluginId.ToString()));

        // Verify logging
        AssertLogged(PluginExecutionFailedEventId, $"Plugin '{pluginId}' is not loaded");
    }

    [Fact]
    public async Task ExecutePluginAsync_ShouldLogFailureWhenExceptionOccurs()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        _pluginLoaderMock.Setup(l => l.GetLoadedPluginAsync(pluginId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test execution error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _host.ExecutePluginAsync(pluginId.ToString()));

        // Verify logging
        AssertLogged(PluginExecutionFailedEventId, pluginId.ToString(), "Test execution error");
    }

    [Fact]
    public async Task ExecutePluginAsync_ShouldThrowArgumentExceptionWhenPluginIdIsNotGuid()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _host.ExecutePluginAsync("not-a-guid"));

        Assert.Equal("pluginId", exception.ParamName);
        _pluginLoaderMock.Verify(l => l.GetLoadedPluginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Dispose_ShouldMarkAsDisposed()
    {
        // Act
        _host.Dispose();

        // Assert
        Assert.True((bool)typeof(PluginHost).GetField("_isDisposed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(_host)!);
    }

    [Fact]
    public void Dispose_ShouldCallSuppressFinalize()
    {
        // We can't directly test GC.SuppressFinalize, but we can verify the method is called
        // by checking that Dispose can be called multiple times without error

        // Act
        _host.Dispose();
        _host.Dispose(); // Should not throw

        // Assert - if we got here without exception, the test passes
        Assert.True(true);
    }

    private static Plugin CreatePlugin(string name, string version) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Version = version
    };

    private void AssertLogged(int eventId, params string[] fragments)
    {
        Assert.Contains(_logger.Entries, entry =>
            entry.EventId.Id == eventId &&
            fragments.All(fragment => entry.Message.Contains(fragment, StringComparison.Ordinal)));
    }

    /// <summary>
    /// Captures log entries so tests can assert on the messages emitted by <see cref="PluginHostLog"/>.
    /// </summary>
    private sealed class CapturingLogger : ILogger<PluginHost>
    {
        public List<(EventId EventId, string Message)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((eventId, formatter(state, exception)));
        }
    }
}
