using PluginEngine.Services.Implementations;
using PluginEngine.Domain.Entities;
using PluginEngine.Results;
using Xunit;

namespace PluginEngine.Tests;

public class HotSwapServiceExtensionsTests
{
    [Fact]
    public void CanSwap_ReturnsTrue_ForValidPlugin()
    {
        // Arrange
        var service = new HotSwapService();
        var plugin = new Plugin();

        // Act
        var result = service.CanSwap(plugin);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetLastSwapRecordAsync_ReturnsLastSwapRecord_ForValidPlugin()
    {
        // Arrange
        var service = new HotSwapService();
        var pluginId = Guid.NewGuid();

        // Act
        var result = await service.GetLastSwapRecordAsync(pluginId);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSwapHistoryAsync_ReturnsSwapHistory_ForValidPlugin()
    {
        // Arrange
        var service = new HotSwapService();
        var pluginId = Guid.NewGuid();

        // Act
        var result = await service.GetSwapHistoryAsync(pluginId);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void RegisterPostSwapCallback_DoesNotThrow_ForValidPluginAndCallback()
    {
        // Arrange
        var service = new HotSwapService();
        var pluginId = Guid.NewGuid();
        var callback = new Func<Plugin, Task>((plugin) => Task.CompletedTask);

        // Act and Assert
        service.RegisterPostSwapCallback(pluginId, callback);
    }

    [Fact]
    public async Task RollbackSwapAsync_DoesNotThrow_ForValidPlugin()
    {
        // Arrange
        var service = new HotSwapService();
        var pluginId = Guid.NewGuid();

        // Act and Assert
        await service.RollbackSwapAsync(pluginId);
    }

    [Fact]
    public async Task SwapPluginAsync_DoesNotThrow_ForValidPluginAndAssemblyPath()
    {
        // Arrange
        var service = new HotSwapService();
        var pluginId = Guid.NewGuid();
        var assemblyPath = "path/to/assembly";

        // Act and Assert
        await service.SwapPluginAsync(pluginId, assemblyPath);
    }

    [Fact]
    public async Task HasSwapHistoryAsync_ReturnsTrue_ForPluginWithSwapHistory()
    {
        // Arrange
        var service = new HotSwapService();
        var pluginId = Guid.NewGuid();

        // Act
        var result = await service.HasSwapHistoryAsync(pluginId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetSwapHistoryCountAsync_ReturnsSwapHistoryCount_ForValidPlugin()
    {
        // Arrange
        var service = new HotSwapService();
        var pluginId = Guid.NewGuid();

        // Act
        var result = await service.GetSwapHistoryCountAsync(pluginId);

        // Assert
        Assert.True(result >= 0);
    }
}
