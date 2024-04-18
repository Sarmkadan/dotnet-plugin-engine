using System;
using System.Text.Json;

namespace PluginEngine.Tests
{
    public static class PluginDependencyResolverTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string ToJson(this PluginDependencyResolverTests value, bool indented = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (indented)
            {
                var indentedOptions = new JsonSerializerOptions(Options)
                {
                    WriteIndented = true
                };
                return JsonSerializer.Serialize(value, indentedOptions);
            }

            return JsonSerializer.Serialize(value, Options);
        }

        public static PluginDependencyResolverTests? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<PluginDependencyResolverTests>(json, Options);
        }

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
                return value != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
