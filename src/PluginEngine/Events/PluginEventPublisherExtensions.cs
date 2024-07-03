#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Globalization;

namespace PluginEngine.Events;

/// <summary>
/// Extension methods for <see cref="PluginEventPublisher"/> that provide additional functionality
/// for event publishing, subscription management, and diagnostics.
/// </summary>
public static class PluginEventPublisherExtensions
{
    /// <summary>
    /// Publishes an event synchronously to all registered subscribers.
    /// </summary>
    /// <typeparam name="T">The type of plugin event to publish.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="event">The event to publish.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> or <paramref name="event"/> is null.</exception>
    public static void Publish<T>(this PluginEventPublisher publisher, T @event) where T : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(@event);

        // Offloaded to the thread pool so callers holding a synchronization context do not deadlock.
        Task.Run(() => publisher.PublishAsync(@event)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Publishes multiple events as a batch. This is more efficient than publishing events individually
    /// when you have multiple related events to publish.
    /// </summary>
    /// <typeparam name="T">The type of plugin event to publish.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="events">The collection of events to publish.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> or <paramref name="events"/> is null.</exception>
    public static async Task PublishBatchAsync<T>(this PluginEventPublisher publisher, IEnumerable<T> events) where T : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(events);

        var eventList = events.ToList();
        if (eventList.Count == 0)
        {
            return;
        }

        var tasks = new List<Task>(eventList.Count);
        foreach (var @event in eventList)
        {
            tasks.Add(publisher.PublishAsync(@event));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the current statistics from the publisher without locking.
    /// </summary>
    /// <param name="publisher">The event publisher instance.</param>
    /// <returns>A snapshot of the publisher's statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> is null.</exception>
    public static EventPublisherStatistics GetCurrentStatistics(this PluginEventPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        return publisher.GetStatistics();
    }

    /// <summary>
    /// Subscribes a synchronous handler to an event type.
    /// The handler will be invoked on the thread pool.
    /// </summary>
    /// <typeparam name="T">The type of plugin event to subscribe to.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="handler">The synchronous handler to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> or <paramref name="handler"/> is null.</exception>
    public static void Subscribe<T>(this PluginEventPublisher publisher, Action<T> handler) where T : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(handler);

        publisher.Subscribe<T>(e => Task.Run(() => handler(e)));
    }

    /// <summary>
    /// Unsubscribes all handlers of a specific type from the publisher.
    /// This removes ALL subscribers for the given event type, regardless of the handler instance.
    /// </summary>
    /// <typeparam name="T">The type of plugin event to unsubscribe from.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> is null.</exception>
    public static void UnsubscribeAll<T>(this PluginEventPublisher publisher) where T : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);

        // To unsubscribe all, we need to clear the handlers list for this event type
        // Since we don't have direct access to the internal dictionary, we'll use reflection
        // to clear the subscribers for this specific event type
        var eventType = typeof(T);
        var field = typeof(PluginEventPublisher).GetField("_subscribers",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(publisher) is Dictionary<Type, List<Delegate>> subscribers)
        {
            lock (subscribers)
            {
                subscribers.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// Gets a formatted string representation of the current publisher statistics.
    /// Useful for logging and diagnostics.
    /// </summary>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="formatProvider">The format provider to use (defaults to invariant culture).</param>
    /// <returns>A formatted statistics string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> is null.</exception>
    public static string GetStatisticsString(this PluginEventPublisher publisher, IFormatProvider? formatProvider = null)
    {
        ArgumentNullException.ThrowIfNull(publisher);

        formatProvider ??= CultureInfo.InvariantCulture;

        var stats = publisher.GetCurrentStatistics();
        return string.Create(formatProvider, null,
            $"PluginEventPublisher Stats [Timestamp: {stats.Timestamp:O}] | " +
            $"Events: {stats.EventsPublished:N0} | " +
            $"Subscribers: {stats.RegisteredSubscribers:N0} | " +
            $"Event Types: {stats.MonitoredEventTypes:N0}");
    }

    /// <summary>
    /// Creates a temporary subscription that automatically unsubscribes after the first event is received.
    /// Useful for one-time event handling scenarios.
    /// </summary>
    /// <typeparam name="T">The type of plugin event to subscribe to.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="handler">The handler to invoke for the first event.</param>
    /// <returns>A disposable that unsubscribes the handler when disposed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> or <paramref name="handler"/> is null.</exception>
    public static IDisposable SubscribeOnce<T>(this PluginEventPublisher publisher, Func<T, Task> handler) where T : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(handler);

        // Create a wrapper that unsubscribes after first invocation
        async Task WrappedHandler(T @event)
        {
            await handler(@event).ConfigureAwait(false);
            publisher.Unsubscribe<T>(WrappedHandler);
        }

        publisher.Subscribe<T>(WrappedHandler);

        return new SubscriptionHandle(() => publisher.Unsubscribe<T>(WrappedHandler));
    }

    /// <summary>
    /// Creates a temporary synchronous subscription that automatically unsubscribes after the first event is received.
    /// </summary>
    /// <typeparam name="T">The type of plugin event to subscribe to.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="handler">The synchronous handler to invoke for the first event.</param>
    /// <returns>A disposable that unsubscribes the handler when disposed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> or <paramref name="handler"/> is null.</exception>
    public static IDisposable SubscribeOnce<T>(this PluginEventPublisher publisher, Action<T> handler) where T : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(handler);

        return publisher.SubscribeOnce<T>(e => Task.Run(() => handler(e)));
    }

    /// <summary>
    /// Gets all currently monitored event types.
    /// </summary>
    /// <param name="publisher">The event publisher instance.</param>
    /// <returns>An enumerable of monitored event types.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> is null.</exception>
    public static IEnumerable<Type> GetMonitoredEventTypes(this PluginEventPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher);

        var field = typeof(PluginEventPublisher).GetField("_subscribers",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(publisher) is Dictionary<Type, List<Delegate>> subscribers)
        {
            lock (subscribers)
            {
                return subscribers.Keys.ToList().AsReadOnly();
            }
        }

        return Array.Empty<Type>();
    }

    /// <summary>
    /// Gets the count of subscribers for a specific event type.
    /// </summary>
    /// <typeparam name="T">The type of plugin event.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <returns>The number of registered subscribers, or 0 if no subscribers exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> is null.</exception>
    public static int GetSubscriberCount<T>(this PluginEventPublisher publisher) where T : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);

        var eventType = typeof(T);
        var field = typeof(PluginEventPublisher).GetField("_subscribers",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(publisher) is Dictionary<Type, List<Delegate>> subscribers)
        {
            lock (subscribers)
            {
                return subscribers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
            }
        }

        return 0;
    }

    private sealed class SubscriptionHandle : IDisposable
    {
        private readonly Action _unsubscribeAction;
        private int _disposed;

        public SubscriptionHandle(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction ?? throw new ArgumentNullException(nameof(unsubscribeAction));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _unsubscribeAction();
            }
        }
    }
}
