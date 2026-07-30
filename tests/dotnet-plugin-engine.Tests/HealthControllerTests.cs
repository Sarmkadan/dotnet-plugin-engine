namespace dotnet_plugin_engine.Tests;

using Microsoft.AspNetCore.Mvc;
using Moq;
using PluginEngine.Services.Abstractions;
using PluginEngine.Events;
using Xunit;

public class HealthControllerTests
{
    [Fact]
    public async Task GetHealthAsync_ReturnsOkResult()
    {
        // Arrange
        var pluginManagerServiceMock = new Mock<IPluginManagerService>();
        var hotReloadServiceMock = new Mock<IHotReloadService>();
        var eventPublisherMock = new Mock<IPluginEventPublisher>();

        var controller = new HealthController(pluginManagerServiceMock.Object, hotReloadServiceMock.Object, eventPublisherMock.Object);

        // Act
        var result = await controller.GetHealthAsync();

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetReadyAsync_ReturnsOkResult_WhenInitializedAndLoadedPluginsExist()
    {
        // Arrange
        var pluginManagerServiceMock = new Mock<IPluginManagerService>();
        var hotReloadServiceMock = new Mock<IHotReloadService>();
        var eventPublisherMock = new Mock<IPluginEventPublisher>();

        pluginManagerServiceMock.Setup(ps => ps.GetStatusAsync()).ReturnsAsync(new PluginStatus { IsInitialized = true, LoadedPlugins = 1 });

        var controller = new HealthController(pluginManagerServiceMock.Object, hotReloadServiceMock.Object, eventPublisherMock.Object);

        // Act
        var result = await controller.GetReadyAsync();

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetReadyAsync_ReturnsStatusCode503_WhenNotInitializedOrNoLoadedPluginsExist()
    {
        // Arrange
        var pluginManagerServiceMock = new Mock<IPluginManagerService>();
        var hotReloadServiceMock = new Mock<IHotReloadService>();
        var eventPublisherMock = new Mock<IPluginEventPublisher>();

        pluginManagerServiceMock.Setup(ps => ps.GetStatusAsync()).ReturnsAsync(new PluginStatus { IsInitialized = false, LoadedPlugins = 0 });

        var controller = new HealthController(pluginManagerServiceMock.Object, hotReloadServiceMock.Object, eventPublisherMock.Object);

        // Act
        var result = await controller.GetReadyAsync();

        // Assert
        Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(503, ((StatusCodeResult)result).StatusCode);
    }

    [Fact]
    public async Task GetReportAsync_ReturnsOkResult()
    {
        // Arrange
        var pluginManagerServiceMock = new Mock<IPluginManagerService>();
        var hotReloadServiceMock = new Mock<IHotReloadService>();
        var eventPublisherMock = new Mock<IPluginEventPublisher>();

        pluginManagerServiceMock.Setup(ps => ps.GetStatusAsync()).ReturnsAsync(new PluginStatus { IsInitialized = true, LoadedPlugins = 1 });
        hotReloadServiceMock.Setup(hrs => hrs.GetStatisticsAsync()).ReturnsAsync(new HotReloadStatistics { TotalReloads = 1, SuccessfulReloads = 1, FailedReloads = 0 });
        eventPublisherMock.Setup(ep => ep.GetStatistics()).Returns(new EventStatistics { EventsPublished = 1, RegisteredSubscribers = 1, MonitoredEventTypes = 1, Timestamp = DateTime.Now });

        var controller = new HealthController(pluginManagerServiceMock.Object, hotReloadServiceMock.Object, eventPublisherMock.Object);

        // Act
        var result = await controller.GetReportAsync();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
