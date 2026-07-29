using System;
using System.Text.Json;
using Xunit;
using PluginEngine.Services.Implementations;

namespace PluginEngine.Tests
{
    public class PluginDependencyResolverJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ValidObject_ReturnsJsonString()
        {
            // Arrange
            var resolver = new PluginDependencyResolver();

            // Act
            var json = resolver.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.NotEmpty(json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((PluginDependencyResolver)null!).ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsObject()
        {
            // Arrange
            var resolver = new PluginDependencyResolver();
            var json = resolver.ToJson();

            // Act
            var result = json.FromJson();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ((string)null!).FromJson());
        }

        [Fact]
        public void FromJson_EmptyInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => string.Empty.FromJson());
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Act & Assert
            Assert.Throws<JsonException>(() => "{ invalid }".FromJson());
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndObject()
        {
            // Arrange
            var resolver = new PluginDependencyResolver();
            var json = resolver.ToJson();

            // Act
            var success = json.TryFromJson(out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "{ not valid }";

            // Act
            var success = invalidJson.TryFromJson(out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_NullInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ((string)null!).TryFromJson(out _));
        }
    }
}
