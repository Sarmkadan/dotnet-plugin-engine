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

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginEventSubscriber"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> or <paramref name="logger"/> is <see langword="null"/>.</exception>
    public PluginEventSubscriber(PluginEventPublisher publisher, ILogger<PluginEventSubscriber> logger)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(logger);

        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Subscribes a handler to events of the specified type.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    public void Subscribe<T>(Func<T, Task> handler) where T : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

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

    /// <summary>
    /// Unsubscribes a handler from events of the specified type.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    public void Unsubscribe<T>(Func<T, Task> handler) where T : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    public void OnPluginLoaded(Func<PluginLoadedEvent, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to plugin unloaded events.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    public void OnPluginUnloaded(Func<PluginUnloadedEvent, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to plugin updated events.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    public void OnPluginUpdated(Func<PluginUpdatedEvent, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to plugin error events.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    public void OnPluginError(Func<PluginErrorEvent, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to dependencies resolved events.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    public void OnDependenciesResolved(Func<DependenciesResolvedEvent, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

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
        List<Delegate> handlers;

        lock (_subscriptionsLock)
        {
            var eventType = typeof(T);

            if (!_subscriptions.TryGetValue(eventType, out var existing))
            {
                return;
            }

            handlers = new List<Delegate>(existing);
            _subscriptions.Remove(eventType);
        }

        foreach (var handler in handlers)
        {
            if (handler is Func<T, Task> typedHandler)
            {
                _publisher.Unsubscribe(typedHandler);
            }
        }

        _logger.LogInformation("Unsubscribed all handlers from event: {EventType}", typeof(T).Name);
    }

    /// <summary>
    /// Removes all subscriptions belonging to the specified AssemblyLoadContext.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    public void RemoveSubscribersForContext(System.Runtime.Loader.AssemblyLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

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
