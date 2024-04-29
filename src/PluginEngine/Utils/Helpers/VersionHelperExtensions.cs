using System;

namespace PluginEngine.Utils.Helpers
{
    public static class VersionHelperExtensions
    {
        /// <summary>
        /// Determines whether the specified version string is greater than the target version.
        /// </summary>
        /// <param name="helper">The version helper instance</param>
        /// <param name="versionString">The version string to compare</param>
        /// <param name="targetVersion">The target version to compare against</param>
        /// <returns>True if versionString is greater than targetVersion</returns>
        public static bool IsGreaterThan(this VersionHelper helper, string versionString, string targetVersion)
        {
            if (helper == null)
                throw new ArgumentNullException(nameof(helper));

            if (string.IsNullOrWhiteSpace(versionString))
                throw new ArgumentException("Version string cannot be null or whitespace", nameof(versionString));

            if (string.IsNullOrWhiteSpace(targetVersion))
                throw new ArgumentException("Target version cannot be null or whitespace", nameof(targetVersion));

            return helper.CompareVersions(versionString, targetVersion) > 0;
        }

        /// <summary>
        /// Determines whether the specified version string is less than the target version.
        /// </summary>
        /// <param name="helper">The version helper instance</param>
        /// <param name="versionString">The version string to compare</param>
        /// <param name="targetVersion">The target version to compare against</param>
        /// <returns>True if versionString is less than targetVersion</returns>
        public static bool IsLessThan(this VersionHelper helper, string versionString, string targetVersion)
        {
            if (helper == null)
                throw new ArgumentNullException(nameof(helper));

            if (string.IsNullOrWhiteSpace(versionString))
                throw new ArgumentException("Version string cannot be null or whitespace", nameof(versionString));

            if (string.IsNullOrWhiteSpace(targetVersion))
                throw new ArgumentException("Target version cannot be null or whitespace", nameof(targetVersion));

            return helper.CompareVersions(versionString, targetVersion) < 0;
        }

        /// <summary>
        /// Determines whether the specified version string is equal to the target version.
        /// </summary>
        /// <param name="helper">The version helper instance</param>
        /// <param name="versionString">The version string to compare</param>
        /// <param name="targetVersion">The target version to compare against</param>
        /// <returns>True if versionString is equal to targetVersion</returns>
        public static bool IsEqualTo(this VersionHelper helper, string versionString, string targetVersion)
        {
            if (helper == null)
                throw new ArgumentNullException(nameof(helper));

            if (string.IsNullOrWhiteSpace(versionString))
                throw new ArgumentException("Version string cannot be null or whitespace", nameof(versionString));

            if (string.IsNullOrWhiteSpace(targetVersion))
                throw new ArgumentException("Target version cannot be null or whitespace", nameof(targetVersion));

            return helper.CompareVersions(versionString, targetVersion) == 0;
        }

        /// <summary>
        /// Gets the version information for a version string.
        /// </summary>
        /// <param name="helper">The version helper instance</param>
        /// <param name="versionString">The version string to parse</param>
        /// <returns>ParsedVersionInfo containing major, minor, patch, and stability information</returns>
        public static ParsedVersionInfo GetVersionInfo(this VersionHelper helper, string versionString)
        {
            if (helper == null)
                throw new ArgumentNullException(nameof(helper));

            if (string.IsNullOrWhiteSpace(versionString))
                throw new ArgumentException("Version string cannot be null or whitespace", nameof(versionString));

            return helper.GetVersionInfo(versionString);
        }

        /// <summary>
        /// Determines whether the specified version string is a valid semantic version.
        /// </summary>
        /// <param name="helper">The version helper instance</param>
        /// <param name="versionString">The version string to validate</param>
        /// <returns>True if the version string is valid</returns>
        public static bool IsValidSemanticVersion(this VersionHelper helper, string versionString)
        {
            if (helper == null)
                throw new ArgumentNullException(nameof(helper));

            if (string.IsNullOrWhiteSpace(versionString))
                return false;

            return helper.IsValidSemanticVersion(versionString);
        }
    }
}