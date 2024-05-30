# PluginEventPublisherTests

The `PluginEventPublisherTests` class contains unit tests for the `PluginEventPublisher` type. Each test method validates a specific behavior of the publisher, including subscription management, event dispatch, error handling, statistics tracking, and concurrent publishing. The tests are designed to run with an asynchronous test framework (e.g., xUnit) and assume the `PluginEventPublisher` is correctly implemented.

## API

### `public PluginEventPublisherTests()`
Default constructor. No special initialization is performed; test fixtures or shared state are typically managed via the test framework’s lifecycle attributes (not shown here).

### `public async Task PublishAsync_WithNoSubscribers_CompletesWithoutError()`
Verifies that calling `PublishAsync` when no handlers are subscribed completes without throwing an exception.  
- **Parameters**: None.  
- **Returns**: `Task`.  
- **Throws**: None under normal test conditions; test fails if an exception is thrown.

### `public async Task PublishAsync_WithOneSubscriber_InvokesHandler()`
Ensures that a single subscribed handler is invoked exactly once when an event is published.  
- **Parameters**: None.  
- **Returns**: `Task`.  
- **Throws**: None; test assertion fails if handler is not called.

### `public async Task PublishAsync_WithMultipleSubscribers_InvokesAllHandlers()`
Confirms that all subscribed handlers are invoked when an event is published.  
- **Parameters**: None.  
- **Returns**: `Task`.  
- **Throws**: None; test fails if any handler is missed.

### `public async Task PublishAsync_HandlerReceivesCorrectEvent()`
Validates that the handler receives the exact event object that was published.  
- **Parameters**: None.  
- **Returns**: `Task`.  
- **Throws**: None; test fails if the received event differs from the published one.

### `public async Task Unsubscribe_AfterSubscribing_HandlerIsNotInvokedOnNextPublish()`
Tests that after unsubscribing a handler, subsequent publishes do not invoke that handler.  
- **Parameters**: None.  
- **Returns**: `Task`.  
- **Throws**: None; test fails if the handler is still called.

### `public void Unsubscribe_ForHandlerThatWasNeverRegistered_DoesNotThrow()`
Verifies that calling `Unsubscribe` with a handler that was never registered does not throw an exception.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: None; test fails if an exception is thrown.

### `public async Task PublishAsync_WithDifferentEventTypes_DeliverOnlyToMatchingSubscribers()`
Ensures that handlers subscribed to a specific event type are not invoked when a different event type is published.  
- **Parameters**: None.  
- **Returns**: `Task`.  
- **Throws**: None; test fails if a non-matching handler is called.

### `public async Task PublishAsync_ErrorEvent_InvokesCorrectSubscriber()`
Confirms that an error event (or a dedicated error-handling subscriber) is invoked appropriately when an error occurs during publishing.  
- **Parameters**: None.  
- **Returns**: `Task`.  
- **Throws**: None; test fails if the error subscriber is not called or the wrong subscriber is called.

### `public void GetStatistics_Initially_ReturnsZeroCounts()`
Checks that the statistics object returned by `GetStatistics` has zero values before any subscriptions or publishes.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: None; test fails if counts are non-zero.

### `public void GetStatistics_AfterSubscribing_ReflectsSubscriberCount()`
Verifies that after subscribing one or more handlers, the statistics reflect the correct subscriber count.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: None; test fails if subscriber count is incorrect.

### `public async Task GetStatistics_AfterPublishing_IncrementsEventCount()`
Ensures that after publishing an event, the statistics show an incremented event count.  
- **Parameters**: None.  
- **Returns**: `Task`.  
- **Throws**: None; test fails if event count is not incremented.

### `public void GetStatistics_AfterUnsubscribing_DecreasesCount()`
Confirms that unsubscribing a handler decreases the subscriber count in the statistics.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: None; test fails if count does not decrease.

### `public async Task PublishAsync_ConcurrentPublishes_AllHandlersReceiveEvents()`
Tests that when multiple publishes occur concurrently, every handler receives the events it is subscribed to, without data loss or corruption.  
- **Parameters**: None.  
- **Returns**: `Task`.  
- **Throws**: None; test fails if any handler misses an event or receives an incorrect event.

## Usage

The following examples demonstrate how to use `PluginEventPublisher` in a test context, mirroring the patterns verified by the test methods.

### Example 1: Subscribing and Publishing

```csharp
using var publisher = new PluginEventPublisher();

// Subscribe a handler
var receivedEvent = null as MyEvent;
publisher.Subscribe<MyEvent>(e => receivedEvent = e);

// Publish an event
var myEvent = new MyEvent { Data = "test" };
await publisher.PublishAsync(myEvent);

// Assert the handler was invoked with the correct event
Assert.NotNull(receivedEvent);
Assert.Equal("test", receivedEvent.Data);
```

### Example 2: Unsubscribing and Checking Statistics

```csharp
using var publisher = new PluginEventPublisher();

var handler = (MyEvent e) => { /* do nothing */ };
publisher.Subscribe(handler);

// Verify subscriber count
var stats = publisher.GetStatistics();
Assert.Equal(1, stats.SubscriberCount);

// Unsubscribe
publisher.Unsubscribe(handler);
stats = publisher.GetStatistics();
Assert.Equal(0, stats.SubscriberCount);

// Publishing after unsubscribe should not invoke the handler
bool wasCalled = false;
publisher.Subscribe<MyEvent>(e => wasCalled = true);
publisher.Unsubscribe<MyEvent>(e => wasCalled = true);
await publisher.PublishAsync(new MyEvent());
Assert.False(wasCalled);
```

## Notes

- **Edge Cases**:  
  - Publishing with no subscribers completes silently (no exception).  
  - Unsubscribing a handler that was never registered is a no-op and does not throw.  
  - Handlers are only invoked for the exact event type they subscribed to; derived types may or may not be delivered depending on the publisher’s implementation (the test `PublishAsync_WithDifferentEventTypes_DeliverOnlyToMatchingSubscribers` verifies strict type matching).  
  - Error events are treated as a separate event type and are delivered only to subscribers of that error type.

- **Thread Safety**:  
  - The test `PublishAsync_ConcurrentPublishes_AllHandlersReceiveEvents` validates that the publisher handles concurrent `PublishAsync` calls correctly. The implementation is expected to use thread-safe collections or synchronization primitives to avoid race conditions.  
  - Subscribing and unsubscribing while publishes are in progress may require additional care; the test suite does not explicitly cover concurrent subscription changes, but the presence of a concurrency test suggests the publisher is designed for multi-threaded scenarios.

- **Statistics**:  
  - The `GetStatistics` method returns a snapshot of current counts. The returned object is not updated live; repeated calls return fresh snapshots.  
  - Statistics are updated atomically; the tests verify that counts are consistent after each operation.

- **Test Execution**:  
  - All test methods are parameterless and rely on the test framework to provide any necessary fixtures.  
  - Async methods use `Task` and should be awaited to ensure proper cleanup and exception propagation.
