# PluginEventTypes

Documentation for the plugin event system in the PluginEngine.Events namespace.

## IPluginEvent

Base interface for all plugin events in the system. Supports event sourcing and event-driven architecture.

| Property | Description |
|----------|-------------|
| Guid EventId { get; } | Gets the event ID for tracking. |
| DateTime OccurredAtUtc { get; } | Gets the time the event occurred. |
| Guid PluginId { get; } | Gets the plugin associated with the event. |
| string EventType { get; } | Gets the event type name. |

## IPluginEventHandler<T>

Handler interface for processing specific event types.

| Method | Description |
|--------|-------------|
| Task HandleAsync(T @event) | Handles an event of type T. |

## IPluginEventPublisher

Publisher for raising plugin events throughout the system.

| Method | Description |
|--------|-------------|
| Task PublishAsync<T>(T @event) | Publishes an event to all subscribers. |
| EventPublisherStatistics GetStatistics() | Gets publisher statistics for monitoring. |
| void RemoveSubscribersForContext(AssemblyLoadContext context) | Removes all subscribers belonging to the specified AssemblyLoadContext. |

## IPluginEventSubscriber

Subscriber for registering interest in specific event types.

| Method | Description |
|--------|-------------|
| void Subscribe<T>(Func<T, Task> handler) | Subscribes to an event type with a handler callback. |
| void Unsubscribe<T>(Func<T, Task> handler) | Unsubscribes a handler from an event type. |

## PluginEventBase

Base class for plugin events with common properties.

| Property | Description |
|----------|-------------|
| Guid EventId { get; } | Gets the event ID for tracking. |
| DateTime OccurredAtUtc { get; } | Gets the time the event occurred. |
| Guid PluginId { get; set; } | Gets or sets the plugin associated with the event. |
| string EventType { get; } | Gets the event type name. Must be overridden in derived classes. |

## PluginLoadedEvent

Event raised when a plugin is loaded.

| Property | Description |
|----------|-------------|
| Guid EventId { get; } | Inherited from PluginEventBase. |
| DateTime OccurredAtUtc { get; } | Inherited from PluginEventBase. |
| Guid PluginId { get; set; } | Inherited from PluginEventBase. |
| string EventType { get; } | Returns "PluginLoaded". |
| string PluginName { get; set; } | Gets or sets the name of the plugin. |
| string Version { get; set; } | Gets or sets the version of the plugin. |
| long LoadTimeMs { get; set; } | Gets or sets the load time in milliseconds. |

## PluginUnloadedEvent

Event raised when a plugin is unloaded.

| Property | Description |
|----------|-------------|
| Guid EventId { get; } | Inherited from PluginEventBase. |
| DateTime OccurredAtUtc { get; } | Inherited from PluginEventBase. |
| Guid PluginId { get; set; } | Inherited from PluginEventBase. |
| string EventType { get; } | Returns "PluginUnloaded". |
| string PluginName { get; set; } | Gets or sets the name of the plugin. |
| string? Reason { get; set; } | Gets or sets the reason for unloading the plugin. |

## PluginUpdatedEvent

Event raised when a plugin is updated.

| Property | Description |
|----------|-------------|
| Guid EventId { get; } | Inherited from PluginEventBase. |
| DateTime OccurredAtUtc { get; } | Inherited from PluginEventBase. |
| Guid PluginId { get; set; } | Inherited from PluginEventBase. |
| string EventType { get; } | Returns "PluginUpdated". |
| string PreviousVersion { get; set; } | Gets or sets the previous version of the plugin. |
| string NewVersion { get; set; } | Gets or sets the new version of the plugin. |
| string ChangesSummary { get; set; } | Gets or sets a summary of the changes made. |

## PluginErrorEvent

Event raised when a plugin encounters an error.

| Property | Description |
|----------|-------------|
| Guid EventId { get; } | Inherited from PluginEventBase. |
| DateTime OccurredAtUtc { get; } | Inherited from PluginEventBase. |
| Guid PluginId { get; set; } | Inherited from PluginEventBase. |
| string EventType { get; } | Returns "PluginError". |
| string ErrorMessage { get; set; } | Gets or sets the error message. |
| string? ErrorDetails { get; set; } | Gets or sets additional error details. |
| int ErrorCode { get; set; } | Gets or sets the error code. |

## DependenciesResolvedEvent

Event raised when dependencies are resolved.

| Property | Description |
|----------|-------------|
| Guid EventId { get; } | Inherited from PluginEventBase. |
| DateTime OccurredAtUtc { get; } | Inherited from PluginEventBase. |
| Guid PluginId { get; set; } | Inherited from PluginEventBase. |
| string EventType { get; } | Returns "DependenciesResolved". |
| List<Guid> ResolvedDependencies { get; set; } | Gets or sets the list of resolved dependency IDs. |
| long ResolutionTimeMs { get; set; } | Gets or sets the resolution time in milliseconds. |

## Usage

### Subscribing to a PluginLoadedEvent

```csharp
var subscriber = ...; // obtain IPluginEventSubscriber instance
subscriber.Subscribe<PluginLoadedEvent>(async @event =>
{
    Console.WriteLine($"Plugin {event.PluginName} version {event.Version} loaded in {event.LoadTimeMs}ms");
});
```