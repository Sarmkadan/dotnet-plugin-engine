#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PluginEngine.Utils.Extensions;

/// <summary>
/// Provides System.Text.Json serialization extensions for type metadata.
/// Enables JSON serialization and deserialization of type information.
/// </summary>
public static class TypeExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes a type to a JSON string.
    /// </summary>
    /// <param name="type">The type to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static string ToJson(this Type type, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(type);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(type, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a Type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized Type, or null if the JSON is empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static Type? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Type>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a Type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="type">The resulting Type, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out Type? type)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            type = JsonSerializer.Deserialize<Type>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            type = null;
            return false;
        }
    }
}