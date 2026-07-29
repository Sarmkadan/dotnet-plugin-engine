using System;
using Xunit;
using PluginEngine.Domain.Entities;

namespace PluginEngine.Tests
{
    public class PluginJsonExtensionsTests
    {
        [Fact]
        public void ToJson_NullPlugin_ThrowsArgumentNullException()
        {
            // Arrange
            Plugin? plugin = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => plugin!.ToJson());
        }

        [Fact]
        public void ToJson_HappyPath_ReturnsNonEmptyJson()
        {
            // Arrange
            var plugin = new Plugin(); // assumes parameterless ctor exists
            // Optionally set a few properties if they exist; using reflection to avoid compile errors
            var type = typeof(Plugin);
            var idProp = type.GetProperty("Id");
            if (idProp != null && idProp.CanWrite)
            {
                idProp.SetValue(plugin, Guid.NewGuid());
            }

            // Act
            var json = plugin.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json.Trim());
            Assert.EndsWith("}", json.Trim());
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PluginJsonExtensions.FromJson(json!));
        }

        [Fact]
        public void FromJson_EmptyString_ReturnsNull()
        {
            // Arrange
            var json = string.Empty;

            // Act
            var result = PluginJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsPluginInstance()
        {
            // Arrange
            var original = new Plugin();
            var type = typeof(Plugin);
            var nameProp = type.GetProperty("Name");
            if (nameProp != null && nameProp.CanWrite)
            {
                nameProp.SetValue(original, "TestPlugin");
            }

            var json = original.ToJson();

            // Act
            var deserialized = PluginJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            // If Name property exists, verify it round‑tripped
            if (nameProp != null && nameProp.CanRead)
            {
                var originalName = (string?)nameProp.GetValue(original);
                var deserializedName = (string?)nameProp.GetValue(deserialized!);
                Assert.Equal(originalName, deserializedName);
            }
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndPlugin()
        {
            // Arrange
            var plugin = new Plugin();
            var json = plugin.ToJson();

            // Act
            var success = PluginJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "{ this is not valid json }";

            // Act
            var success = PluginJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void ToJson_WithIndentation_ProducesIndentedJson()
        {
            // Arrange
            var plugin = new Plugin();

            // Act
            var json = plugin.ToJson(indented: true);

            // Assert
            // Indented JSON should contain line breaks
            Assert.Contains(Environment.NewLine, json);
        }
    }
}
