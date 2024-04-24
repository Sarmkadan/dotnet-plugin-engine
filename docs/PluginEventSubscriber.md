# PluginEventSubscriber

The `PluginEventSubscriber` class serves as a centralized event management component within the `dotnet-plugin-engine`, designed to facilitate decoupled communication between plugin lifecycle events and application-level components. It provides a robust mechanism for registering, managing, and invoking handlers for various plugin-related occurrences, such as plugin loading, unloading, updating, error reporting, and dependency resolution, allowing observers to react to system state changes without requiring direct dependencies on the plugin orchestration logic.

## API

*   **`PluginEventSubscriber()`**
    *   Purpose: Initializes a new instance of the `PluginEventSubscriber` class.
*   **`void Subscribe<T>(Action<T> handler)`**
    *   Purpose: Registers a delegate to be invoked when an event of type `T` is triggered.
    *   Parameters: `handler` (The action to execute when the event occurs).
*   **`void Unsubscribe<T>(Action<T> handler)`**
    *   Purpose: Removes a previously registered handler for event type `T`.
    *   Parameters: `handler` (The specific action delegate to remove).
*   **`void OnPluginLoaded(PluginEventArgs e)`**
    *   Purpose: Dispatches the `PluginLoaded` event to all registered subscribers.
    *   Parameters: `e` (The event data containing details about the loaded plugin).
*   **`void OnPluginUnloaded(PluginEventArgs e)`**
    *   Purpose: Dispatches the `PluginUnloaded` event to all registered subscribers.
    *   Parameters: `e` (The event data containing details about the unloaded plugin).
*   **`void OnPluginUpdated(PluginEventArgs e)`**
    *   Purpose: Dispatches the `PluginUpdated` event to all registered subscribers.
    *   Parameters: `e` (The event data containing details about the updated plugin).
*   **`void OnPluginError(PluginErrorEventArgs e)`**
    *   Purpose: Dispatches the `PluginError` event to all registered subscribers.
    *   Parameters: `e` (The event data containing details about the error).
*   **`void OnDependenciesResolved(PluginEventArgs e)`**
    *   Purpose: Dispatches the `DependenciesResolved` event to all registered subscribers.
    *   Parameters: `e` (The event data containing details about the resolved dependencies).
*   **`int GetSubscriptionCount()`**
    *   Purpose: Retrieves the total number of active subscriptions across all event types.
    *   Return Value: `int` representing the total subscription count.
*   **`void UnsubscribeAll<T>()`**
    *   Purpose: Removes all registered handlers for the specified event type `T`.
*   **`void RemoveSubscribersForContext(string contextId)`**
    *   Purpose: Removes all subscriptions explicitly associated with a provided context identifier.
    *   Parameters: `contextId` (The unique identifier of the context to purge).

## Usage

### Example 1: Subscribing to Plugin Lifecycle Events
```csharp
var subscriber = new PluginEventSubscriber();

// Register handlers for lifecycle events
subscriber.Subscribe<PluginEventArgs>(e => 
    Console.WriteLine($"Plugin loaded: {e.PluginName}"));

subscriber.Subscribe<PluginErrorEventArgs>(e => 
    Console.WriteLine($"Error in {e.PluginName}: {e.ErrorMessage}"));

// Trigger events (typically called by the engine)
subscriber.OnPluginLoaded(new PluginEventArgs("MyPlugin"));
```

### Example 2: Managing Context-Specific Subscriptions
```csharp
var subscriber = new PluginEventSubscriber();
string contextId = "ModuleA";

// Subscribe with context
subscriber.Subscribe<PluginEventArgs>(e => 
    Console.WriteLine($"Processing {e.PluginName} for {contextId}"));

// Clean up all subscriptions for a specific context
subscriber.RemoveSubscribersForContext(contextId);
```

## Notes

*   **Thread Safety:** The `PluginEventSubscriber` implementation is designed to be thread-safe regarding subscription management and event dispatching. Simultaneous calls to `Subscribe` and `Unsubscribe` from different threads will not corrupt internal handler collections.
*   **Handler Execution:** Handlers are invoked synchronously within the caller's thread context when an event is dispatched. Long-running or blocking operations within a subscriber handler will block the event dispatcher, potentially impacting the performance of the plugin engine.
*   **Unsubscribing:** Attempting to `Unsubscribe` a handler that has not been previously registered is a safe, no-op operation.
*   **Context Scope:** The `RemoveSubscribersForContext` method depends on the engine correctly associating subscriptions with a `contextId` during the registration phase. If handlers are not associated with a context, this method will not remove them.
