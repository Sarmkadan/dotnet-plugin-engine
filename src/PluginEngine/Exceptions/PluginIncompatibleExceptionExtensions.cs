using System;

namespace PluginEngine.Exceptions
{
    public static class PluginIncompatibleExceptionExtensions
    {
        public static bool IsVersionIncompatible(this PluginIncompatibleException exception)
        {
            return string.IsNullOrEmpty(exception.HostEngineVersion) || string.IsNullOrEmpty(exception.DeclaredConstraint);
        }

        public static string GetIncompatibilityReason(this PluginIncompatibleException exception)
        {
            return $"Plugin {exception.DeclaredConstraint} is incompatible with host engine version {exception.HostEngineVersion}";
        }

        public static bool HasDeclaredConstraint(this PluginIncompatibleException exception)
        {
            return !string.IsNullOrEmpty(exception.DeclaredConstraint);
        }
    }
}
