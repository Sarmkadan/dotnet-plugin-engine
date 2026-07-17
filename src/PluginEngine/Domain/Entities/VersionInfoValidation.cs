#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Provides validation helpers for <see cref="VersionInfo"/> instances.
/// </summary>
public static class VersionInfoValidation
{
    /// <summary>
    /// Validates a <see cref="VersionInfo"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The version info to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this VersionInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
            errors.Add("Id must not be empty.");

        // Validate EntityId
        if (value.EntityId == Guid.Empty)
            errors.Add("EntityId must not be empty.");

        // Validate Version
        if (string.IsNullOrWhiteSpace(value.Version))
            errors.Add("Version must not be null or whitespace.");
        else if (!Version.TryParse(value.Version, out _))
            errors.Add("Version must be a valid semantic version string (e.g., 1.0.0).");

        // Validate ReleaseDate
        if (value.ReleaseDate == default)
            errors.Add("ReleaseDate must not be the default DateTime value.");
        else if (value.ReleaseDate > DateTime.UtcNow.AddDays(1))
            errors.Add("ReleaseDate cannot be in the future.");
        else if (value.ReleaseDate < new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            errors.Add("ReleaseDate must be a valid date after year 2000.");

        // Validate ReleaseNotes (can be empty, but not null)
        ArgumentNullException.ThrowIfNull(value.ReleaseNotes);

        // Validate IsPrerelease and PrereleaseIdentifier
        if (value.IsPrerelease && string.IsNullOrWhiteSpace(value.PrereleaseIdentifier))
            errors.Add("PrereleaseIdentifier must not be null or whitespace when IsPrerelease is true.");

        // Validate BuildMetadata (can be empty)
        ArgumentNullException.ThrowIfNull(value.BuildMetadata);

        // Validate Compatibility (can be empty)
        ArgumentNullException.ThrowIfNull(value.Compatibility);

        // Validate DeprecationNotice (can be empty)
        ArgumentNullException.ThrowIfNull(value.DeprecationNotice);

        // Validate DownloadCount
        if (value.DownloadCount < 0)
            errors.Add("DownloadCount must not be negative.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="VersionInfo"/> is valid.
    /// </summary>
    /// <param name="value">The version info to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this VersionInfo value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="VersionInfo"/> is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The version info to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this VersionInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"VersionInfo is invalid. Validation errors:\n{string.Join("\n", errors)}",
                nameof(value));
        }
    }
}