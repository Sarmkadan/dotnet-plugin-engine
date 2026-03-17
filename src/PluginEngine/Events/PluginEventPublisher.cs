// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Events;

/// <summary>
/// Event publisher for raising plugin events through the system.
/// Manages event routing to registered subscribers.
/// Thread-safe with support for async event handling.
/// </summary>
public class PluginEventPublisher : IPluginEventPublisher
{
    private readonly ILogger<PluginEventPublisher> _logger;
    private readonly Dictionary<Type, List<Delegate>> _subscribers = [];
    private readonly object _subscribersLock = new();
    private long _eventsPublished;

    public PluginEventPublisher(ILogger<PluginEventPublisher> logger)
    {
        _logger = logger;
    }

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

            foreach (var handler in handlers)
            {
                if (handler is Func<T, Task> asyncHandler)
                {
                    tasks.Add(asyncHandler(@event));
                }
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
                _logger.LogInformation(
                    "Event published to {HandlerCount} subscribers: {EventType}",
                    tasks.Count, @event.EventType);
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
}

/// <summary>
/// Statistics for the event publisher.
/// </summary>
public class EventPublisherStatistics
{
    public long EventsPublished { get; set; }
    public int RegisteredSubscribers { get; set; }
    public int MonitoredEventTypes { get; set; }
    public DateTime Timestamp { get; set; }
}
