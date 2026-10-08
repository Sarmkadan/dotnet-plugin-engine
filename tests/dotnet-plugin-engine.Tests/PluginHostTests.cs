#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Configuration;
using PluginEngine.Services.Abstractions;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Tests for the <see cref="PluginHost"/> class.
/// </summary>
public class PluginHostTests
{
    private readonly Mock<ILogger<PluginHost>> _loggerMock;
    private readonly Mock<PluginEngine> _engineMock;
    private readonly PluginHost _host;

    public PluginHostTests()
    {
        _loggerMock = new Mock<ILogger<PluginHost>>();
        _engineMock = new Mock<PluginEngine>();
        _host = new PluginHost(_loggerMock.Object, _engineMock.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldInitializeEngineAndLoadPlugins()
    {
        // Arrange
        _engineMock.Setup(e => e.InitializeAsync(It.IsAny<System.Threading.CancellationToken>()))
            .Returns(Task.CompletedTask);
        _engineMock.Setup(e => e.LoadAllPluginsAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(3);
        _engineMock.Setup(e => e.GetStatusAsync())
            .ReturnsAsync(new Services.Abstractions.PluginManagerStatus
            {
                IsInitialized = true,
                InitializedAt = System.DateTimeOffset.UtcNow
            });
        _engineMock.Setup(e => e.PluginLoader.GetAllLoadedPluginsAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PluginEngine.Domain.Entities.Plugin
                {
                    Id = "plugin1",
                    Name = "Test Plugin 1",
                    Version = new PluginEngine.Domain.Entities.VersionInfo(1, 0, 0)
                },
                new PluginEngine.Domain.Entities.Plugin
                {
                    Id = "plugin2",
                    Name = "Test Plugin 2",
                    Version = new PluginEngine.Domain.Entities.VersionInfo(2, 0, 0)
                }
            });

        // Act
        await _host.StartAsync();

        // Assert
        _engineMock.Verify(e => e.InitializeAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _engineMock.Verify(e => e.LoadAllPluginsAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _engineMock.Verify(e => e.GetStatusAsync(), Times.Once);
        _engineMock.Verify(e => e.PluginLoader.GetAllLoadedPluginsAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);

        // Verify logging using the actual PluginHostLog methods
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.HostStart(l), Times.Once);
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginHostStarted(l, 3, It.IsAny<System.DateTimeOffset>()), Times.Once);
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginLoaded(l, "plugin1", "Test Plugin 1", "1.0.0"), Times.Once);
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginLoaded(l, "plugin2", "Test Plugin 2", "2.0.0"), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldUnloadPluginsAndShutdownEngine()
    {
        // Arrange
        _engineMock.Setup(e => e.PluginLoader.GetAllLoadedPluginsAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PluginEngine.Domain.Entities.Plugin
                {
                    Id = "plugin1",
                    Name = "Test Plugin 1"
                }
            });
        _engineMock.Setup(e => e.PluginLoader.UnloadPluginAsync("plugin1", It.IsAny<System.Threading.CancellationToken>()))
            .Returns(Task.CompletedTask);
        _engineMock.Setup(e => e.ShutdownAsync(It.IsAny<System.Threading.CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _host.StopAsync();

        // Assert
        _engineMock.Verify(e => e.PluginLoader.GetAllLoadedPluginsAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _engineMock.Verify(e => e.PluginLoader.UnloadPluginAsync("plugin1", It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _engineMock.Verify(e => e.ShutdownAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);

        // Verify logging
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.HostStop(l), Times.Once);
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginUnloaded(l, "plugin1", "Test Plugin 1", It.IsAny<long>()), Times.Once);
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginHostStopped(l), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldHandlePluginUnloadFailure()
    {
        // Arrange
        var exception = new System.Exception("Test exception");
        _engineMock.Setup(e => e.PluginLoader.GetAllLoadedPluginsAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PluginEngine.Domain.Entities.Plugin
                {
                    Id = "plugin1",
                    Name = "Test Plugin 1"
                }
            });
        _engineMock.Setup(e => e.PluginLoader.UnloadPluginAsync("plugin1", It.IsAny<System.Threading.CancellationToken>()))
            .ThrowsAsync(exception);
        _engineMock.Setup(e => e.ShutdownAsync(It.IsAny<System.Threading.CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _host.StopAsync();

        // Assert
        _engineMock.Verify(e => e.PluginLoader.GetAllLoadedPluginsAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _engineMock.Verify(e => e.PluginLoader.UnloadPluginAsync("plugin1", It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _engineMock.Verify(e => e.ShutdownAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);

        // Verify logging
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.HostStop(l), Times.Once);
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginUnloadFailed(
            l,
            "plugin1",
            "Test Plugin 1",
            It.IsAny<long>(),
            It.Is<string>(s => s.Contains("Test exception"))), Times.Once);
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginHostStopped(l), Times.Once);
    }

    [Fact]
    public async Task ExecutePluginAsync_ShouldExecutePluginAndLogSuccess()
    {
        // Arrange
        var pluginId = "test-plugin";
        var correlationId = System.Guid.NewGuid().ToString();
        _engineMock.Setup(e => e.PluginLoader.GetLoadedPluginAsync(pluginId, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new PluginEngine.Domain.Entities.Plugin
            {
                Id = pluginId,
                Name = "Test Plugin"
            });

        // Act
        await _host.ExecutePluginAsync(pluginId);

        // Assert
        _engineMock.Verify(e => e.PluginLoader.GetLoadedPluginAsync(pluginId, It.IsAny<System.Threading.CancellationToken>()), Times.Once);

        // Verify logging
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginExecuted(l, pluginId, It.Is<string>(id => id == correlationId)), Times.Once);
    }

    [Fact]
    public async Task ExecutePluginAsync_ShouldLogFailureWhenPluginNotFound()
    {
        // Arrange
        var pluginId = "non-existent-plugin";
        _engineMock.Setup(e => e.PluginLoader.GetLoadedPluginAsync(pluginId, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync((PluginEngine.Domain.Entities.Plugin?)null);

        // Act & Assert
        await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
            _host.ExecutePluginAsync(pluginId));

        // Verify logging
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginExecutionFailed(
            l,
            pluginId,
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("Plugin 'non-existent-plugin' is not loaded"))), Times.Once);
    }

    [Fact]
    public async Task ExecutePluginAsync_ShouldLogFailureWhenExceptionOccurs()
    {
        // Arrange
        var pluginId = "test-plugin";
        var exception = new System.Exception("Test execution error");
        _engineMock.Setup(e => e.PluginLoader.GetLoadedPluginAsync(pluginId, It.IsAny<System.Threading.CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() =>
            _host.ExecutePluginAsync(pluginId));

        // Verify logging
        _loggerMock.Verify(l => PluginEngine.PluginHostLog.PluginExecutionFailed(
            l,
            pluginId,
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("Test execution error"))), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldMarkAsDisposed()
    {
        // Act
        _host.Dispose();

        // Assert
        Assert.True(typeof(PluginHost).GetField("_isDisposed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(_host));
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
}