## PluginException The `PluginException` class represents a custom exception thrown by the plugin engine. It provides additional context and information about the error, including an error code, entity ID, and context dictionary. ### Usage Example ```csharp try { // Code that may throw a PluginException } catch (PluginException ex) { Console.WriteLine($"Error code: {ex.ErrorCode}"); Console.WriteLine($"Entity ID: {ex.EntityId}"); Console.WriteLine($"Context: {ex.Context}"); Console.WriteLine(ex.ToString()); } ``` ## VersionMismatchException The `VersionMismatchException` is thrown when version constraints between components are not satisfied. It provides detailed information about the expected and actual versions, along with the component type and name that caused the mismatch. ### Usage Example ```csharp using PluginEngine.Exceptions; // Validate plugin version compatibility if (plugin.Version != expectedVersion) { throw new VersionMismatchException( message: $"Plugin version mismatch detected", expectedVersion: expectedVersion, actualVersion: plugin.Version, componentType: "Plugin", componentName: plugin.Name ); } // Or when validating assembly dependencies if (assembly.GetName().Version?.ToString() != requiredAssemblyVersion) { throw new VersionMismatchException( message: $"Assembly version constraint violated", expectedVersion: requiredAssemblyVersion, actualVersion: assembly.GetName().Version?.ToString() ?? "unknown", componentType: "Assembly", componentName: assembly.GetName().Name ?? "unknown" ); } // Catch and handle the exception try { pluginEngine.LoadPlugin(pluginPath); } catch (VersionMismatchException ex) { Console.WriteLine($"Version mismatch error: {ex.Message}"); Console.WriteLine(ex.ToString()); Console.WriteLine($"Expected: {ex.ExpectedVersion}"); Console.WriteLine($"Actual: {ex.ActualVersion}"); Console.WriteLine($"Component: {ex.ComponentType} - {ex.ComponentName}"); } ``` ## PluginLoadException The `PluginLoadException` class represents an error that occurs during the loading of a plugin. It provides details about the plugin name, assembly path, and the specific stage at which the load failed. ### Usage Example ```csharp try { // Simulate a plugin load failure throw new PluginLoadException( message: "Failed to resolve plugin dependencies", pluginName: "MyPlugin", assemblyPath: "/plugins/MyPlugin.dll", stage: PluginLoadStage.DependencyResolution ); } catch (PluginLoadException ex) { Console.WriteLine($"Error loading plugin: {ex.Message}"); Console.WriteLine($"Plugin: {ex.PluginName}"); Console.WriteLine($"Path: {ex.AssemblyPath}"); Console.WriteLine($"Stage: {ex.LoadStage}"); Console.WriteLine(ex.ToString()); } ``` ## DependencyResolutionException The `DependencyResolutionException` represents an error that occurs during the resolution of plugin dependencies. It provides details about the required dependency plugin ID, version constraint, and the reason for the resolution failure. ### Usage Example ```csharp try { // Simulate a dependency resolution failure throw new DependencyResolutionException( message: "Failed to resolve dependency", dependencyPluginId: Guid.NewGuid(), versionConstraint: ">= 1.0.0", reason: DependencyResolutionReason.DependencyNotFound ); } catch (DependencyResolutionException ex) { Console.WriteLine($"Dependency resolution error: {ex.Message}"); Console.WriteLine($"Dependency ID: {ex.DependencyPluginId}"); Console.WriteLine($"Version constraint: {ex.VersionConstraint}"); Console.WriteLine($"Reason: {ex.Reason}"); Console.WriteLine($"Unresolved dependencies: {string.Join(", ", ex.UnresolvedDependencies)}"); Console.WriteLine(ex.ToString()); } ``` ## PluginEventPublisher

The `PluginEventPublisher` is responsible for routing plugin events to registered subscribers in a thread‑safe manner. It supports asynchronous handlers, keeps statistics on published events, and can clean up subscribers when a plugin’s `AssemblyLoadContext` is unloaded.

```csharp
using System;
using System.Threading.Tasks;
using PluginEngine.Events;

// Create the publisher (normally injected via DI)
var logger = /* obtain ILogger<PluginEventPublisher> */;
var publisher = new PluginEventPublisher(logger);

// Subscribe to a specific event type
publisher.Subscribe<PluginLoadedEvent>(async ev =>
{
    Console.WriteLine($"Plugin {ev.PluginName} loaded in {ev.LoadTimeMs}ms");
    await Task.CompletedTask;
});

// Publish an event
var loadedEvent = new PluginLoadedEvent
{
    PluginId = Guid.NewGuid(),
    PluginName = "DataProcessor",
    Version = "2.1.0",
    LoadTimeMs = 150
};
await publisher.PublishAsync(loadedEvent);

// Retrieve statistics
EventPublisherStatistics stats = publisher.GetStatistics();
Console.WriteLine($"Events published: {stats.EventsPublished}");
Console.WriteLine($"Registered subscribers: {stats.RegisteredSubscribers}");
Console.WriteLine($"Monitored event types: {stats.MonitoredEventTypes}");
Console.WriteLine($"Timestamp: {stats.Timestamp:u}");

// Unsubscribe when no longer needed
publisher.Unsubscribe<PluginLoadedEvent>(async ev => await Task.CompletedTask);

// Remove all subscribers belonging to a specific AssemblyLoadContext
var context = System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(typeof(PluginEventPublisher).Assembly);
publisher.RemoveSubscribersForContext(context);
```

The example demonstrates creating the publisher, subscribing to an event, publishing that event, inspecting statistics, and cleaning up subscribers. All operations use only the public members listed in the class definition.


## PluginEventSubscriber

The `PluginEventSubscriber` class provides a fluent API for registering and managing event handlers in the plugin engine. It maintains a registry of subscriptions and offers convenience methods for common plugin lifecycle events, while delegating the actual event routing to a `PluginEventPublisher`.



### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PluginEngine.Events;

// Setup (normally via dependency injection)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<PluginEventSubscriber>();
var publisher = new PluginEventPublisher(logger);
var subscriber = new PluginEventSubscriber(publisher, logger);

// Subscribe to plugin loaded events using the fluent API
subscriber.OnPluginLoaded(async ev =>
{
    Console.WriteLine($"Plugin loaded: {ev.PluginName} v{ev.Version}");
    await Task.CompletedTask;
});

// Subscribe to plugin unloaded events
subscriber.OnPluginUnloaded(async ev =>
{
    Console.WriteLine($"Plugin unloaded: {ev.PluginName}");
    await Task.CompletedTask;
});

// Subscribe to plugin error events
subscriber.OnPluginError(async ev =>
{
    Console.WriteLine($"Plugin error in {ev.PluginName}: {ev.ErrorMessage}");
    await Task.CompletedTask;
});

// Subscribe to dependencies resolved events
subscriber.OnDependenciesResolved(async ev =>
{
    Console.WriteLine($"Dependencies resolved for plugin: {ev.PluginName}");
    await Task.CompletedTask;
});

// Subscribe to plugin updated events
subscriber.OnPluginUpdated(async ev =>
{
    Console.WriteLine($"Plugin updated: {ev.PluginName} from v{ev.PreviousVersion} to v{ev.NewVersion}");
    await Task.CompletedTask;
});

// Get current subscription count
int subscriptionCount = subscriber.GetSubscriptionCount();
Console.WriteLine($"Active subscriptions: {subscriptionCount}");

// Unsubscribe a specific handler
subscriber.Unsubscribe<PluginLoadedEvent>(async ev => await Task.CompletedTask);

// Unsubscribe all handlers for a specific event type
subscriber.UnsubscribeAll<PluginUnloadedEvent>();

// Remove all subscriptions for a specific AssemblyLoadContext
var context = System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(
    typeof(PluginEventSubscriber).Assembly
);
subscriber.RemoveSubscribersForContext(context);
```