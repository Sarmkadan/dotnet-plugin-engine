using System;

namespace PluginEngine.Utils.Helpers
{
    /// <summary>
    /// Provides extension methods for <see cref="VersionHelper"/> to simplify version comparison operations.
    /// </summary>
    public static class VersionHelperExtensions
    {
        /// <summary>
        /// Determines whether the specified version string is greater than the target version.
        /// </summary>
        /// <param name="helper">The version helper instance.</param>
        /// <param name="versionString">The version string to compare.</param>
        /// <param name="targetVersion">The target version to compare against.</param>
        /// <returns>True if versionString is greater than targetVersion; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="helper"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="versionString"/> or <paramref name="targetVersion"/> is null or whitespace.</exception>
        public static bool IsGreaterThan(this VersionHelper helper, string versionString, string targetVersion)
        {
            ArgumentNullException.ThrowIfNull(helper);
            ArgumentException.ThrowIfNullOrEmpty(versionString);
            ArgumentException.ThrowIfNullOrEmpty(targetVersion);

            return helper.CompareVersions(versionString, targetVersion) > 0;
        }

        /// <summary>
        /// Determines whether the specified version string is less than the target version.
        /// </summary>
        /// <param name="helper">The version helper instance.</param>
        /// <param name="versionString">The version string to compare.</param>
        /// <param name="targetVersion">The target version to compare against.</param>
        /// <returns>True if versionString is less than targetVersion; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="helper"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="versionString"/> or <paramref name="targetVersion"/> is null or whitespace.</exception>
        public static bool IsLessThan(this VersionHelper helper, string versionString, string targetVersion)
        {
            ArgumentNullException.ThrowIfNull(helper);
            ArgumentException.ThrowIfNullOrEmpty(versionString);
            ArgumentException.ThrowIfNullOrEmpty(targetVersion);

            return helper.CompareVersions(versionString, targetVersion) < 0;
        }

        /// <summary>
        /// Determines whether the specified version string is equal to the target version.
        /// </summary>
        /// <param name="helper">The version helper instance.</param>
        /// <param name="versionString">The version string to compare.</param>
        /// <param name="targetVersion">The target version to compare against.</param>
        /// <returns>True if versionString is equal to targetVersion; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="helper"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="versionString"/> or <paramref name="targetVersion"/> is null or whitespace.</exception>
        public static bool IsEqualTo(this VersionHelper helper, string versionString, string targetVersion)
        {
            ArgumentNullException.ThrowIfNull(helper);
            ArgumentException.ThrowIfNullOrEmpty(versionString);
            ArgumentException.ThrowIfNullOrEmpty(targetVersion);

            return helper.CompareVersions(versionString, targetVersion) == 0;
        }

        /// <summary>
        /// Gets the version information for a version string.
        /// </summary>
        /// <param name="helper">The version helper instance.</param>
        /// <param name="versionString">The version string to parse.</param>
        /// <returns>ParsedVersionInfo containing major, minor, patch, and stability information.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="helper"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="versionString"/> is null or whitespace.</exception>
        public static ParsedVersionInfo GetVersionInfo(this VersionHelper helper, string versionString)
        {
            ArgumentNullException.ThrowIfNull(helper);
            ArgumentException.ThrowIfNullOrEmpty(versionString);

            return helper.GetVersionInfo(versionString);
        }

        /// <summary>
        /// Determines whether the specified version string is a valid semantic version.
        /// </summary>
        /// <param name="helper">The version helper instance.</param>
        /// <param name="versionString">The version string to validate.</param>
        /// <returns>True if the version string is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="helper"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="versionString"/> is null or whitespace.</exception>
        public static bool IsValidSemanticVersion(this VersionHelper helper, string versionString)
        {
            ArgumentNullException.ThrowIfNull(helper);
            ArgumentException.ThrowIfNullOrEmpty(versionString);

            return helper.IsValidSemanticVersion(versionString);
        }
    }
}