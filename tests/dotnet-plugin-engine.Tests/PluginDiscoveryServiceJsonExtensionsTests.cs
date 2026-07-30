using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using PluginEngine.Utils.Helpers;
using Newtonsoft.Json;

namespace PluginDiscoveryServiceJsonExtensionsTests
{
    public class PluginDiscoveryServiceJsonExtensionsTests
    {
        [Fact]
        public void ToJson_Happy_PATH()
        {
            // Arrange
            var service = new PluginDiscoveryServiceJsonExtensions();
            // Act
            var json = service.ToJson(true);
            // Assert
            Assert.NotEmpty(json);
        }

        [Fact]
        public void FromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{}";
            // Act
            var service = PluginDiscoveryServiceJsonExtensions.FromJson(json);
            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void TryFromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{}";
            PluginDiscoveryService? service;
            // Act
            var result = PluginDiscoveryServiceJsonExtensions.TryFromJson(json, out service);
            // Assert
            Assert.True(result);
            Assert.NotNull(service);
        }
    }
}