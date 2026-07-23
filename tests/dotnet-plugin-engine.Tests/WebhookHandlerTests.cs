using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PluginEngine.Integration;
using PluginEngine.Services.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using FluentAssertions;

namespace PluginEngine.Tests;

public class WebhookHandlerTests
{
    private readonly Mock<ILogger<WebhookHandler>> _loggerMock = new();
    private readonly Mock<IPluginManagerService> _pluginManagerMock = new();

    [Fact]
    public async Task ProcessWebhookAsync_ShouldReturnTrue_WhenEventIsRegistered()
    {
        // Arrange
        var handler = new WebhookHandler(_loggerMock.Object, _pluginManagerMock.Object);
        var payload = new WebhookPayload
        {
            PluginId = Guid.NewGuid(),
            EventType = "plugin.created",
            TimestampUtc = DateTime.UtcNow
        };

        // Act
        var result = await handler.ProcessWebhookAsync(payload);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_ShouldReturnFalse_WhenEventTypeIsUnknown()
    {
        // Arrange
        var handler = new WebhookHandler(_loggerMock.Object, _pluginManagerMock.Object);
        var payload = new WebhookPayload
        {
            PluginId = Guid.NewGuid(),
            EventType = "unknown.event",
            TimestampUtc = DateTime.UtcNow
        };

        // Act
        var result = await handler.ProcessWebhookAsync(payload);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterEventHandler_ShouldAllowProcessingCustomEvent()
    {
        // Arrange
        var handler = new WebhookHandler(_loggerMock.Object, _pluginManagerMock.Object);
        var handled = false;
        handler.RegisterEventHandler("custom.event", (payload) => {
            handled = true;
            return Task.CompletedTask;
        });

        var payload = new WebhookPayload
        {
            PluginId = Guid.NewGuid(),
            EventType = "custom.event",
            TimestampUtc = DateTime.UtcNow
        };

        // Act
        var result = await handler.ProcessWebhookAsync(payload);

        // Assert
        result.Should().BeTrue();
        handled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_ShouldReturnFalse_WhenSignatureIsInvalid()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Webhooks:Secret"]).Returns("mysecret");

        var handler = new WebhookHandler(_loggerMock.Object, _pluginManagerMock.Object, configMock.Object);
        var payload = new WebhookPayload
        {
            PluginId = Guid.NewGuid(),
            EventType = "plugin.created",
            TimestampUtc = DateTime.UtcNow
        };

        // Act
        var result = await handler.ProcessWebhookAsync(payload, "invalid-signature");

        // Assert
        result.Should().BeFalse();
    }
}
