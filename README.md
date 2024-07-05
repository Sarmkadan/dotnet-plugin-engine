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
## HttpPluginClient

The `HttpPluginClient` class is an HTTP client for communicating with remote plugin registries and services. It handles plugin updates, notifications, and metadata synchronization through a fluent interface that exposes plugin state and registry operations.

### Usage Example

```csharp
using PluginEngine.Integration;
using Microsoft.Extensions.Logging;

// Create required dependencies (typically injected via DI)
var httpClient = new HttpClient();
var logger = LoggerFactory.Create(builder => builder.AddConsole())
    .CreateLogger<HttpPluginClient>();
var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?> 
    {
        ["PluginRegistry:BaseUrl"] = "https://plugins.example.com"
    })
    .Build();

// Create an instance of HttpPluginClient
var pluginClient = new HttpPluginClient(
    httpClient: httpClient,
    logger: logger,
    configuration: configuration);

// Configure plugin state
pluginClient.PluginId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");
pluginClient.CurrentVersion = "1.0.0";
pluginClient.AvailableVersion = "1.1.0";
pluginClient.DownloadUrl = "https://plugins.example.com/plugins/123e4567-e89b-12d3-a456-426614174000/1.1.0/download";
pluginClient.IsSecurityUpdate = false;
pluginClient.ReleaseNotes = "Bug fixes and performance improvements";

// Check registry availability
var isAvailable = await pluginClient.IsAvailableAsync();
Console.WriteLine($"Registry available: {isAvailable}");

// Get plugin information
var pluginInfo = await pluginClient.GetPluginInfoAsync(pluginClient.PluginId);
if (pluginInfo != null)
{
    Console.WriteLine($"Plugin: {pluginInfo.Name} by {pluginInfo.Author}");
}

// Send a notification
var notification = new PluginNotification
{
    PluginId = pluginClient.PluginId,
    PluginName = "Example Plugin",
    OccurredAtUtc = DateTime.UtcNow,
    Metadata = new Dictionary<string, object>()
};
await pluginClient.SendNotificationAsync("plugin.installed", notification);

// Check for updates
var updates = await pluginClient.CheckForUpdatesAsync(
    new List<Guid> { pluginClient.PluginId });
if (updates.Count > 0)
{
    Console.WriteLine($"Update available: {updates[0].AvailableVersion}");
}

// Upload a plugin
var uploadSuccess = await pluginClient.UploadPluginAsync("./MyPlugin.dll", "Initial release");
Console.WriteLine($"Upload successful: {uploadSuccess}");
```

## RemotePluginRegistry

The `RemotePluginRegistry` class manages interaction with a remote plugin registry for discovering, downloading, and publishing plugins. It provides caching capabilities to minimize network requests and includes methods for searching plugins, retrieving plugin information, managing versions, and handling plugin lifecycle operations.

### Usage Example

```csharp
using PluginEngine.Integration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

// Create required dependencies (typically injected via DI)
var httpClient = new HttpPluginClient(/* configuration */);
var cache = new MemoryCache(new MemoryCacheOptions());
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RemotePluginRegistry>();
var versionHelper = new VersionHelper();

// Create an instance of RemotePluginRegistry
var registry = new RemotePluginRegistry(
    httpClient: httpClient,
    cache: cache,
    logger: logger,
    versionHelper: versionHelper);

// Search for plugins
var plugins = await registry.SearchAsync("logging", limit: 10);
Console.WriteLine($"Found {plugins.Count} plugins");

// Get detailed information about a specific plugin
var pluginId = Guid.NewGuid(); // Replace with actual plugin ID
var pluginInfo = await registry.GetPluginAsync(pluginId);
if (pluginInfo != null)
{
    Console.WriteLine($"Plugin: {pluginInfo.Name} by {pluginInfo.Author}");
}

// Get all available versions of a plugin
var versions = await registry.GetVersionsAsync(pluginId);
Console.WriteLine($"Available versions: {string.Join(", ", versions.Select(v => v.Version))}");

// Download a specific plugin version
var downloadPath = "./plugins";
var downloadedFile = await registry.DownloadPluginAsync(pluginId, "1.0.0", downloadPath);
if (downloadedFile != null)
{
    Console.WriteLine($"Downloaded to: {downloadedFile}");
}

// Publish a new plugin
var publishMetadata = new PluginPublishMetadata
{
    PluginName = "MyAwesomePlugin",
    Version = "1.0.0",
    Description = "A plugin for doing awesome things",
    Author = "My Name",
    Company = "My Company",
    Tags = new List<string> { "awesome", "utilities" },
    LicenseType = "MIT"
};

var publishResult = await registry.PublishPluginAsync("./MyAwesomePlugin.dll", publishMetadata);
Console.WriteLine(publishResult ? "Publish successful" : "Publish failed");

// Invalidate cache for a plugin
registry.InvalidateCache(pluginId);
```

## PluginOperationResult

The `PluginOperationResult` type and its generic variant `PluginOperationResult<T>` provide a standardized structure for handling plugin operation outcomes, including success/failure status, messages, and optional data. It simplifies error handling by encapsulating error codes, exception details, and operation timing in a consistent response wrapper.

### Usage Example

```csharp
using PluginEngine.Results;

// Create a successful result
var successResult = PluginOperationResult.CreateSuccess("Plugin loaded successfully", durationMs: 150);

// Create a result with data
var dataResult = PluginOperationResult<string>.CreateSuccess("Plugin Data", "Operation completed", durationMs: 50);

// Create a failure result
var failureResult = PluginOperationResult.CreateFailure("Failed to load plugin", errorCode: 1001, details: "Missing dependency");

// Handling exceptions
try 
{
    // Simulate operation...
    throw new Exception("Critical error");
}
catch (Exception ex)
{
    var exceptionResult = PluginOperationResult.FromException(ex);
    Console.WriteLine($"Result status: {exceptionResult.Success}, Error Code: {exceptionResult.ErrorCode}");
}

// Working with batches
var batchResult = new PluginBatchOperationResult();
batchResult.AddResult(Guid.NewGuid(), "PluginA", successResult);
batchResult.AddResult(Guid.NewGuid(), "PluginB", failureResult);

Console.WriteLine(batchResult.GetSummary());
```
