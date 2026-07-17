using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PluginEngine.Domain.Entities
{
    /// <summary>
    /// Extension methods for <see cref="PluginDependency"/>.
    /// </summary>
    public static class PluginDependencyExtensions
    {
        /// <summary>
        /// Determines whether the supplied <paramref name="version"/> satisfies the version
        /// constraints defined by the <see cref="PluginDependency"/>.
        /// </summary>
        /// <param name="dependency">The dependency to evaluate.</param>
        /// <param name="version">The version string to test.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="version"/> is within the inclusive range
        /// defined by <see cref="PluginDependency.MinimumVersion"/> and
        /// <see cref="PluginDependency.MaximumVersion"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dependency"/> or <paramref name="version"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="version"/> is an empty string or cannot be parsed as a <see cref="Version"/>.
        /// </exception>
        public static bool IsVersionSatisfied(this PluginDependency dependency, string version)
        {
            ArgumentNullException.ThrowIfNull(dependency);
            ArgumentException.ThrowIfNullOrEmpty(version);

            if (!Version.TryParse(version, out var targetVersion))
                throw new ArgumentException($"'{version}' is not a valid version string.", nameof(version));

            // Minimum bound
            if (!string.IsNullOrWhiteSpace(dependency.MinimumVersion) &&
                Version.TryParse(dependency.MinimumVersion, out var minVersion) &&
                targetVersion < minVersion)
            {
                return false;
            }

            // Maximum bound
            if (!string.IsNullOrWhiteSpace(dependency.MaximumVersion) &&
                Version.TryParse(dependency.MaximumVersion, out var maxVersion) &&
                targetVersion > maxVersion)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the version range of <paramref name="first"/> overlaps with the
        /// version range of <paramref name="second"/>.
        /// </summary>
        /// <param name="first">The first dependency.</param>
        /// <param name="second">The second dependency.</param>
        /// <returns>
        /// <c>true</c> if the two dependencies have at least one version that satisfies both
        /// constraints; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="first"/> or <paramref name="second"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown when either dependency has an invalid version string that cannot be parsed.
        /// </exception>
        public static bool OverlapsWith(this PluginDependency first, PluginDependency second)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);

            // If they refer to different plugins, overlapping versions are irrelevant.
            if (first.DependencyPluginId != second.DependencyPluginId)
                return false;

            // Resolve bounds; null or empty means unbounded.
            if (!Version.TryParse(first.MinimumVersion, out var firstMin))
                throw new FormatException($"Invalid minimum version format: '{first.MinimumVersion}'");

            Version? firstMax = null;
            if (!string.IsNullOrWhiteSpace(first.MaximumVersion) && !Version.TryParse(first.MaximumVersion, out firstMax))
                throw new FormatException($"Invalid maximum version format: '{first.MaximumVersion}'");

            if (!Version.TryParse(second.MinimumVersion, out var secondMin))
                throw new FormatException($"Invalid minimum version format: '{second.MinimumVersion}'");

            Version? secondMax = null;
            if (!string.IsNullOrWhiteSpace(second.MaximumVersion) && !Version.TryParse(second.MaximumVersion, out secondMax))
                throw new FormatException($"Invalid maximum version format: '{second.MaximumVersion}'");

            // The latest lower bound must be less than or equal to the earliest upper bound.
            var effectiveMin = Max(firstMin, secondMin);
            var effectiveMax = Min(firstMax, secondMax);

            // If either side is unbounded, the comparison is trivially true.
            if (effectiveMin is null || effectiveMax is null)
                return true;

            return effectiveMin <= effectiveMax;
        }

        /// <summary>
        /// Returns a concise, human‑readable summary of the dependency.
        /// </summary>
        /// <param name="dependency">The dependency to summarise.</param>
        /// <returns>A string containing the plugin identifier, version constraints,
        /// optional flag and description.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dependency"/> is <c>null</c>.
        /// </exception>
        public static string ToSummary(this PluginDependency dependency)
        {
            ArgumentNullException.ThrowIfNull(dependency);

            return $"{dependency.DependencyPluginId} " +
                   $"[{(string.IsNullOrWhiteSpace(dependency.MinimumVersion) ? "any" : dependency.MinimumVersion)} - " +
                   $"{(string.IsNullOrWhiteSpace(dependency.MaximumVersion) ? "any" : dependency.MaximumVersion)}] " +
                   $"{(dependency.IsOptional ? "Optional" : "Required")} - {dependency.Description}";
        }

        /// <summary>
        /// Returns the greater of two nullable Version values, treating null as unbounded
        /// (i.e., greater than any concrete version).
        /// </summary>
        /// <param name="a">The first version to compare.</param>
        /// <param name="b">The second version to compare.</param>
        /// <returns>The greater version, or null if both are null.</returns>
        private static Version? Max(Version? a, Version? b)
        {
            return a switch
            {
                null => b,
                _ when b is null => a,
                _ => a > b ? a : b
            };
        }

        /// <summary>
        /// Returns the lesser of two nullable Version values, treating null as unbounded
        /// (i.e., less than any concrete version).
        /// </summary>
        /// <param name="a">The first version to compare.</param>
        /// <param name="b">The second version to compare.</param>
        /// <returns>The lesser version, or null if both are null.</returns>
        private static Version? Min(Version? a, Version? b)
        {
            return a switch
            {
                null => b,
                _ when b is null => a,
                _ => a < b ? a : b
            };
        }
    }
}