using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using PluginEngine.Services.Implementations;

namespace dotnet_plugin_engine.Tests
{
    public class VersioningServiceTests
    {
        public VersioningServiceTests()
        {
        }
        [Fact]
        public void ValidateVersion_ValidVersion_ReturnsTrue()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version = new SemanticVersion("1.2.3");
            // Act
            var result = versioningService.ValidateVersion(version);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateVersion_InvalidVersion_ReturnsFalse()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version = new SemanticVersion("1.2");
            // Act
            var result = versioningService.ValidateVersion(version);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSatisfiedBy_ValidVersion_ReturnsTrue()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version = new SemanticVersion("1.2.3");
            // Act
            var result = versioningService.IsSatisfiedBy(version);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSatisfiedBy_InvalidVersion_ReturnsFalse()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version = new SemanticVersion("1.2");
            // Act
            var result = versioningService.IsSatisfiedBy(version);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CompareVersions_ValidVersions_ReturnsCorrectComparison()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version1 = new SemanticVersion("1.2.3");
            var version2 = new SemanticVersion("1.2.4");
            // Act
            var result = versioningService.CompareVersions(version1, version2);
            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void ParseVersion_ValidVersionString_ReturnsCorrectVersion()
        {
            // Arrange
            var versioningService = new VersioningService();
            var versionString = "1.2.3";
            // Act
            var result = versioningService.ParseVersion(versionString);
            // Assert
            Assert.Equal(new SemanticVersion("1.2.3"), result);
        }

        [Fact]
        public async Task<IEnumerable<Domain.Entities.VersionInfo>> GetVersionHistoryAsync_ValidVersion_ReturnsVersionHistory()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version = new SemanticVersion("1.2.3");
            // Act
            var result = await versioningService.GetVersionHistoryAsync(version);
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
        }

        [Fact]
        public void IncrementVersion_ValidVersion_ReturnsCorrectIncrement()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version = new SemanticVersion("1.2.3");
            // Act
            var result = versioningService.IncrementVersion(version);
            // Assert
            Assert.Equal("1.2.4", result);
        }

        [Fact]
        public void AreCompatible_ValidVersions_ReturnsTrue()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version1 = new SemanticVersion("1.2.3");
            var version2 = new SemanticVersion("1.2.3");
            // Act
            var result = versioningService.AreCompatible(version1, version2);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AreCompatible_IncompatibleVersions_ReturnsFalse()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version1 = new SemanticVersion("1.2.3");
            var version2 = new SemanticVersion("1.2.4");
            // Act
            var result = versioningService.AreCompatible(version1, version2);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task<Domain.Entities.VersionInfo?> GetLatestVersionAsync_ValidVersion_ReturnsLatestVersion()
        {
            // Arrange
            var versioningService = new VersioningService();
            var version = new SemanticVersion("1.2.3");
            // Act
            var result = await versioningService.GetLatestVersionAsync(version);
            // Assert
            Assert.NotNull(result);
        }
    }
}