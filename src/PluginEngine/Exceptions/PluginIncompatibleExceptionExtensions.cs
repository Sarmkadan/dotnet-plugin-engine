using System;

namespace PluginEngine.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="PluginIncompatibleException"/> to analyze and describe version incompatibility issues.
    /// </summary>
    public static class PluginIncompatibleExceptionExtensions
    {
        /// <summary>
        /// Determines whether the plugin version is incompatible with the host engine version.
        /// </summary>
        /// <param name="exception">The exception containing version compatibility information.</param>
        /// <returns><see langword="true"/> if either the host engine version or declared constraint is missing; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static bool IsVersionIncompatible(this PluginIncompatibleException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return string.IsNullOrEmpty(exception.HostEngineVersion) || string.IsNullOrEmpty(exception.DeclaredConstraint);
        }

        /// <summary>
        /// Gets a human-readable description of the incompatibility reason.
        /// </summary>
        /// <param name="exception">The exception containing version compatibility information.</param>
        /// <returns>A formatted string describing the incompatibility.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static string GetIncompatibilityReason(this PluginIncompatibleException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return $"Plugin {exception.DeclaredConstraint} is incompatible with host engine version {exception.HostEngineVersion}";
        }

        /// <summary>
        /// Determines whether the exception has a declared version constraint.
        /// </summary>
        /// <param name="exception">The exception containing version compatibility information.</param>
        /// <returns><see langword="true"/> if a constraint is present; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static bool HasDeclaredConstraint(this PluginIncompatibleException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return !string.IsNullOrEmpty(exception.DeclaredConstraint);
        }
    }
}