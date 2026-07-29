using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using PluginEngine.Services.Implementations;
using Xunit;
using Moq;

namespace PluginEngine.Services.Implementations.Tests
{
    public class PluginManagerServiceJsonExtensionsTests
    {
        [Fact]
        public void ToJson_Happy_PATH()
        {
            // Arrange
            var pluginManagerService = new PluginManagerServiceJsonExtensions();
            var json = "{}";

            // Act
            var result = PluginManagerServiceJsonExtensions.ToJson(pluginManagerService);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void FromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{}";

            // Act
            var result = PluginManagerServiceJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{}";
            PluginManagerService? result = null;

            // Act
            var isSuccess = PluginManagerServiceJsonExtensions.TryFromJson(json, out result);

            // Assert
            Assert.True(isSuccess);
            Assert.NotNull(result);
        }
    }
}