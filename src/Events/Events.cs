using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events
{
    public sealed class PluginLoadingEvent : PluginEventBase
    {
        public PluginLoadingEvent() : base(PluginEventType.PluginLoading) { }
    }

    public sealed class PluginUnloadingEvent : PluginEventBase
    {
        public PluginUnloadingEvent() : base(PluginEventType.PluginUnloading) { }
    }

    public sealed class DependencyResolutionFailedEvent : PluginEventBase
    {
        public DependencyResolutionFailedEvent() : base(PluginEventType.DependencyResolutionFailed) { }
    }
}