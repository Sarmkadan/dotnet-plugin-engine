#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Events;

/// <summary>
/// Event subscriber for registering and managing event handlers.
/// Provides a fluent API for subscribing to specific event types.
/// </summary>
public sealed class PluginEventSubscriber : IPluginEventSubscriber
{
    private readonly PluginEventPublisher _publisher;
    private readonly ILogger<PluginEventSubscriber> _logger;
    private readonly Dictionary<Type, List<Delegate>> _subscriptions = [];
    private readonly object _subscriptionsLock = new();

    public PluginEventSubscriber(PluginEventPublisher publisher, ILogger<PluginEventSubscriber> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public void Subscribe<T>(Func<T, Task> handler) where T : IPluginEvent
    {
        _publisher.Subscribe(handler);

        lock (_subscriptionsLock)
        {
            var eventType = typeof(T);

            if (!_subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers = [];
                _subscriptions[eventType] = handlers;
            }

            handlers.Add(handler);
        }

        _logger.LogInformation("Subscribed to event: {EventType}", typeof(T).Name);
    }

    public void Unsubscribe<T>(Func<T, Task> handler) where T : IPluginEvent
    {
        _publisher.Unsubscribe(handler);

        lock (_subscriptionsLock)
        {
            var eventType = typeof(T);

            if (_subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);

                if (handlers.Count == 0)
                {
                    _subscriptions.Remove(eventType);
                }
            }
        }

        _logger.LogInformation("Unsubscribed from event: {EventType}", typeof(T).Name);
    }

    /// <summary>
    /// Subscribes to plugin loaded events.
    /// </summary>
    public void OnPluginLoaded(Func<PluginLoadedEvent, Task> handler)
    {
        Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to plugin unloaded events.
    /// </summary>
    public void OnPluginUnloaded(Func<PluginUnloadedEvent, Task> handler)
    {
        Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to plugin updated events.
    /// </summary>
    public void OnPluginUpdated(Func<PluginUpdatedEvent, Task> handler)
    {
        Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to plugin error events.
    /// </summary>
    public void OnPluginError(Func<PluginErrorEvent, Task> handler)
    {
        Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to dependencies resolved events.
    /// </summary>
    public void OnDependenciesResolved(Func<DependenciesResolvedEvent, Task> handler)
    {
        Subscribe(handler);
    }

    /// <summary>
    /// Gets the number of active subscriptions.
    /// </summary>
    public int GetSubscriptionCount()
    {
        lock (_subscriptionsLock)
        {
            return _subscriptions.Values.Sum(h => h.Count);
        }
    }

    /// <summary>
    /// Unsubscribes all handlers for a specific event type.
    /// </summary>
    public void UnsubscribeAll<T>() where T : IPluginEvent
    {
        lock (_subscriptionsLock)
        {
            var eventType = typeof(T);
            _subscriptions.Remove(eventType);
        }

        _logger.LogInformation("Unsubscribed all handlers from event: {EventType}", typeof(T).Name);
    }

    /// <summary>
    /// Removes all subscriptions belonging to the specified AssemblyLoadContext.
    /// </summary>
    public void RemoveSubscribersForContext(System.Runtime.Loader.AssemblyLoadContext context)
    {
        lock (_subscriptionsLock)
        {
            foreach (var key in _subscriptions.Keys.ToList())
            {
                var handlers = _subscriptions[key];
                handlers.RemoveAll(h => 
                {
                    var assembly = h.Method.DeclaringType?.Assembly;
                    return assembly != null && System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(assembly) == context;
                });
                
                if (handlers.Count == 0)
                {
                    _subscriptions.Remove(key);
                }
            }
        }
    }
}
