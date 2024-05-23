using System;
using System.Collections.Generic;
using System.Globalization;

namespace PluginEngine.Domain.Entities
{
    /// <summary>
    /// Provides extension methods for the PluginCapability class.
    /// </summary>
    public static class PluginCapabilityExtensions
    {
        /// <summary>
        /// Gets a display name that includes the capability name and version.
        /// </summary>
        /// <param name="capability">The plugin capability.</param>
        /// <returns>A formatted string combining display name and version.</returns>
        /// <exception cref="ArgumentNullException">Thrown when capability is null.</exception>
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
        /// <exception cref="ArgumentNullException">Thrown when capability or requiredTags is null.</exception>
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
        /// <exception cref="ArgumentNullException">Thrown when capability or other is null.</exception>
        public static bool IsInterfaceCompatible(this PluginCapability capability, PluginCapability other)
        {
            ArgumentNullException.ThrowIfNull(capability);
            ArgumentNullException.ThrowIfNull(other);
            
            return capability.InterfaceTypeName == other.InterfaceTypeName;
        }
    }
}
