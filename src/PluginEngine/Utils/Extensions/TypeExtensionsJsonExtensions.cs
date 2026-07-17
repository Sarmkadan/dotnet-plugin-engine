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
/// Enables JSON serialization and deserialization of type information using the type's assembly qualified name.
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
    /// Serializes a type to a JSON string using its assembly qualified name.
    /// </summary>
    /// <param name="type">The type to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the type's assembly qualified name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static string ToJson(this Type type, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(type);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(type.AssemblyQualifiedName, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a Type by resolving its assembly qualified name.
    /// </summary>
    /// <param name="json">The JSON string containing the assembly qualified type name.</param>
    /// <returns>The deserialized Type, or null if the JSON is empty or invalid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or the type cannot be resolved.</exception>
    public static Type? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var typeName = JsonSerializer.Deserialize<string>(json, _jsonOptions);
        return typeName is null
            ? null
            : Type.GetType(typeName, throwOnError: false);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a Type.
    /// </summary>
    /// <param name="json">The JSON string containing the assembly qualified type name.</param>
    /// <param name="type">The resulting Type, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds and the type can be resolved; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out Type? type)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        type = null;

        try
        {
            var typeName = JsonSerializer.Deserialize<string>(json, _jsonOptions);
            if (typeName is not null)
            {
                type = Type.GetType(typeName, throwOnError: false);
                return type is not null;
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}