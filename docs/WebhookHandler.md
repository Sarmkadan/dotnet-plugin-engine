# WebhookHandler

The `WebhookHandler` class provides the infrastructure necessary to receive, parse, and route webhook payloads to appropriate handlers within the `dotnet-plugin-engine`. It acts as the primary interface for managing event subscriptions and ensures that incoming webhooks are processed by the logic registered to the corresponding plugin and event type.

## API

*   `WebhookHandler()`: Initializes a new instance of the `WebhookHandler` class.
*   `Task<bool> ProcessWebhookAsync()`: Asynchronously processes the current webhook configuration and payload. Returns `true` if processing is successful, and `false` otherwise.
*   `void RegisterEventHandler()`: Registers an event handler to be invoked when a webhook of the corresponding type is processed.
*   `Guid PluginId`: Gets or sets the unique identifier for the plugin responsible for this webhook. This property is required.
*   `string EventType`: Gets or sets the identifier for the type of event being handled. This property is required.
*   `DateTime TimestampUtc`: Gets or sets the UTC timestamp indicating when the webhook event occurred. This property is required.
*   `Dictionary<string, object> Data`: Gets or sets the payload data associated with the webhook.

## Usage

**Registering an Event Handler**
```csharp
var handler = new WebhookHandler();
// Assuming a delegate is passed to handle the specific event type
handler.RegisterEventHandler("UserRegistered", (data) =>
{
    Console.WriteLine($"Processing registration for user: {data["Username"]}");
});
```

**Processing a Webhook**
```csharp
var handler = new WebhookHandler
{
    PluginId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
    EventType = "UserRegistered",
    TimestampUtc = DateTime.UtcNow,
    Data = new Dictionary<string, object> { { "Username", "exampleUser" } }
};

bool success = await handler.ProcessWebhookAsync();
```

## Notes

*   **Thread Safety**: The `ProcessWebhookAsync` method is designed to be asynchronous; however, the thread-safety of the final event execution depends entirely on the implementation of the handlers registered via `RegisterEventHandler`. Ensure that registered callbacks are thread-safe if the engine processes webhooks concurrently.
*   **Required Properties**: `PluginId`, `EventType`, and `TimestampUtc` must be set before invoking `ProcessWebhookAsync`. Failure to initialize these required properties may result in validation errors depending on the engine's runtime state.
*   **Data Integrity**: The `Data` dictionary is mutable. It is recommended to populate this dictionary completely before calling `ProcessWebhookAsync` to avoid race conditions during the processing lifecycle.
