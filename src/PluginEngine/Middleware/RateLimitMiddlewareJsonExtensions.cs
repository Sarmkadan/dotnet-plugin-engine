#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PluginEngine.Middleware;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="RateLimitMiddleware"/>.
/// </summary>
public static class RateLimitMiddlewareJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions;

    static RateLimitMiddlewareJsonExtensions()
    {
        _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Serializes the <see cref="RateLimitMiddleware"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The middleware instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the middleware.</returns>
    public static string ToJson(this RateLimitMiddleware value, bool indented = false)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = _jsonOptions;
        if (indented)
        {
            options = new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            };
        }

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="RateLimitMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized middleware instance, or null if the JSON is null or empty.</returns>
    public static RateLimitMiddleware? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<RateLimitMiddleware>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="RateLimitMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized middleware instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out RateLimitMiddleware? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<RateLimitMiddleware>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}