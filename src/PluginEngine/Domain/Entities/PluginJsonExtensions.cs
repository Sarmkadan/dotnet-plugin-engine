#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Provides JSON serialization and deserialization extensions for the Plugin type.
/// </summary>
public static class PluginJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes a Plugin instance to a JSON string.
    /// </summary>
    /// <param name="value">The plugin to serialize.</param>
    /// <param name="indented">Whether to indent the JSON output.</param>
    /// <returns>A JSON string representation of the plugin.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this Plugin value, bool indented = false) => value is null
        ? throw new ArgumentNullException(nameof(value))
        : JsonSerializer.Serialize(value, GetJsonSerializerOptions(indented));

    /// <summary>
    /// Deserializes a Plugin instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized Plugin instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static Plugin? FromJson(string json)
    {
        return json is null
            ? throw new ArgumentNullException(nameof(json))
            : TryDeserialize(json, out var result)
                ? result
                : null;
    }

    /// <summary>
    /// Attempts to deserialize a Plugin instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized Plugin instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded and produced a non-null result; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out Plugin? value)
    {
        value = null;
        return json is not null && TryDeserialize(json, out value);
    }

    private static JsonSerializerOptions GetJsonSerializerOptions(bool indented) => indented
        ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
        : _jsonSerializerOptions;

    private static bool TryDeserialize(string json, [NotNullWhen(true)] out Plugin? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<Plugin>(json, _jsonSerializerOptions);
            return result is not null;
        }
        catch (JsonException)
        {
            result = null;
            return false;
        }
    }
}