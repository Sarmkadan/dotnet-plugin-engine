# Event System Guide

This document describes the event system in the `dotnet-plugin-engine`, covering core interfaces, base classes, built-in events, the publisher/subscriber pattern, and usage examples.

## Core Interfaces and Base Classes

### `IPluginEvent`
The base interface for all plugin events in the system.

| Member | Type | Description |
|--------|------|-------------|
| `EventId` | `Guid` | Unique identifier for the event instance |
| `OccurredAtUtc` | `DateTime` | Timestamp when the event occurred (UTC) |
| `PluginId` | `Guid` | Identifier of the plugin associated with the event |
| `EventType` | `string` | Name of the event type |

### `PluginEventBase`
Abstract base class providing common implementation for plugin events.

| Member | Type | Description |
|--------|------|-------------|
| `EventId` | `Guid` | Generated automatically via `Guid.NewGuid()` |
| `OccurredAtUtc` | `DateTime` | Set automatically to `DateTime.UtcNow` |
| `PluginId` | `Guid` | Required property to be set by derived events |
| `EventType` | `string` | Abstract property to be implemented by derived events |

### `IPluginEventSubscriber`
Interface for subscribing to event types using callback delegates.

| Member | Type | Description |
|--------|------|-------------|
| `Subscribe<T>(Func<T, Task> handler)` | `void` | Registers a handler for events of type `T` |
| `Unsubscribe<T>(Func<T, Task> handler)` | `void` | Removes a previously registered handler |

## Built-in Events

The system includes five built-in event types:

### `PluginLoadedEvent`
Raised when a plugin is successfully loaded.
- `PluginName` (`string`): Name of the loaded plugin (required)
- `Version` (`string`): Version of the loaded plugin (required)
- `LoadTimeMs` (`long`): Time taken to load the plugin in milliseconds

### `PluginUnloadedEvent`
Raised when a plugin is unloaded.
- `PluginName` (`string`): Name of the unloaded plugin (required)
- `Reason` (`string?`): Optional reason for unloading

### `PluginUpdatedEvent`
Raised when a plugin is updated to a new version.
- `PreviousVersion` (`string`): Version before update (required)
- `NewVersion` (`string`): Version after update (required)
- `ChangesSummary` (`string`): Summary of changes made (required)

### `PluginErrorEvent`
Raised when a plugin encounters an error.
- `ErrorMessage` (`string`): Description of the error (required)
- `ErrorDetails` (`string?`): Additional error details
- `ErrorCode` (`int`): Numeric error code

### `DependenciesResolvedEvent`
Raised when a plugin's dependencies have been resolved.
- `ResolvedDependencies` (`List<Guid>`): List of plugin IDs that were resolved
- `ResolutionTimeMs` (`long`): Time taken to resolve dependencies in milliseconds

## Publisher and Subscriber Pattern

### `IPluginEventPublisher`
Interface for publishing events to subscribers.

| Member | Type | Description |
|--------|------|-------------|
| `PublishAsync<T>(T @event)` | `Task` | Publishes an event to all subscribers of type `T` |
| `GetStatistics()` | `EventPublisherStatistics` | Returns current publisher statistics |
| `RemoveSubscribersForContext(AssemblyLoadContext)` | `void` | Removes all subscribers associated with the specified load context |

### `PluginEventPublisher`
Concrete implementation of `IPluginEventPublisher` that manages event routing.

#### Key Features:
- **Thread-Safe Management**: Uses locks to safely manage subscriptions and publishing state.
- **Re-entrancy Protection**: Detects and prevents infinite recursion if an event handler publishes the same event type again. Logs a warning and skips the nested publish.
- **Exception Aggregation**: Catches exceptions from individual handlers, continues dispatching to remaining subscribers, and throws an `AggregateException` containing all failures after all handlers are invoked.
- **Statistics Tracking**: Provides real-time metrics on publishing activity.

#### Statistics (`EventPublisherStatistics`)

| Property | Type | Description |
|----------|------|-------------|
| `EventsPublished` | `long` | Total number of events published |
| `RegisteredSubscribers` | `int` | Total number of registered event handlers |
| `MonitoredEventTypes` | `int` | Number of distinct event types with subscribers |
| `Timestamp` | `DateTime` | When the statistics were captured |

#### `RemoveSubscribersForContext`
This method is essential for preventing memory leaks when plugins are unloaded. It removes all event handlers that were registered by types belonging to the specified `AssemblyLoadContext`, which typically corresponds to a plugin's isolation context.

## Usage Example

The following example demonstrates how to subscribe to and publish events using the event system.

```csharp
using PluginEngine.Events;
using System.Threading.Tasks;

// Define a custom event (optional, built-in events can be used directly)
public class CustomPluginEvent : PluginEventBase
{
    public override string EventType => "CustomPluginEvent";
    public required string Message { get; set; }
}

// Example subscriber class
public class EventHandler
{
    private readonly IPluginEventPublisher _publisher;
    
    public EventHandler(IPluginEventPublisher publisher)
    {
        _publisher = publisher;
        
        // Subscribe to PluginLoaded events
        _publisher.Subscribe<PluginLoadedEvent>(HandlePluginLoadedAsync);
        
        // Subscribe to custom events
        _publisher.Subscribe<CustomPluginEvent>(HandleCustomEventAsync);
    }
    
    private Task HandlePluginLoadedAsync(PluginLoadedEvent @event)
    {
        // Handle plugin loaded event
        Console.WriteLine($"Plugin '{@event.PluginName}' v{@event.Version} loaded in {@event.LoadTimeMs}ms");
        return Task.CompletedTask;
    }
    
    private Task HandleCustomEventAsync(CustomPluginEvent @event)
    {
        // Handle custom event
        Console.WriteLine($"Custom event received: {@event.Message}");
        return Task.CompletedTask;
    }
    
    // Method to publish events
    public async Task PublishExampleEventsAsync(Guid pluginId)
    {
        // Publish a built-in event
        var loadedEvent = new PluginLoadedEvent
        {
            PluginId = pluginId,
            PluginName = "ExamplePlugin",
            Version = "1.0.0",
            LoadTimeMs = 250
        };
        
        await _publisher.PublishAsync(loadedEvent);
        
        // Publish a custom event
        var customEvent = new CustomPluginEvent
        {
            PluginId = pluginId,
            Message = "This is a custom event"
        };
        
        await _publisher.PublishAsync(customEvent);
    }
    
    // Cleanup subscriptions when no longer needed
    public void Dispose()
    {
        _publisher.Unsubscribe<PluginLoadedEvent>(HandlePluginLoadedAsync);
        _publisher.Unsubscribe<CustomPluginEvent>(HandleCustomEventAsync);
    }
}
```

### Example Usage in Application

```csharp
// Assuming you have access to an IPluginEventPublisher instance
var publisher = new PluginEventPublisher(logger);
var handler = new EventHandler(publisher);

// Somewhere in your plugin loading code:
await handler.PublishExampleEventsAsync(Guid.NewGuid());

// When cleaning up (e.g., plugin unload):
handler.Dispose();
// Or to remove all subscribers for a specific plugin context:
// publisher.RemoveSubscribersForContext(pluginAssemblyLoadContext);
```

## Important Notes

1. Event handlers should avoid long-running operations as they block the publishing thread until all handlers complete.
2. If an event handler throws an exception, the publisher catches it, continues to invoke other handlers, and then throws an `AggregateException` containing all collected exceptions.
3. Re-entrant publishing (publishing an event while the same event type is already being processed) is detected and prevented to avoid infinite recursion.
4. The `RemoveSubscribersForContext` method is critical for cleaning up subscribers when plugins are unloaded to prevent memory leaks.
