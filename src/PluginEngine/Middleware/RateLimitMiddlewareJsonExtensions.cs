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
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this RateLimitMiddleware value, bool indented = false) =>
        value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(value, GetOptions(indented));

    /// <summary>
    /// Deserializes a JSON string to a <see cref="RateLimitMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized middleware instance, or <see langword="null"/> if the JSON is <see langword="null"/>, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static RateLimitMiddleware? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : TryDeserialize(json);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="RateLimitMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized middleware instance, or <see langword="null"/> if deserialization fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out RateLimitMiddleware? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = string.IsNullOrWhiteSpace(json)
            ? null
            : TryDeserialize(json);

        return value is not null;
    }

    private static JsonSerializerOptions GetOptions(bool indented) =>
        indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

    private static RateLimitMiddleware? TryDeserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<RateLimitMiddleware>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}