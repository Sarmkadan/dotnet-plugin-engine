using System;
using System.Text.Json;
using Xunit;

namespace PluginEngine.Tests
{
    public class PluginAssemblyJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath()
        {
            // Arrange
            var pluginAssembly = new PluginAssembly();
            // Act
            var json = PluginAssemblyJsonExtensions.ToJson(pluginAssembly);
            // Assert
            Assert.NotNull(json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PluginAssemblyJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_HappyPath()
        {
            // Arrange
            var json = "{\"name\":\"TestAssembly\",\"version\":\"1.0.0\"}";
            // Act
            var pluginAssembly = PluginAssemblyJsonExtensions.FromJson(json);
            // Assert
            Assert.NotNull(pluginAssembly);
        }

        [Fact]
        public void FromJson_NullInput_ReturnsNull()
        {
            // Act and Assert
            Assert.Null(PluginAssemblyJsonExtensions.FromJson(null));
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act and Assert
            Assert.Null(PluginAssemblyJsonExtensions.FromJson(""));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"name\":\"TestAssembly\",\"version\":\"1.0.0\"}";
            // Act
            var result = PluginAssemblyJsonExtensions.TryFromJson(json, out var pluginAssembly);
            // Assert
            Assert.True(result);
            Assert.NotNull(pluginAssembly);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act and Assert
            var result = PluginAssemblyJsonExtensions.TryFromJson(null, out var pluginAssembly);
            Assert.False(result);
            Assert.Null(pluginAssembly);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act and Assert
            var result = PluginAssemblyJsonExtensions.TryFromJson("", out var pluginAssembly);
            Assert.False(result);
            Assert.Null(pluginAssembly);
        }
    }
}
