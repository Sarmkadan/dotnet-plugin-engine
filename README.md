# dotnet-plugin-engine

[... existing content ...]

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