using System;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Utils.Helpers;
using Xunit;

namespace dotnet_plugin_engine.Tests
{
    public class FileSystemHelperJsonExtensionsTests
    {
        private readonly Mock<ILogger<FileSystemHelper>> _mockLogger;
        private readonly FileSystemHelper _helper;

        public FileSystemHelperJsonExtensionsTests()
        {
            _mockLogger = new Mock<ILogger<FileSystemHelper>>();
            _helper = new FileSystemHelper(_mockLogger.Object);
        }

        [Fact]
        public void ToJson_ValidHelper_ReturnsJsonString()
        {
            // Act
            var json = _helper.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.Contains("{", json);
            Assert.Contains("}", json);
        }

        [Fact]
        public void ToJson_Indented_ReturnsFormattedJson()
        {
            // Act
            var json = _helper.ToJson(indented: true);

            // Assert
            Assert.Contains(Environment.NewLine, json);
        }

        [Fact]
        public void ToJson_NullHelper_ThrowsArgumentNullException()
        {
            // Arrange
            FileSystemHelper helper = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => helper.ToJson());
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            // Act
            var result = FileSystemHelperJsonExtensions.FromJson("invalid-json");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_NullOrEmptyJson_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => FileSystemHelperJsonExtensions.FromJson(null!));
            Assert.Throws<ArgumentException>(() => FileSystemHelperJsonExtensions.FromJson(""));
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Act
            var success = FileSystemHelperJsonExtensions.TryFromJson("invalid-json", out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_NullOrEmptyJson_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => FileSystemHelperJsonExtensions.TryFromJson(null!, out _));
            Assert.Throws<ArgumentException>(() => FileSystemHelperJsonExtensions.TryFromJson("", out _));
        }
    }
}
