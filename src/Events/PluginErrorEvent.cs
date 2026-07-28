public class PluginErrorEvent : IPluginEvent
    {
        public string PluginId { get; set; }
        public string SanitizedMessage { get; set; }
        public string ExceptionType { get; set; }
    }