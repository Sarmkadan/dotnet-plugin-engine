using System;
using System.Collections.Generic;
using Xunit;
using PluginEngine.Domain.Entities;

namespace PluginEngine.Tests
{
    public class PluginMetadataExtensionsTests
    {
        [Fact]
        public void HasCustomProperty_HappyPath()
        {
            // Arrange
            var metadata = new PluginMetadata();
            metadata.SetCustomProperty("key", "value");

            // Act
            var result = PluginMetadataExtensions.HasCustomProperty(metadata, "key");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasCustomProperty_NullMetadata()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PluginMetadataExtensions.HasCustomProperty(null, "key"));
        }

        [Fact]
        public void HasCustomProperty_EmptyKey()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => PluginMetadataExtensions.HasCustomProperty(new PluginMetadata(), string.Empty));
        }

        [Fact]
        public void HasCustomProperty_KeyNotFound()
        {
            // Arrange
            var metadata = new PluginMetadata();

            // Act
            var result = PluginMetadataExtensions.HasCustomProperty(metadata, "key");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetAllCustomProperties_HappyPath()
        {
            // Arrange
            var metadata = new PluginMetadata();
            metadata.SetCustomProperty("key1", "value1");
            metadata.SetCustomProperty("key2", "value2");

            // Act
            var result = PluginMetadataExtensions.GetAllCustomProperties(metadata);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
        }

        [Fact]
        public void GetAllCustomProperties_NullMetadata()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PluginMetadataExtensions.GetAllCustomProperties(null));
        }

        [Fact]
        public void ClearCustomProperties_HappyPath()
        {
            // Arrange
            var metadata = new PluginMetadata();
            metadata.SetCustomProperty("key1", "value1");
            metadata.SetCustomProperty("key2", "value2");

            // Act
            PluginMetadataExtensions.ClearCustomProperties(metadata);

            // Assert
            Assert.Empty(metadata.GetCustomProperties());
        }

        [Fact]
        public void ClearCustomProperties_NullMetadata()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PluginMetadataExtensions.ClearCustomProperties(null));
        }
    }
}
