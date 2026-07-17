# PluginEventPublisherExtensions

PluginEventPublisherExtensions provides extension methods for managing event publishing and subscription within the dotnet-plugin-engine framework. It enables plugins to publish events, subscribe to event types, and monitor subscription statistics through a centralized event publisher interface.

## API

### `Publish<T>(IEventPublisher, T)`
Publishes a single event to all subscribers registered for the event type `T`.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.
- `event` (`T`): The event instance to publish.

**Returns**
- `void`

**Exceptions**
- Throws `ArgumentNullException` if `publisher` or `event` is null.
- May throw exceptions from subscriber handlers during invocation.

---

### `PublishBatchAsync<T>(IEventPublisher, IEnumerable<T>)`
Asynchronously publishes a batch of events to all subscribers registered for the event type `T`.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.
- `events` (`IEnumerable<T>`): The collection of event instances to publish.

**Returns**
- `Task`: A task representing the asynchronous operation.

**Exceptions**
- Throws `ArgumentNullException` if `publisher` or `events` is null.
- May throw exceptions from subscriber handlers during invocation.

---

### `GetCurrentStatistics(IEventPublisher)`
Retrieves runtime statistics for the event publisher, including counts of published events and active subscriptions.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.

**Returns**
- `EventPublisherStatistics`: An object containing current statistics.

**Exceptions**
- Throws `ArgumentNullException` if `publisher` is null.

---

### `Subscribe<T>(IEventPublisher, Action<T>)`
Registers a synchronous handler for events of type `T`.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.
- `handler` (`Action<T>`): The handler to invoke when events are published.

**Returns**
- `void`

**Exceptions**
- Throws `ArgumentNullException` if `publisher` or `handler` is null.

---

### `UnsubscribeAll<T>(IEventPublisher)`
Removes all subscribers registered for events of type `T`.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.

**Returns**
- `void`

**Exceptions**
- Throws `ArgumentNullException` if `publisher` is null.

---

### `GetStatisticsString(IEventPublisher)`
Returns a formatted string representation of the current event publisher statistics.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.

**Returns**
- `string`: A human-readable statistics summary.

**Exceptions**
- Throws `ArgumentNullException` if `publisher` is null.

---

### `SubscribeOnce<T>(IEventPublisher, Action<T>)`
Registers a handler that automatically unsubscribes after the first event is received.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.
- `handler` (`Action<T>`): The handler to invoke once.

**Returns**
- `IDisposable`: A disposable handle to manually unsubscribe before the first event.

**Exceptions**
- Throws `ArgumentNullException` if `publisher` or `handler` is null.

---

### `SubscribeOnce<T>(IEventPublisher, Func<T, Task>)`
Registers an asynchronous handler that automatically unsubscribes after the first event is received.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.
- `handler` (`Func<T, Task>`): The asynchronous handler to invoke once.

**Returns**
- `IDisposable`: A disposable handle to manually unsubscribe before the first event.

**Exceptions**
- Throws `ArgumentNullException` if `publisher` or `handler` is null.

---

### `GetMonitoredEventTypes(IEventPublisher)`
Returns all event types currently being monitored by the publisher.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.

**Returns**
- `IEnumerable<Type>`: The collection of monitored event types.

**Exceptions**
- Throws `ArgumentNullException` if `publisher` is null.

---

### `GetSubscriberCount<T>(IEventPublisher)`
Returns the number of active subscribers for events of type `T`.

**Parameters**
- `publisher` (`IEventPublisher`): The event publisher instance.

**Returns**
- `int`: The subscriber count.

**Exceptions**
- Throws `ArgumentNullException` if `publisher` is null.

---

### `SubscriptionHandle`
Represents a handle to a subscription, allowing manual disposal to unsubscribe.

#### `Dispose()`
Unsubscribes the associated handler from the event publisher.

**Exceptions**
- Does not throw exceptions. Safe to call multiple times.

---

## Usage

### Basic Event Publishing and Subscription
```csharp
var publisher = serviceProvider.GetRequiredService<IEventPublisher>();
publisher.Subscribe<MyCustomEvent>(e => Console.WriteLine($"Received: {e.Message}"));
publisher.Publish(new MyCustomEvent { Message = "Hello, plugins!" });
// Output: Received: Hello, plugins!
```

### Batch Publishing with Statistics Monitoring
```csharp
var publisher = serviceProvider.GetRequiredService<IEventPublisher>();
var events = new[] { new DataEvent(1), new DataEvent(2), new DataEvent(3) };

await publisher.PublishBatchAsync(events);
Console.WriteLine(publisher.GetStatisticsString());
// Output: Events Published: 3 | Active Subscriptions: 0
```

---

## Notes

- All methods are thread-safe for concurrent invocation.
- `SubscribeOnce` handlers are automatically removed after the first invocation, even if an exception occurs.
- `UnsubscribeAll<T>` is a no-op if no subscribers exist for the specified type.
- `GetSubscriberCount<T>` returns 0 for unmonitored event types.
- Exceptions thrown by subscriber handlers are not caught; they propagate to the publisher.
- `SubscriptionHandle.Dispose()` is idempotent and safe to call multiple times.
