// entire file content ...
## WebhookHandler
The `WebhookHandler` class is responsible for processing incoming webhooks from external systems about plugin updates and events. It provides secure webhook processing with signature verification and dispatches to registered event handlers.

### Usage Example

```csharp
using PluginEngine.Integration;

// Create an instance of WebhookHandler
var webhookHandler = new WebhookHandler(
    logger: /* obtain ILogger<WebhookHandler> */,
    pluginManager: /* obtain IPluginManagerService */,
    configuration: /* obtain IConfiguration? */);

// Process a webhook
var webhookPayload = new WebhookPayload
{
    PluginId = Guid.NewGuid(),
    EventType = "plugin.created",
    TimestampUtc = DateTime.UtcNow,
    Data = new Dictionary<string, object>
    {
        ["version"] = "1.0.0",
        ["loadTimeMs"] = 150
    }
};
await webhookHandler.ProcessWebhookAsync(webhookPayload);

// Register a custom event handler
webhookHandler.RegisterEventHandler("plugin.updated", async payload =>
{
    Console.WriteLine($"Plugin updated: {payload.PluginId} - Version {payload.Data?["version"]}");
    await Task.CompletedTask;
});
```
// ... goes in between
