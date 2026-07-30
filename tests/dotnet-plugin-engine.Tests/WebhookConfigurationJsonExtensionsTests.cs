using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using PluginEngine.Configuration;

namespace dotnet_plugin_engine.Tests
{
    public class WebhookConfigurationJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var webhookConfiguration = new WebhookConfiguration { Url = "https://example.com/webhook" };

            // Act
            var json = WebhookConfigurationJsonExtensions.ToJson(webhookConfiguration);

            // Assert
            Assert.NotNull(json);
            Assert.Equal("{\"Url\":\"https://example.com/webhook\"}", json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => WebhookConfigurationJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsWebhookConfiguration()
        {
            // Arrange
            var json = "{\"Url\":\"https://example.com/webhook\"}";

            // Act
            var webhookConfiguration = WebhookConfigurationJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(webhookConfiguration);
            Assert.Equal("https://example.com/webhook", webhookConfiguration.Url);
        }

        [Fact]
        public void FromJson_NullInput_ReturnsNull()
        {
            // Act
            var webhookConfiguration = WebhookConfigurationJsonExtensions.FromJson(null);

            // Assert
            Assert.Null(webhookConfiguration);
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var webhookConfiguration = WebhookConfigurationJsonExtensions.FromJson("");

            // Assert
            Assert.Null(webhookConfiguration);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"Url\":\"https://example.com/webhook\"}";

            // Act
            var result = WebhookConfigurationJsonExtensions.TryFromJson(json, out var webhookConfiguration);

            // Assert
            Assert.True(result);
            Assert.NotNull(webhookConfiguration);
            Assert.Equal("https://example.com/webhook", webhookConfiguration.Url);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var result = WebhookConfigurationJsonExtensions.TryFromJson(null, out var webhookConfiguration);

            // Assert
            Assert.False(result);
            Assert.Null(webhookConfiguration);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act
            var result = WebhookConfigurationJsonExtensions.TryFromJson("", out var webhookConfiguration);

            // Assert
            Assert.False(result);
            Assert.Null(webhookConfiguration);
        }
    }
}
