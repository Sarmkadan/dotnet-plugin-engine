// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Extensions;

/// <summary>
/// Extension methods for string operations commonly used in plugin processing.
/// Provides utilities for path normalization, version parsing, and string formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Normalizes a plugin path by removing extra separators and converting to OS-specific format.
    /// </summary>
    public static string NormalizePluginPath(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var normalized = Path.Normalize(path).Replace('/', Path.DirectorySeparatorChar);
        return Path.TrimEndingDirectorySeparator(normalized);
    }

    /// <summary>
    /// Determines if a string represents a valid plugin identifier (GUID format).
    /// </summary>
    public static bool IsValidPluginId(this string value)
    {
        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Determines if a string represents a valid semantic version.
    /// </summary>
    public static bool IsValidVersion(this string value)
    {
        return Version.TryParse(value, out _);
    }

    /// <summary>
    /// Removes unsafe characters from a plugin name for safe file operations.
    /// </summary>
    public static string SanitizeForFilename(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = value.Where(c => !invalidChars.Contains(c)).ToArray();
        return new string(sanitized);
    }

    /// <summary>
    /// Extracts the assembly name from a file path (without extension).
    /// </summary>
    public static string GetAssemblyName(this string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// Determines if string is a valid file path for a .NET assembly.
    /// </summary>
    public static bool IsAssemblyPath(this string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext == ".dll" || ext == ".exe";
    }

    /// <summary>
    /// Truncates a string to a maximum length with ellipsis if needed.
    /// </summary>
    public static string TruncateWithEllipsis(this string value, int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        return value[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Formats a time span in human-readable format (e.g., "2h 30m 45s").
    /// </summary>
    public static string ToReadableTimeSpan(this TimeSpan span)
    {
        var parts = new List<string>();

        if (span.Days > 0) parts.Add($"{span.Days}d");
        if (span.Hours > 0) parts.Add($"{span.Hours}h");
        if (span.Minutes > 0) parts.Add($"{span.Minutes}m");
        if (span.Seconds > 0) parts.Add($"{span.Seconds}s");

        return parts.Count > 0 ? string.Join(" ", parts) : "0s";
    }

    /// <summary>
    /// Converts bytes to human-readable format (e.g., "2.5 MB").
    /// </summary>
    public static string FormatBytes(this long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
