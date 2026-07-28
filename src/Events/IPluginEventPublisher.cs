using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PluginEngine.Events
{
    public interface IPluginEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : IPluginEvent;
        void Subscribe<T>(Func<T, Task> handler) where T : IPluginEvent;
        void Unsubscribe<T>(Func<T, Task> handler) where T : IPluginEvent;
    }
}
