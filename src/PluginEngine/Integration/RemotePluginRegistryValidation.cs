#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace PluginEngine.Integration;

/// <summary>
/// Provides validation helpers for <see cref="RemotePluginRegistry"/> and related data structures.
/// </summary>
public static class RemotePluginRegistryValidation
{
    /// <summary>
    /// Validates a <see cref="PluginInfo"/> instance.
    /// </summary>
    /// <param name="value">The plugin info to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this PluginInfo? value)
    {
        if (value is null)
        {
            return ["PluginInfo cannot be null"];
        }

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("PluginInfo.Name cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.Version))
        {
            problems.Add("PluginInfo.Version cannot be null, empty, or whitespace");
        }
        else if (!IsValidSemanticVersion(value.Version))
        {
            problems.Add("PluginInfo.Version must be a valid semantic version (e.g., 1.0.0)");
        }

        if (value.Id == Guid.Empty)
        {
            problems.Add("PluginInfo.Id cannot be an empty GUID");
        }

        if (value.DownloadUrl is not null && !IsValidUrl(value.DownloadUrl))
        {
            problems.Add("PluginInfo.DownloadUrl must be a valid URL or null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="PluginVersionInfo"/> instance.
    /// </summary>
    /// <param name="value">The plugin version info to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this PluginVersionInfo? value)
    {
        if (value is null)
        {
            return ["PluginVersionInfo cannot be null"];
        }

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Version))
        {
            problems.Add("PluginVersionInfo.Version cannot be null, empty, or whitespace");
        }
        else if (!IsValidSemanticVersion(value.Version))
        {
            problems.Add("PluginVersionInfo.Version must be a valid semantic version (e.g., 1.0.0)");
        }

        if (value.PublishedAtUtc == default)
        {
            problems.Add("PluginVersionInfo.PublishedAtUtc cannot be the default DateTime value");
        }
        else if (value.PublishedAtUtc > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("PluginVersionInfo.PublishedAtUtc cannot be in the future");
        }

        if (string.IsNullOrWhiteSpace(value.DownloadUrl))
        {
            problems.Add("PluginVersionInfo.DownloadUrl cannot be null, empty, or whitespace");
        }
        else if (!IsValidUrl(value.DownloadUrl))
        {
            problems.Add("PluginVersionInfo.DownloadUrl must be a valid URL");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="PluginPublishMetadata"/> instance.
    /// </summary>
    /// <param name="value">The plugin publish metadata to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this PluginPublishMetadata? value)
    {
        if (value is null)
        {
            return ["PluginPublishMetadata cannot be null"];
        }

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.PluginName))
        {
            problems.Add("PluginPublishMetadata.PluginName cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.Version))
        {
            problems.Add("PluginPublishMetadata.Version cannot be null, empty, or whitespace");
        }
        else if (!IsValidSemanticVersion(value.Version))
        {
            problems.Add("PluginPublishMetadata.Version must be a valid semantic version (e.g., 1.0.0)");
        }

        if (string.IsNullOrWhiteSpace(value.Description))
        {
            problems.Add("PluginPublishMetadata.Description cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.Author))
        {
            problems.Add("PluginPublishMetadata.Author cannot be null, empty, or whitespace");
        }

        if (value.Tags is null)
        {
            problems.Add("PluginPublishMetadata.Tags cannot be null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="RemotePluginRegistry"/> instance.
    /// </summary>
    /// <param name="value">The remote plugin registry to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this RemotePluginRegistry? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return [];
    }

    /// <summary>
    /// Determines whether the specified <see cref="PluginInfo"/> is valid.
    /// </summary>
    /// <param name="value">The plugin info to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this PluginInfo? value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether the specified <see cref="PluginVersionInfo"/> is valid.
    /// </summary>
    /// <param name="value">The plugin version info to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this PluginVersionInfo? value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether the specified <see cref="PluginPublishMetadata"/> is valid.
    /// </summary>
    /// <param name="value">The plugin publish metadata to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this PluginPublishMetadata? value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether the specified <see cref="RemotePluginRegistry"/> is valid.
    /// </summary>
    /// <param name="value">The remote plugin registry to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this RemotePluginRegistry? value) => value is not null;

    /// <summary>
    /// Ensures that the specified <see cref="PluginInfo"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The plugin info to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the value is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this PluginInfo? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PluginInfo is invalid: {string.Join("; ", problems)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="PluginVersionInfo"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The plugin version info to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the value is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this PluginVersionInfo? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PluginVersionInfo is invalid: {string.Join("; ", problems)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="PluginPublishMetadata"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The plugin publish metadata to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the value is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this PluginPublishMetadata? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PluginPublishMetadata is invalid: {string.Join("; ", problems)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="RemotePluginRegistry"/> is valid, throwing an <see cref="ArgumentNullException"/> if null.
    /// </summary>
    /// <param name="value">The remote plugin registry to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this RemotePluginRegistry? value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }

    private static bool IsValidSemanticVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        // Basic semantic version validation: major.minor.patch or major.minor.patch-prerelease
        var parts = version.Split(new[] { '-' }, 2);
        var versionPart = parts[0];

        var versionParts = versionPart.Split('.');
        if (versionParts.Length < 2 || versionParts.Length > 3)
        {
            return false;
        }

        foreach (var part in versionParts)
        {
            if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num) || num < 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Basic URL validation - checks for scheme and valid characters
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}