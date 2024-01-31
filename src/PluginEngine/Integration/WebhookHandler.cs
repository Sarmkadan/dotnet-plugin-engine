#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// Handles incoming webhooks from external systems about plugin updates and events.
/// Provides secure webhook processing with signature verification.
/// </summary>
public sealed class WebhookHandler
{
    private readonly ILogger<WebhookHandler> _logger;
    private readonly IPluginManagerService _pluginManager;
    private readonly Dictionary<string, Func<WebhookPayload, Task>> _eventHandlers;
    private readonly string? _webhookSecret;

    public WebhookHandler(
        ILogger<WebhookHandler> logger,
        IPluginManagerService pluginManager,
        IConfiguration? configuration = null)
    {
        _logger = logger;
        _pluginManager = pluginManager;
        _webhookSecret = configuration?["Webhooks:Secret"];
        _eventHandlers = new()
        {
            ["plugin.created"] = HandlePluginCreated,
            ["plugin.updated"] = HandlePluginUpdated,
            ["plugin.deleted"] = HandlePluginDeleted,
            ["plugin.security_patch"] = HandleSecurityPatch
        };
    }

    /// <summary>
    /// Processes an incoming webhook request.
    /// Validates signature and dispatches to appropriate handler.
    /// </summary>
    public async Task<bool> ProcessWebhookAsync(WebhookPayload payload, string? signature = null)
    {
        try
        {
            // Verify signature if secret is configured
            if (!string.IsNullOrEmpty(_webhookSecret) && !VerifySignature(payload, signature))
            {
                _logger.LogWarning("Webhook signature verification failed");
                return false;
            }

            if (!_eventHandlers.TryGetValue(payload.EventType.ToLowerInvariant(), out var handler))
            {
                _logger.LogWarning("Unknown webhook event type: {EventType}", payload.EventType);
                return false;
            }

            await handler(payload);
            _logger.LogInformation("Processed webhook: {EventType} for plugin {PluginId}",
                payload.EventType, payload.PluginId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return false;
        }
    }

    private bool VerifySignature(WebhookPayload payload, string? signature)
    {
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(_webhookSecret))
            return false;

        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payloadJson));
        var computed = Convert.ToHexString(hash);

        return signature.Equals(computed, StringComparison.OrdinalIgnoreCase);
    }

    private Task HandlePluginCreated(WebhookPayload payload)
    {
        _logger.LogInformation("Plugin created via webhook: {PluginId}", payload.PluginId);
        return Task.CompletedTask;
    }

    private async Task HandlePluginUpdated(WebhookPayload payload)
    {
        _logger.LogInformation("Plugin update notification: {PluginId} - Version {Version}",
            payload.PluginId, payload.Data?["version"]);

        // Could trigger hot reload here if desired
        await Task.CompletedTask;
    }

    private async Task HandlePluginDeleted(WebhookPayload payload)
    {
        _logger.LogInformation("Plugin deletion notification: {PluginId}", payload.PluginId);

        try
        {
            await _pluginManager.UnloadPluginAsync(payload.PluginId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to unload plugin from deletion webhook: {PluginId}", payload.PluginId);
        }
    }

    private Task HandleSecurityPatch(WebhookPayload payload)
    {
        _logger.LogWarning(
            "Security patch available for plugin: {PluginId} - Severity: {Severity}",
            payload.PluginId,
            payload.Data?.ContainsKey("severity") == true ? payload.Data["severity"] : "Unknown");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers a custom webhook event handler.
    /// </summary>
    public void RegisterEventHandler(string eventType, Func<WebhookPayload, Task> handler)
    {
        _eventHandlers[eventType.ToLowerInvariant()] = handler;
    }
}

/// <summary>
/// Represents the payload of a webhook event.
/// </summary>
public sealed class WebhookPayload
{
    public required Guid PluginId { get; set; }
    public required string EventType { get; set; }
    public required DateTime TimestampUtc { get; set; }
    public Dictionary<string, object> Data { get; set; } = [];
}
