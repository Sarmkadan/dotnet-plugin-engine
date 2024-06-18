#nullable enable
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
		ArgumentNullException.ThrowIfNull(path);

		return string.IsNullOrWhiteSpace(path)
			? string.Empty
			: Path.TrimEndingDirectorySeparator(
				path.Replace('/', Path.DirectorySeparatorChar)
					.Replace('\\', Path.DirectorySeparatorChar));
	}

	/// <summary>
	/// Determines if a string represents a valid plugin identifier (GUID format).
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static bool IsValidPluginId(this string value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return Guid.TryParse(value, out _);
	}

	/// <summary>
	/// Determines if a string represents a valid semantic version.
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static bool IsValidVersion(this string value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return Version.TryParse(value, out _);
	}

	/// <summary>
	/// Removes unsafe characters from a plugin name for safe file operations.
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string SanitizeForFilename(this string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		if (string.IsNullOrEmpty(value))
			return string.Empty;

		var invalidChars = Path.GetInvalidFileNameChars();
		var sanitized = value.Where(c => !invalidChars.Contains(c)).ToArray();
		return new string(sanitized);
	}

	/// <summary>
	/// Extracts the assembly name from a file path (without extension).
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null.</exception>
	public static string GetAssemblyName(this string filePath)
	{
		ArgumentNullException.ThrowIfNull(filePath);
		return Path.GetFileNameWithoutExtension(filePath);
	}

	/// <summary>
	/// Determines if string is a valid file path for a .NET assembly.
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null.</exception>
	public static bool IsAssemblyPath(this string filePath)
	{
		ArgumentNullException.ThrowIfNull(filePath);

		return Path.GetExtension(filePath) is string ext
			&& (ext.Equals(".dll", StringComparison.OrdinalIgnoreCase)
			|| ext.Equals(".exe", StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Truncates a string to a maximum length with ellipsis if needed.
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than 3.</exception>
	public static string TruncateWithEllipsis(this string value, int maxLength)
	{
		ArgumentNullException.ThrowIfNull(value);

		if (maxLength < 3)
			throw new ArgumentOutOfRangeException(nameof(maxLength), "Maximum length must be at least 3 to accommodate the ellipsis.");

		return value.Length <= maxLength
			? value
			: value[..(maxLength - 3)] + "...";
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
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bytes"/> is negative.</exception>
	public static string FormatBytes(this long bytes)
	{
		if (bytes < 0)
			throw new ArgumentOutOfRangeException(nameof(bytes), "Byte count cannot be negative.");

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