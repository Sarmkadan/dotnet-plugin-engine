#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for PluginAssembly.
/// </summary>
public static class PluginAssemblyJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the PluginAssembly to a JSON string.
    /// </summary>
    /// <param name="value">The PluginAssembly instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the PluginAssembly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this PluginAssembly value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a PluginAssembly instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A PluginAssembly instance if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    public static PluginAssembly? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            return JsonSerializer.Deserialize<PluginAssembly>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a PluginAssembly instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized PluginAssembly instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out PluginAssembly? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<PluginAssembly>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}