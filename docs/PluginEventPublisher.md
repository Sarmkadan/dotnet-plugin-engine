# PluginEventPublisher

The `PluginEventPublisher` provides a centralized messaging mechanism designed for decoupled communication within a plugin-based architecture. It facilitates event-driven interactions, allowing independent modules to broadcast events and react to them without requiring direct references between the publisher and the subscribers.

## API

### Constructors

*   **`PluginEventPublisher()`**
    Initializes a new instance of the `PluginEventPublisher` class.

### Methods

*   **`Task PublishAsync<T>(T eventData)`**
    Asynchronously broadcasts an event of type `T` to all registered subscribers.
    *   **Parameters:** `eventData` — The event object to be published.
    *   **Returns:** A `Task` representing the asynchronous publication operation.
    *   **Exceptions:** Throws `ArgumentNullException` if `eventData` is null.

*   **`void Subscribe<T>(Action<T> handler)`**
    Registers a callback handler for events of type `T`.
    *   **Parameters:** `handler` — The action to execute when an event of type `T` is published.
    *   **Exceptions:** Throws `ArgumentNullException` if `handler` is null.

*   **`void Unsubscribe<T>(Action<T> handler)`**
    Removes a previously registered handler for events of type `T`.
    *   **Parameters:** `handler` — The action to remove.
    *   **Exceptions:** Throws `ArgumentNullException` if `handler` is null.

*   **`EventPublisherStatistics GetStatistics()`**
    Retrieves current runtime statistics for the publisher.
    *   **Returns:** An `EventPublisherStatistics` object containing current publication metrics.

*   **`void RemoveSubscribersForContext(string contextId)`**
    Removes all subscribers associated with the specified context identifier.
    *   **Parameters:** `contextId` — The identifier for the context from which subscribers should be removed.
    *   **Exceptions:** Throws `ArgumentNullException` if `contextId` is null or whitespace.

### Properties

*   **`long EventsPublished`**
    Gets the total number of events successfully published through this instance.

*   **`int RegisteredSubscribers`**
    Gets the total count of currently active subscriber registrations.

*   **`int MonitoredEventTypes`**
    Gets the count of distinct event types currently having one or more active subscribers.

*   **`DateTime Timestamp`**
    Gets the date and time of the last recorded event publication or subscription activity.

## Usage

### Example 1: Basic Event Subscription and Publication

```csharp
public record UserLoggedInEvent(string Username);

// Setup
var publisher = new PluginEventPublisher();

// Subscriber
publisher.Subscribe<UserLoggedInEvent>(e => 
    Console.WriteLine($"User logged in: {e.Username}"));

// Publisher
await publisher.PublishAsync(new UserLoggedInEvent("Alice"));
```

### Example 2: Managing Subscribers by Context

```csharp
var publisher = new PluginEventPublisher();
string contextId = "ReportingModule_v1";

// Subscribers for specific module
publisher.Subscribe<DataProcessedEvent>(e => Log(e));

// Removing all subscribers associated with the module context
publisher.RemoveSubscribersForContext(contextId);
```

## Notes

*   **Thread Safety:** `PluginEventPublisher` is designed to be thread-safe. Multiple threads can safely subscribe, unsubscribe, or publish events concurrently without external synchronization.
*   **Execution Order:** Subscribers are invoked synchronously when `PublishAsync` is called; however, the `Task` returned ensures that the publication process completes asynchronously relative to the caller. If a subscriber handler throws an exception, it may interrupt the notification chain for subsequent subscribers.
*   **Memory Management:** Use `Unsubscribe` or `RemoveSubscribersForContext` to prevent memory leaks if plugins or subscriber contexts are frequently created and destroyed during the application lifecycle.
