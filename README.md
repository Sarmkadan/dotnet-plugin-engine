# dotnet-plugin-engine

[... existing content ...]

## PluginEventPublisherExtensions

The `PluginEventPublisherExtensions` class provides a set of extension methods for `PluginEventPublisher` that simplify event publishing, subscription management, and diagnostics. It offers both synchronous and asynchronous APIs for publishing events, batch operations for efficiency, and utilities for monitoring publisher state and managing subscriptions.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

// Create a publisher instance
var publisher = new PluginEventPublisher();

// Publish a single event synchronously
publisher.Publish(new PluginLoadedEvent("MyPlugin", "1.0.0"));

// Get current statistics
var stats = publisher.GetCurrentStatistics();
Console.WriteLine(publisher.GetStatisticsString());

// Subscribe to events
publisher.Subscribe<PluginLoadedEvent>(loadedEvent => {
    Console.WriteLine($"Plugin loaded: {loadedEvent.PluginName} v{loadedEvent.Version}");
    return Task.CompletedTask;
});

// Check subscriber count
var subscriberCount = publisher.GetSubscriberCount<PluginLoadedEvent>();
Console.WriteLine($"Subscribers for PluginLoadedEvent: {subscriberCount}");

// Subscribe once (auto-unsubscribe after first event)
var oneTimeDisposable = publisher.SubscribeOnce<PluginUnloadedEvent>(unloadedEvent => {
    Console.WriteLine($"Plugin unloaded: {unloadedEvent.PluginName}");
    return Task.CompletedTask;
});

// Publish multiple events as a batch (more efficient than individual publishes)
publisher.PublishBatchAsync(new[] {
    new PluginLoadedEvent("PluginA", "1.0.0"),
    new PluginLoadedEvent("PluginB", "2.0.0"),
    new PluginLoadedEvent("PluginC", "1.5.0")
}).Wait();

// Get all monitored event types
var monitoredTypes = publisher.GetMonitoredEventTypes();
Console.WriteLine($"Monitored event types: {string.Join(", ", monitoredTypes.Select(t => t.Name))}");

// Unsubscribe all handlers for a specific event type
publisher.UnsubscribeAll<PluginLoadedEvent>();
```

## PluginExtensions

The `PluginExtensions` class provides a comprehensive set of extension methods for the `Plugin` class that simplify common plugin management operations. It offers utilities for checking plugin state, formatting metadata, managing timestamps, and analyzing plugin dependencies and capabilities, all with proper null-safety and culture-invariant formatting.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Domain.Entities;
using System;

// Create a sample plugin
var plugin = new Plugin
{
    Name = "AnalyticsDashboard",
    Version = "2.1.0",
    Author = "Acme Corp",
    Description = "Provides advanced analytics and reporting capabilities",
    Status = PluginStatus.Active,
    CreatedAt = DateTime.UtcNow.AddDays(-30),
    ModifiedAt = DateTime.UtcNow.AddDays(-5),
    Dependencies = new List<PluginDependency> { new PluginDependency("Logging", "1.0.0") },
    Capabilities = new List<string> { "Reporting", "Dashboard", "Export" }
};

// Check plugin state
Console.WriteLine($"Is active: {plugin.IsActive()}");
Console.WriteLine($"Is failed: {plugin.IsFailed()}");
Console.WriteLine($"Is transitioning: {plugin.IsTransitioning()}");

// Get formatted metadata
Console.WriteLine($"Display name: {plugin.GetDisplayName()}");
Console.WriteLine($"Created: {plugin.GetFormattedCreationDate()}");
Console.WriteLine($"Modified: {plugin.GetFormattedModificationDate()}");
Console.WriteLine($"Age in days: {plugin.GetAgeInDays()}");

// Check dependencies and capabilities
Console.WriteLine($"Has dependencies: {plugin.HasDependencies()}");
Console.WriteLine($"Has capabilities: {plugin.HasCapabilities()}");

// Get metadata summary
Console.WriteLine($"Metadata: {plugin.GetMetadataSummary()}");

// Update modification timestamp
plugin.Touch();
Console.WriteLine($"New modified date: {plugin.GetFormattedModificationDate()}");
```

## RemotePluginRegistryValidation

The `RemotePluginRegistryValidation` class provides validation helpers for `PluginInfo`, `PluginVersionInfo`, `PluginPublishMetadata`, and `RemotePluginRegistry` instances. It offers extension methods that validate plugin metadata against semantic versioning rules, URL formats, and required fields, returning detailed error messages for invalid data or throwing exceptions when strict validation is required.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Integration;
using System;
using System.Linq;

// Create valid plugin info
var validPluginInfo = new PluginInfo {
    Id = Guid.NewGuid(),
    Name = "MyAwesomePlugin",
    Version = "1.0.0",
    DownloadUrl = "https://example.com/plugins/myplugin.zip"
};

// Validate plugin info - returns list of problems (empty if valid)
var validationErrors = validPluginInfo.Validate();
if (validationErrors.Any())
{
    Console.WriteLine("Plugin validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Plugin info is valid!");
}

// Check if plugin info is valid using IsValid extension
if (validPluginInfo.IsValid())
{
    Console.WriteLine("Plugin info passed validation checks");
}

// Validate plugin version info
var versionInfo = new PluginVersionInfo {
    Version = "2.1.0",
    PublishedAtUtc = DateTime.UtcNow,
    DownloadUrl = "https://example.com/plugins/myplugin-v2.1.0.zip"
};

if (!versionInfo.IsValid())
{
    Console.WriteLine("Version info is invalid");
}

// Validate plugin publish metadata
var publishMetadata = new PluginPublishMetadata {
    PluginName = "MyAwesomePlugin",
    Version = "1.0.0",
    Description = "A sample plugin for the plugin engine",
    Author = "John Doe",
    Tags = new[] { "sample", "demo", "plugin" }
};

// Use EnsureValid to throw if invalid
try
{
    publishMetadata.EnsureValid();
    Console.WriteLine("Publish metadata is valid");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate RemotePluginRegistry
var registry = new RemotePluginRegistry();
registry.EnsureValid(); // Throws if null

// Batch validation example
var plugins = new[] { validPluginInfo, null };
foreach (var plugin in plugins)
{
    var errors = plugin.Validate();
    if (errors.Count > 0)
    {
        Console.WriteLine($"Plugin {plugin?.Name ?? "null"} has {errors.Count} validation errors");
    }
}
```

## PluginEventPublisherTests

The `PluginEventPublisherTests` class contains unit tests for the `PluginEventPublisher` class, which provides functionality for publishing plugin events to subscribers. It tests various scenarios including publishing with no subscribers, publishing with one or multiple subscribers, unsubscribing handlers, and publishing different event types. Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Events;
using PluginEngine.Tests;
using Xunit;

public class PluginEventPublisherDemo
{
    public void DemonstratePluginEventPublisherTests()
    {
        var tests = new PluginEventPublisherTests();

        // Test publishing with no subscribers
        tests.PublishAsync_WithNoSubscribers_CompletesWithoutError().Wait();

        // Test publishing with one subscriber
        var invoked = false;
        Func<PluginLoadedEvent, Task> handler = _ => { invoked = true; return Task.CompletedTask; };
        tests.PublishAsync_WithOneSubscriber_InvokesHandler().Wait();

        // Test publishing with multiple subscribers
        var count = 0;
        tests.PublishAsync_WithMultipleSubscribers_InvokesAllHandlers().Wait();

        // Test unsubscribing a handler
        tests.Unsubscribe_AfterSubscribing_HandlerIsNotInvokedOnNextPublish().Wait();

        // Test publishing with different event types
        tests.PublishAsync_WithDifferentEventTypes_DeliverOnlyToMatchingSubscribers().Wait();

        // Test publishing an error event
        tests.PublishAsync_ErrorEvent_InvokesCorrectSubscriber().Wait();

        // Test getting statistics initially
        tests.GetStatistics_Initially_ReturnsZeroCounts();

        // Test getting statistics after subscribing
        tests.GetStatistics_AfterSubscribing_ReflectsSubscriberCount();

        // Test getting statistics after publishing
        tests.GetStatistics_AfterPublishing_IncrementsEventCount().Wait();

        // Test getting statistics after unsubscribing
        tests.GetStatistics_AfterUnsubscribing_DecreasesCount();

        // Test concurrent publishes
        tests.PublishAsync_ConcurrentPublishes_AllHandlersReceiveEvents().Wait();
    }
}
```

## PluginExecutionContextTests

[... existing content ...]

## PluginEntityTests

[... existing content ...]

## HotSwapServiceTests

[... existing content ...]

## VersionHelperTests

[... existing content ...]

## StringExtensionsTests

[... existing content ...]

## PluginOperationResultTests

[... existing content ...]

## PluginValidatorTests

[... existing content ...]

## DependencyGraphAnalyzerTests

[... existing content ...]

## PluginLifecycleTests

[... existing content ...]

## PluginDependencyResolverTests

[... existing content ...]

## MemoryPluginCacheTests

[... existing content ...]
```