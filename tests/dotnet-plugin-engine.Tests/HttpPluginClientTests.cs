using PluginEngine.Integration;
using Xunit;
using System.Threading;
using System.Collections.Generic;
using Moq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace PluginEngine.Tests.Integration
{
    public class HttpPluginClientTests
    {
        [Fact]
        public async Task TestGetAsync_HappyPath_CorrectResponse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var logger = new LoggerFactory().CreateLogger<HttpPluginClient>();
            var client = new HttpPluginClient(httpClient, logger);
            var url = "https://example.com";

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task TestIsAvailableAsync_HappyPath_ServiceAvailable()
        {
            // Arrange
            var httpClient = new HttpClient();
            var logger = new LoggerFactory().CreateLogger<HttpPluginClient>();
            var client = new HttpPluginClient(httpClient, logger);
            var url = "https://example.com";

            // Act
            var result = await client.IsAvailableAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task TestSendNotificationAsync_HappyPath_CorrectResponse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var logger = new LoggerFactory().CreateLogger<HttpPluginClient>();
            var client = new HttpPluginClient(httpClient, logger);
            var url = "https://example.com";
            var notification = new PluginNotification
            {
                PluginId = Guid.NewGuid(),
                PluginName = "TestPlugin",
                EventType = "TestEvent",
                OccurredAtUtc = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>()
            };

            // Act
            await client.SendNotificationAsync(notification.EventType, notification);

            // Assert
            // No assert as this is just a happy path test
        }

        [Fact]
        public async Task TestGetAsync_NullUrl_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var logger = new LoggerFactory().CreateLogger<HttpPluginClient>();
            var client = new HttpPluginClient(httpClient, logger);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetAsync(null);
            });
        }

        [Fact]
        public async Task TestIsAvailableAsync_NullRegistryBaseUrl_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var logger = new LoggerFactory().CreateLogger<HttpPluginClient>();
            var client = new HttpPluginClient(httpClient, logger);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await client.IsAvailableAsync();
            });
        }
    }
}