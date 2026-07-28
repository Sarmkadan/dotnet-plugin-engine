using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Metrics;

namespace Events
{
    /// <summary>
    /// Publishes plugin events to subscribers.
    /// </summary>
    public class PluginEventPublisher
    {
        private readonly object _lock = new();
        private readonly HashSet<PluginEventSubscriber> _subscribers = new HashSet<PluginEventSubscriber>();

        private readonly Meter _meter;
        private readonly Counter<long> _eventsPublishedCounter;
        private readonly Counter<long> _handlerFailureCounter;
        private readonly Gauge<int> _activeSubscriptionsGauge;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginEventPublisher"/> class.
        /// </summary>
        public PluginEventPublisher()
        {
            _meter = new Meter("PluginEventPublisher");
            _eventsPublishedCounter = _meter.CreateCounter<long>("plugin_event_published");
            _handlerFailureCounter = _meter.CreateCounter<long>("plugin_event_handler_failure");
            _activeSubscriptionsGauge = _meter.CreateGauge<int>("plugin_event_subscriptions_active");
        }

        /// <summary>
        /// Subscribes a plugin event subscriber to receive events.
        /// </summary>
        /// <param name="subscriber">The subscriber to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="subscriber"/> is <see langword="null"/>.</exception>
        public void Subscribe(PluginEventSubscriber subscriber)
        {
            ArgumentNullException.ThrowIfNull(subscriber);

            lock (_lock)
            {
                _subscribers.Add(subscriber);
            }

            _activeSubscriptionsGauge.Value = _subscribers.Count;
        }

        /// <summary>
        /// Unsubscribes a plugin event subscriber from receiving events.
        /// </summary>
        /// <param name="subscriber">The subscriber to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="subscriber"/> is <see langword="null"/>.</exception>
        public void Unsubscribe(PluginEventSubscriber subscriber)
        {
            ArgumentNullException.ThrowIfNull(subscriber);

            lock (_lock)
            {
                _subscribers.Remove(subscriber);
            }

            _activeSubscriptionsGauge.Value = _subscribers.Count;
        }

        /// <summary>
        /// Publishes a plugin event to all subscribers.
        /// </summary>
        /// <param name="pluginEvent">The event to publish.</param>
        /// <param name="memberName">The name of the method that invoked the publish.</param>
        /// <param name="maxRetries">The maximum number of retry attempts.</param>
        /// <param name="retryBackoff">The delay between retry attempts.</param>
        /// <param name="breakOnFailure">Whether to break (stop retrying) on the first failure.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pluginEvent"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an error occurs during event handling and <paramref name="breakOnFailure"/> is true or the maximum number of retries is exceeded.</exception>
        public async Task PublishAsync(PluginEvent pluginEvent, [CallerMemberName] string memberName = null, int maxRetries = 3, TimeSpan retryBackoff = default, bool breakOnFailure = true)
        {
            ArgumentNullException.ThrowIfNull(pluginEvent);

            if (retryBackoff == default)
            {
                retryBackoff = TimeSpan.FromSeconds(1);
            }

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    await Task.WhenAll(_subscribers.Select(subscriber => subscriber.HandleEvent(pluginEvent)));
                    _eventsPublishedCounter.Add(1);
                    return;
                }
                catch (Exception ex)
                {
                    _handlerFailureCounter.Add(1);
                    if (breakOnFailure || attempt == maxRetries - 1)
                    {
                        throw;
                    }

                    await Task.Delay(retryBackoff);
                }
            }
        }
    }
}