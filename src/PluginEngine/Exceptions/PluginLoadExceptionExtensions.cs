using System;

namespace PluginEngine.Exceptions
{
    public static class PluginLoadExceptionExtensions
    {
        public static bool IsLoadStage(this PluginLoadException exception, PluginLoadStage stage)
        {
            return exception.LoadStage == stage;
        }

        public static string GetLoadFailureSummary(this PluginLoadException exception)
        {
            return $"Failed to load plugin '{exception.PluginName}' from '{exception.AssemblyPath}' at stage {exception.LoadStage}.";
        }

        public static PluginLoadException WithLoadStage(this PluginLoadException exception, PluginLoadStage stage)
        {
            // Creating a new exception to avoid changing the original exception's state
            return new PluginLoadException(exception.Message)
            {
                PluginName = exception.PluginName,
                AssemblyPath = exception.AssemblyPath,
                LoadStage = stage
            };
        }
    }
}
