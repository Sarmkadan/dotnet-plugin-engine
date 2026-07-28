using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Events
{
    public class PluginEventPublisher
    {
        private readonly object _lock = new object();
        private readonly HashSet<PluginEventSubscriber> _subscribers = new HashSet<PluginEventSubscriber>();

        public void Subscribe(PluginEventSubscriber subscriber)
        {
            lock (_lock)
            {
                _subscribers.Add(subscriber);
            }
        }

        public void Unsubscribe(PluginEventSubscriber subscriber)
        {
            lock (_lock)
            {
                _subscribers.Remove(subscriber);
            }
        }

        public async Task PublishAsync(PluginEvent pluginEvent, [CallerMemberName] string memberName = null, int maxRetries = 3, TimeSpan retryBackoff = default, bool breakOnFailure = true)
        {
            var subscribersSnapshot = _subscribers.ToList();
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    foreach (var subscriber in subscribersSnapshot)
                    {
                        try
                        {
                            subscriber.HandleEvent(pluginEvent);
                        }
                        catch (Exception ex)
                        {
                            // Handle exception
                        }
                    }
                }
            });
        }
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    foreach (var subscriber in _subscribers.ToList())
                    {
                        try
                        {
                            subscriber.HandleEvent(pluginEvent);
                        }
                        catch (Exception ex)
                        {
                            // Handle exception
                        }
                    }
                }
            });
        }
    }
}