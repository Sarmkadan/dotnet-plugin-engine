using Xunit;
using System.Threading.Tasks;
using PluginEngine.Data.Repositories;
using PluginEngine.Data.Models;
using System.Collections.Generic;
using Moq;

namespace dotnet_plugin_engine.Tests
{
    public class PluginRepositoryExtensionsTests
    {
        [Fact]
        public async Task TestGetByNameAsync_ValidInput_ReturnsPlugin()
        {
            // Arrange
            var mockPluginRepository = new Mock<PluginRepository>();
            mockPluginRepository.Setup(repo => repo.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(new Plugin());
            var pluginRepositoryExtensions = new PluginRepositoryExtensions(mockPluginRepository.Object);

            // Act
            var result = await pluginRepositoryExtensions.GetByNameAsync("valid-plugin-name");

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task TestGetByNameAsync_InvalidInput_ThrowsException()
        {
            // Arrange
            var mockPluginRepository = new Mock<PluginRepository>>();
            mockPluginRepository.Setup(repo => repo.GetByNameAsync(It.IsAny<string>())).Throws(new Exception());
            var pluginRepositoryExtensions = new PluginRepositoryExtensions(mockPluginRepository.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>>(async () => await pluginRepositoryExtensions.GetByNameAsync(null));
        }

        [Fact]
        public async Task TestGetByStatusAsync_ValidInput_ReturnsPlugins()
        {
            // Arrange
            var mockPluginRepository = new Mock<PluginRepository>();
            mockPluginRepository.Setup(repo => repo.GetByStatusAsync(It.IsAny<PluginStatus>())).ReturnsAsync(new List<Plugin>> { new Plugin() });
            var pluginRepositoryExtensions = new PluginRepositoryExtensions(mockPluginRepository.Object);

            // Act
            var result = await pluginRepositoryExtensions.GetByStatusAsync(PluginStatus.Enabled);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task TestGetByStatusAsync_InvalidInput_ThrowsException()
        {
            // Arrange
            var mockPluginRepository = new Mock<PluginRepository>>();
            mockPluginRepository.Setup(repo => repo.GetByStatusAsync(It.IsAny<PluginStatus>())).Throws(new Exception());
            var pluginRepositoryExtensions = new PluginRepositoryExtensions(mockPluginRepository.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>>(async () => await pluginRepositoryExtensions.GetByStatusAsync((PluginStatus)(-1));
        }

        [Fact]
        public async Task TestGetByStatusesAsync_ValidInput_ReturnsPlugins()
        {
            // Arrange
            var mockPluginRepository = new Mock<PluginRepository>>();
            mockPluginRepository.Setup(repo => repo.GetByStatusesAsync(It.IsAny<IEnumerable<PluginStatus>>())).ReturnsAsync(new List<Plugin>> { new Plugin() });
            var pluginRepositoryExtensions = new PluginRepositoryExtensions(mockPluginRepository.Object);

            // Act
            var result = await pluginRepositoryExtensions.GetByStatusesAsync(new List<PluginStatus>> { PluginStatus.Enabled });

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task TestGetByStatusesAsync_InvalidInput_ThrowsException()
        {
            // Arrange
            var mockPluginRepository = new Mock<PluginRepository>>();
            mockPluginRepository.Setup(repo => repo.GetByStatusesAsync(It.IsAny<IEnumerable<PluginStatus>>())).Throws(new Exception());
            var pluginRepositoryExtensions = new PluginRepositoryExtensions(mockPluginRepository.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>>(async () => await pluginRepositoryExtensions.GetByStatusesAsync(null);
        }

        [Fact]
        public async Task TestGetDependentPluginsAsync_ValidInput_ReturnsPlugins()
        {
            // Arrange
            var mockPluginRepository = new Mock<PluginRepository>>();
            mockPluginRepository.Setup(repo => repo.GetDependentPluginsAsync(It.IsAny<Plugin>>())).ReturnsAsync(new List<Plugin>> { new Plugin() });
            var pluginRepositoryExtensions = new PluginRepositoryExtensions(mockPluginRepository.Object);

            // Act
            var result = await pluginRepositoryExtensions.GetDependentPluginsAsync(new Plugin());

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task TestGetDependentPluginsAsync_InvalidInput_ThrowsException()
        {
            // Arrange
            var mockPluginRepository = new Mock<PluginRepository>>();
            mockPluginRepository.Setup(repo => repo.GetDependentPluginsAsync(It.IsAny<Plugin>>())).Throws(new Exception());
            var pluginRepositoryExtensions = new PluginRepositoryExtensions(mockPluginRepository.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>>(async () => await pluginRepositoryExtensions.GetDependentPluginsAsync(null);
        }
    }
}