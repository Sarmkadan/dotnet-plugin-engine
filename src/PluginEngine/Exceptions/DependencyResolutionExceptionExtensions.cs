using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginEngine.Exceptions
{
    /// <summary>
    /// Extension methods that make working with <see cref="DependencyResolutionException"/> more convenient.
    /// </summary>
    public static class DependencyResolutionExceptionExtensions
    {
        /// <summary>
        /// Sets the <see cref="DependencyResolutionException.DependencyPluginId"/> and returns the same exception
        /// instance to allow fluent chaining.
        /// </summary>
        public static DependencyResolutionException WithDependencyPluginId(
            this DependencyResolutionException exception,
            Guid pluginId)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            exception.DependencyPluginId = pluginId;
            return exception;
        }

        /// <summary>
        /// Sets the <see cref="DependencyResolutionException.VersionConstraint"/> and returns the same exception
        /// instance to allow fluent chaining.
        /// </summary>
        public static DependencyResolutionException WithVersionConstraint(
            this DependencyResolutionException exception,
            string versionConstraint)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            exception.VersionConstraint = versionConstraint ?? throw new ArgumentNullException(nameof(versionConstraint));
            return exception;
        }

        /// <summary>
        /// Adds multiple unresolved dependency identifiers to the exception.
        /// </summary>
        public static DependencyResolutionException AddUnresolvedDependencies(
            this DependencyResolutionException exception,
            IEnumerable<string> dependencies)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            if (dependencies == null) throw new ArgumentNullException(nameof(dependencies));

            foreach (var dep in dependencies)
            {
                // Use the existing AddUnresolvedDependency method to keep any internal logic consistent.
                exception.AddUnresolvedDependency(dep);
            }

            return exception;
        }

        /// <summary>
        /// Returns a single string that lists all unresolved dependencies, separated by commas.
        /// If there are none, returns an empty string.
        /// </summary>
        public static string GetUnresolvedDependenciesSummary(this DependencyResolutionException exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return exception.UnresolvedDependencies != null && exception.UnresolvedDependencies.Any()
                ? string.Join(", ", exception.UnresolvedDependencies)
                : string.Empty;
        }
    }
}
