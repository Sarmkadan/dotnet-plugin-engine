using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedRocket.Plugin
{
    public class PluginErrorEvent : PluginEventBase
    {
        public PluginErrorEvent(Exception exception) : base(exception)
        {
        }

    {
        public string SanitizedMessage { get; private set; }
        public int ErrorCode { get; private set; }
        public string GetFullExceptionDetails()
        {
            // TO DO: implement a diagnostic-only accessor that isn't broadcast to arbitrary subscribers
        }
    }
}