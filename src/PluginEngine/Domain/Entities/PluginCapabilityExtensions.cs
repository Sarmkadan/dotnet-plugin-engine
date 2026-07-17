using System;
using System.Collections.Generic;

namespace PluginEngine.Domain.Entities
{
    /// <summary>
    /// Provides extension methods for the <see cref="PluginCapability"/> class.
    /// </summary>
    public static class PluginCapabilityExtensions
    {
        /// <summary>
        /// Gets a display name that includes the capability name and version.
        /// </summary>
        /// <param name="capability">The plugin capability.</param>
        /// <returns>A formatted string combining display name and version.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="capability"/> is null.</exception>
        public static string GetDisplayNameWithVersion(this PluginCapability capability)
        {
            ArgumentNullException.ThrowIfNull(capability);
            return $"{capability.GetDisplayName()} v{capability.Version}";
        }

        /// <summary>
        /// Determines whether this capability has all the specified tags.
        /// </summary>
        /// <param name="capability">The plugin capability.</param>
        /// <param name="requiredTags">The tags that must be present.</param>
        /// <returns>true if all required tags are present; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="capability"/> or <paramref name="requiredTags"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any tag in <paramref name="requiredTags"/> is null or empty.</exception>
        public static bool HasAllTags(this PluginCapability capability, IEnumerable<string> requiredTags)
        {
            ArgumentNullException.ThrowIfNull(capability);
            ArgumentNullException.ThrowIfNull(requiredTags);

            foreach (var tag in requiredTags)
            {
                if (string.IsNullOrEmpty(tag))
                {
                    throw new ArgumentException("Tag cannot be null or empty.", nameof(requiredTags));
                }

                if (!capability.HasTag(tag))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether this capability's interface type is compatible with another capability.
        /// </summary>
        /// <param name="capability">The plugin capability.</param>
        /// <param name="other">The capability to compare with.</param>
        /// <returns>true if interface types match; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="capability"/> or <paramref name="other"/> is null.</exception>
        public static bool IsInterfaceCompatible(this PluginCapability capability, PluginCapability other)
        {
            ArgumentNullException.ThrowIfNull(capability);
            ArgumentNullException.ThrowIfNull(other);
            return capability.InterfaceTypeName == other.InterfaceTypeName;
        }
    }
}
