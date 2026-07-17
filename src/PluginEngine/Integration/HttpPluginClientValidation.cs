#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// Provides validation helpers for <see cref="HttpPluginClient"/> instances.
/// Validates plugin configuration, version information, and connectivity state.
/// </summary>
public static class HttpPluginClientValidation
{
    /// <summary>
    /// Validates the specified <see cref="HttpPluginClient"/> instance.
    /// </summary>
    /// <param name="value">The plugin client to validate.</param>
    /// <returns>A list of validation errors; empty if the client is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HttpPluginClient? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();


        // Validate PluginId (required Guid)
        if (value.PluginId == default)
        {
            errors.Add("PluginId must be a non-default Guid.");
        }

        // Validate CurrentVersion (required string)
        if (string.IsNullOrWhiteSpace(value.CurrentVersion))
        {
            errors.Add("CurrentVersion must be a non-empty string.");
        }
        else if (!IsValidSemanticVersion(value.CurrentVersion))
        {
            errors.Add("CurrentVersion must be a valid semantic version (e.g., 1.0.0).");
        }

        // Validate AvailableVersion (required string)
        if (string.IsNullOrWhiteSpace(value.AvailableVersion))
        {
            errors.Add("AvailableVersion must be a non-empty string.");
        }
        else if (!IsValidSemanticVersion(value.AvailableVersion))
        {
            errors.Add("AvailableVersion must be a valid semantic version (e.g., 1.0.0).");
        }

        // Validate DownloadUrl (required string)
        if (string.IsNullOrWhiteSpace(value.DownloadUrl))
        {
            errors.Add("DownloadUrl must be a non-empty string.");
        }
        else if (!Uri.IsWellFormedUriString(value.DownloadUrl, UriKind.Absolute))
        {
            errors.Add("DownloadUrl must be a well-formed absolute URI.");
        }

        if (!string.IsNullOrWhiteSpace(value.ReleaseNotes) && value.ReleaseNotes.Length > 10000)
        {
            errors.Add("ReleaseNotes must be 10000 characters or less.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="HttpPluginClient"/> is valid.
    /// </summary>
    /// <param name="value">The plugin client to check.</param>
    /// <returns>True if the client is valid; otherwise, false.</returns>
    public static bool IsValid(this HttpPluginClient? value)
                    => value is not null && value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="HttpPluginClient"/> is valid.
    /// </summary>
    /// <param name="value">The plugin client to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the client is invalid, containing validation errors.</exception>
    public static void EnsureValid(this HttpPluginClient? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "The HttpPluginClient is invalid. Details: " + string.Join(" ", errors),
                nameof(value));
        }
    }

    /// <summary>
    /// Checks if a version string is a valid semantic version.
    /// </summary>
    /// <param name="version">The version string to check.</param>
    /// <returns>True if the version is valid; otherwise, false.</returns>
    private static bool IsValidSemanticVersion(string version)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        // Basic semantic version pattern: major.minor.patch[-prerelease][+buildmetadata]
        // Examples: 1.0.0, 2.3.4-alpha, 1.2.3+build.1234
        var pattern = @"^\d+\.\d+\.\d+(?:-[a-zA-Z0-9-.]+)?(?:\+[a-zA-Z0-9-.]+)?$";
        return System.Text.RegularExpressions.Regex.IsMatch(
            version.Trim(),
            pattern,
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
    }
}
