#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PluginEngine.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="VersionHelperTests"/> instances.
/// </summary>
public static class VersionHelperTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions _indentedJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes a <see cref="VersionHelperTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this VersionHelperTests value, bool indented = false) =>
        JsonSerializer.Serialize(value, indented ? _indentedJsonOptions : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="VersionHelperTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <returns>The deserialized instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized into a <see cref="VersionHelperTests"/> instance.</exception>
    public static VersionHelperTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<VersionHelperTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="VersionHelperTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <param name="value">When this method returns, contains the deserialized instance if successful,
    /// or null if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out VersionHelperTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<VersionHelperTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}