namespace dotnet_plugin_engine.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Utils.Helpers;
using Xunit;

public class PluginDiscoveryServiceTests
{
    [Fact]
    public async Task DiscoverPluginsAsync_ReturnsEmptyList_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var fileSystemHelperMock = new Mock<IFileSystemHelper>();
        var versionHelperMock = new Mock<IVersionHelper>();
        var loggerMock = new Mock<ILogger<PluginDiscoveryService>>();

        var service = new PluginDiscoveryService(fileSystemHelperMock.Object, versionHelperMock.Object, loggerMock.Object);

        // Act
        var result = await service.DiscoverPluginsAsync("non-existent-directory");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task DiscoverPluginsAsync_ReturnsPlugins_WhenDirectoryExists()
    {
        // Arrange
        var fileSystemHelperMock = new Mock<IFileSystemHelper>();
        var versionHelperMock = new Mock<IVersionHelper>();
        var loggerMock = new Mock<ILogger<PluginDiscoveryService>>();

        fileSystemHelperMock.Setup(fsh => fsh.DiscoverPlugins("directory")).Returns(new[] { "plugin1.dll", "plugin2.dll" });

        var service = new PluginDiscoveryService(fileSystemHelperMock.Object, versionHelperMock.Object, loggerMock.Object);

        // Act
        var result = await service.DiscoverPluginsAsync("directory");

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task InspectPluginAsync_ReturnsNull_WhenFilePathIsInvalid()
    {
        // Arrange
        var fileSystemHelperMock = new Mock<IFileSystemHelper>();
        var versionHelperMock = new Mock<IVersionHelper>();
        var loggerMock = new Mock<ILogger<PluginDiscoveryService>>();

        var service = new PluginDiscoveryService(fileSystemHelperMock.Object, versionHelperMock.Object, loggerMock.Object);

        // Act
        var result = await service.InspectPluginAsync("invalid-file-path");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InspectPluginAsync_ReturnsPluginInfo_WhenFilePathIsValid()
    {
        // Arrange
        var fileSystemHelperMock = new Mock<IFileSystemHelper>();
        var versionHelperMock = new Mock<IVersionHelper>();
        var loggerMock = new Mock<ILogger<PluginDiscoveryService>>();

        fileSystemHelperMock.Setup(fsh => fsh.GetFileInfo("valid-file-path")).Returns(new FileInfo("valid-file-path"));

        var service = new PluginDiscoveryService(fileSystemHelperMock.Object, versionHelperMock.Object, loggerMock.Object);

        // Act
        var result = await service.InspectPluginAsync("valid-file-path");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FilterPlugins_ReturnsEmptyList_WhenCandidatesListIsEmpty()
    {
        // Arrange
        var service = new PluginDiscoveryService(new FileSystemHelper(), new VersionHelper(), new Logger<PluginDiscoveryService>());

        // Act
        var result = service.FilterPlugins(new List<PluginCandidateInfo>());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FilterPlugins_ReturnsFilteredPlugins_WhenCandidatesListIsNotEmpty()
    {
        // Arrange
        var service = new PluginDiscoveryService(new FileSystemHelper(), new VersionHelper(), new Logger<PluginDiscoveryService>());

        var candidates = new List<PluginCandidateInfo>
        {
            new PluginCandidateInfo { IsValid = true },
            new PluginCandidateInfo { IsValid = false },
            new PluginCandidateInfo { IsValid = true }
        };

        // Act
        var result = service.FilterPlugins(candidates);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetStatistics_ReturnsEmptyStatistics_WhenCandidatesListIsEmpty()
    {
        // Arrange
        var service = new PluginDiscoveryService(new FileSystemHelper(), new VersionHelper(), new Logger<PluginDiscoveryService>());

        // Act
        var result = service.GetStatistics(new List<PluginCandidateInfo>());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetStatistics_ReturnsStatistics_WhenCandidatesListIsNotEmpty()
    {
        // Arrange
        var service = new PluginDiscoveryService(new FileSystemHelper(), new VersionHelper(), new Logger<PluginDiscoveryService>());

        var candidates = new List<PluginCandidateInfo>
        {
            new PluginCandidateInfo { IsValid = true },
            new PluginCandidateInfo { IsValid = false },
            new PluginCandidateInfo { IsValid = true }
        };

        // Act
        var result = service.GetStatistics(candidates);

        // Assert
        Assert.NotEmpty(result);
    }
}
