using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginEngine.Exceptions
{
    /// <summary>
    /// Provides extension methods that make working with <see cref="DependencyResolutionException"/> more convenient.
    /// </summary>
    public static class DependencyResolutionExceptionExtensions
    {
        /// <summary>
        /// Sets the <see cref="DependencyResolutionException.DependencyPluginId"/> property and returns the same exception
        /// instance to allow fluent method chaining.
        /// </summary>
        /// <param name="exception">The exception instance to modify.</param>
        /// <param name="pluginId">The plugin identifier to set as the dependency plugin ID.</param>
        /// <returns>The modified exception instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
        public static DependencyResolutionException WithDependencyPluginId(
            this DependencyResolutionException exception,
            Guid pluginId)
        {
            ArgumentNullException.ThrowIfNull(exception);
            exception.DependencyPluginId = pluginId;
            return exception;
        }

        /// <summary>
        /// Sets the <see cref="DependencyResolutionException.VersionConstraint"/> property and returns the same exception
        /// instance to allow fluent method chaining.
        /// </summary>
        /// <param name="exception">The exception instance to modify.</param>
        /// <param name="versionConstraint">The version constraint string to set.</param>
        /// <returns>The modified exception instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="versionConstraint"/> is <see langword="null"/>.</exception>
        public static DependencyResolutionException WithVersionConstraint(
            this DependencyResolutionException exception,
            string versionConstraint)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(versionConstraint);
            exception.VersionConstraint = versionConstraint;
            return exception;
        }

        /// <summary>
        /// Adds multiple unresolved dependency identifiers to the exception by delegating to
        /// <see cref="DependencyResolutionException.AddUnresolvedDependency(string)"/> for each item.
        /// </summary>
        /// <param name="exception">The exception instance to modify.</param>
        /// <param name="dependencies">The collection of dependency names to add.</param>
        /// <returns>The modified exception instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dependencies"/> is <see langword="null"/>.</exception>
        public static DependencyResolutionException AddUnresolvedDependencies(
            this DependencyResolutionException exception,
            IEnumerable<string> dependencies)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(dependencies);

            foreach (var dep in dependencies)
            {
                // Use the existing AddUnresolvedDependency method to keep any internal logic consistent.
                exception.AddUnresolvedDependency(dep);
            }

            return exception;
        }

        /// <summary>
        /// Gets a comma-separated string containing all unresolved dependency names.
        /// </summary>
        /// <param name="exception">The exception instance containing unresolved dependencies.</param>
        /// <returns>A comma-separated string of unresolved dependencies, or an empty string if none exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
        public static string GetUnresolvedDependenciesSummary(this DependencyResolutionException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception.UnresolvedDependencies?.Count > 0
                ? string.Join(", ", exception.UnresolvedDependencies)
                : string.Empty;
        }
    }
}
