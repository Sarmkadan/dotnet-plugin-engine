using System;
using System.Text.Json;

namespace PluginEngine.Tests
{
    /// <summary>
    /// Provides JSON serialization and deserialization helper methods for <see cref="PluginDependencyResolverTests"/> test data.
    /// Uses camelCase property naming and Web defaults for serialization.
    /// </summary>
    public static class PluginDependencyResolverTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the specified test data to a JSON string.
        /// </summary>
        /// <param name="value">The test data to serialize.</param>
        /// <param name="indented">When true, formats JSON with indentation for readability.</param>
        /// <returns>JSON string representation of the test data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this PluginDependencyResolverTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(Options) { WriteIndented = true }
                : Options;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes JSON data into a <see cref="PluginDependencyResolverTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>
        /// The deserialized test data, or null if the input is null or whitespace.
        /// </returns>
        public static PluginDependencyResolverTests? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<PluginDependencyResolverTests>(json, Options);
        }

        /// <summary>
        /// Attempts to deserialize JSON data into a <see cref="PluginDependencyResolverTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized test data if successful; otherwise, null.</param>
        /// <returns>
        /// True if deserialization succeeded; otherwise, false.
        /// </returns>
        public static bool TryFromJson(string json, out PluginDependencyResolverTests? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<PluginDependencyResolverTests>(json, Options);
                return value is not null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
