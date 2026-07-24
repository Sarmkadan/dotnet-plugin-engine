#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Immutable;

namespace PluginEngine.Events;

/// <summary>
/// Event publisher for raising plugin events through the system.
/// Manages event routing to registered subscribers.
/// Thread-safe with support for async event handling.
/// </summary>
public sealed class PluginEventPublisher : IPluginEventPublisher
{
    private readonly ILogger<PluginEventPublisher> _logger;
    private readonly Dictionary<Type, List<Delegate>> _subscribers = [];
    private readonly object _subscribersLock = new();
    private long _eventsPublished;

    public PluginEventPublisher(ILogger<PluginEventPublisher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// </summary>
    /// <typeparam name="T">The type of event to publish.</typeparam>
    /// <param name="@event">The event to publish.</param>
    /// <exception cref="AggregateException">
    /// Thrown when one or more event handlers fail. The <see cref="AggregateException.InnerExceptions"/>
    /// property contains all exceptions thrown by individual handlers.
    /// </exception>
    /// <remarks>
    /// If a handler throws an exception, the publisher catches it and continues dispatching to remaining subscribers.
    /// All exceptions are collected and thrown as an <see cref="AggregateException"/> after all handlers have been invoked.
    /// </remarks>
    public async Task PublishAsync<T>(T @event) where T : IPluginEvent
    {
        try
        {
            lock (_subscribersLock)
            {
                _eventsPublished++;
            }

            _logger.LogInformation(
                "Publishing event: {EventType} for plugin {PluginId} [EventId: {EventId}]",
                @event.EventType, @event.PluginId, @event.EventId);

            var eventType = typeof(T);
            List<Delegate>? handlers;

            lock (_subscribersLock)
            {
                if (!_subscribers.TryGetValue(eventType, out handlers))
                {
                    _logger.LogDebug("No subscribers for event type: {EventType}", @event.EventType);
                    return;
                }

                handlers = new(handlers);
            }

            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            foreach (var handler in handlers)
            {
                if (handler is Func<T, Task> asyncHandler)
                {
                    try
                    {
                        tasks.Add(asyncHandler(@event));
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Handler registration threw exception for event: {EventType}", @event.EventType);
                        exceptions.Add(ex);
                    }
                }
            }

            if (tasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (ex is AggregateException aggregateException)
                    {
                        exceptions.AddRange(aggregateException.InnerExceptions);
                    }
                    else
                    {
                        exceptions.Add(ex);
                    }
                }

                if (exceptions.Count > 0)
                {
                    _logger.LogWarning(
                        "Event published with {HandlerCount} successful subscribers, but {FailedCount} failed: {EventType}",
                        tasks.Count - exceptions.Count,
                        exceptions.Count,
                        @event.EventType);

                    throw new AggregateException(
                        $"One or more event handlers failed for event type {typeof(T).Name}",
                        exceptions.ToImmutableArray());
                }
                else
                {
                    _logger.LogInformation(
                        "Event published to {HandlerCount} subscribers: {EventType}",
                        tasks.Count,
                        @event.EventType);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event: {EventType}", @event.EventType);
        }
    }

    /// <summary>
    /// Registers a subscriber for an event type.
    /// </summary>
    public void Subscribe<T>(Func<T, Task> handler) where T : IPluginEvent
    {
        lock (_subscribersLock)
        {
            var eventType = typeof(T);

            if (!_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers = [];
                _subscribers[eventType] = handlers;
            }

            handlers.Add(handler);
            _logger.LogDebug("Subscriber added for event type: {EventType}", typeof(T).Name);
        }
    }

    /// <summary>
    /// Unregisters a subscriber from an event type.
    /// </summary>
    public void Unsubscribe<T>(Func<T, Task> handler) where T : IPluginEvent
    {
        lock (_subscribersLock)
        {
            var eventType = typeof(T);

            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);

                if (handlers.Count == 0)
                {
                    _subscribers.Remove(eventType);
                }

                _logger.LogDebug("Subscriber removed for event type: {EventType}", typeof(T).Name);
            }
        }
    }

    /// <summary>
    /// Gets publisher statistics for monitoring.
    /// </summary>
    public EventPublisherStatistics GetStatistics()
    {
        lock (_subscribersLock)
        {
            var subscriberCount = _subscribers.Values.Sum(h => h.Count);

            return new EventPublisherStatistics
            {
                EventsPublished = _eventsPublished,
                RegisteredSubscribers = subscriberCount,
                MonitoredEventTypes = _subscribers.Count,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Removes all subscribers belonging to the specified AssemblyLoadContext.
    /// This prevents memory leaks when plugins are unloaded.
    /// </summary>
    public void RemoveSubscribersForContext(System.Runtime.Loader.AssemblyLoadContext context)
    {
        lock (_subscribersLock)
        {
            foreach (var key in _subscribers.Keys.ToList())
            {
                var handlers = _subscribers[key];
                int removed = handlers.RemoveAll(h =>
                {
                    var assembly = h.Method.DeclaringType?.Assembly;
                    return assembly != null && System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(assembly) == context;
                });

                if (removed > 0)
                {
                    _logger.LogDebug("Removed {Count} subscribers for context {ContextName} on event {EventType}",
                        removed, context.Name, key.Name);
                }

                if (handlers.Count == 0)
                {
                    _subscribers.Remove(key);
                }
            }
        }
    }
}

/// <summary>
/// Statistics for the event publisher.
/// </summary>
public sealed class EventPublisherStatistics
{
    public long EventsPublished { get; set; }
    public int RegisteredSubscribers { get; set; }
    public int MonitoredEventTypes { get; set; }
    public DateTime Timestamp { get; set; }
}