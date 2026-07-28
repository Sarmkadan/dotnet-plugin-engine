public sealed class DependencyResolutionException : PluginException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolutionException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DependencyResolutionException(string message) : base(message)
        {
        }
    }

public sealed class PluginLoadException : PluginException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoadException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PluginLoadException(string message) : base(message)
        {
        }
    }
