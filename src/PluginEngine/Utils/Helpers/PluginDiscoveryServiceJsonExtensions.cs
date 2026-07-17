#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PluginEngine.Utils.Helpers;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="PluginDiscoveryService"/>.
/// </summary>
public static class PluginDiscoveryServiceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the <see cref="PluginDiscoveryService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this PluginDiscoveryService value, bool indented = false)
        => JsonSerializer.Serialize(value, indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PluginDiscoveryService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static PluginDiscoveryService FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return json.Length == 0
            ? throw new ArgumentException("Value cannot be empty.", nameof(json))
            : JsonSerializer.Deserialize<PluginDiscoveryService>(json, _jsonOptions)
                ?? throw new JsonException("Deserialization returned null, indicating invalid JSON or missing required properties.");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PluginDiscoveryService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static bool TryFromJson(string json, out PluginDiscoveryService? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        value = json.Length == 0
            ? throw new ArgumentException("Value cannot be empty.", nameof(json))
            : JsonSerializer.Deserialize<PluginDiscoveryService>(json, _jsonOptions);
        return value is not null;
    }
}