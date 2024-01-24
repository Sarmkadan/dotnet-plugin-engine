// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Helpers;

/// <summary>
/// Helper class for semantic version operations in the plugin system.
/// Parses, compares, and validates version specifications.
/// Supports semantic versioning with prerelease and build metadata.
/// </summary>
public class VersionHelper
{
    private readonly ILogger<VersionHelper> _logger;

    public VersionHelper(ILogger<VersionHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses a version string into a Version object.
    /// Handles common version formats with validation.
    /// </summary>
    public Version? ParseVersion(string versionString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(versionString))
                return null;

            // Clean the version string of common prefixes
            var cleaned = versionString
                .TrimStart('v', 'V')
                .Split(['-', '+'], StringSplitOptions.RemoveEmptyEntries)[0];

            if (Version.TryParse(cleaned, out var version))
            {
                _logger.LogDebug("Parsed version: {Input} -> {Output}", versionString, version);
                return version;
            }

            _logger.LogWarning("Failed to parse version: {VersionString}", versionString);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing version: {VersionString}", versionString);
            return null;
        }
    }

    /// <summary>
    /// Compares two version strings. Returns -1 if v1 < v2, 0 if equal, 1 if v1 > v2.
    /// </summary>
    public int CompareVersions(string version1, string version2)
    {
        var v1 = ParseVersion(version1);
        var v2 = ParseVersion(version2);

        if (v1 == null || v2 == null)
            return -1;

        return v1.CompareTo(v2);
    }

    /// <summary>
    /// Determines if a version satisfies a constraint (e.g., ">=1.0.0", "~1.5.0").
    /// </summary>
    public bool SatisfiesConstraint(string version, string constraint)
    {
        var parsedVersion = ParseVersion(version);
        if (parsedVersion == null)
            return false;

        constraint = constraint.Trim();

        // Exact version
        if (constraint[0] != '<' && constraint[0] != '>' && constraint[0] != '~' && constraint[0] != '^')
        {
            var constraintVersion = ParseVersion(constraint);
            return constraintVersion == parsedVersion;
        }

        // Exact match
        if (constraint.StartsWith("=="))
        {
            var target = ParseVersion(constraint[2..]);
            return target == parsedVersion;
        }

        // Greater than or equal
        if (constraint.StartsWith(">="))
        {
            var target = ParseVersion(constraint[2..]);
            return target != null && parsedVersion >= target;
        }

        // Greater than
        if (constraint.StartsWith(">"))
        {
            var target = ParseVersion(constraint[1..]);
            return target != null && parsedVersion > target;
        }

        // Less than or equal
        if (constraint.StartsWith("<="))
        {
            var target = ParseVersion(constraint[2..]);
            return target != null && parsedVersion <= target;
        }

        // Less than
        if (constraint.StartsWith("<"))
        {
            var target = ParseVersion(constraint[1..]);
            return target != null && parsedVersion < target;
        }

        // Caret (compatible with version, allows patch updates)
        if (constraint.StartsWith("^"))
        {
            var target = ParseVersion(constraint[1..]);
            if (target == null)
                return false;

            return parsedVersion.Major == target.Major &&
                   parsedVersion >= target;
        }

        // Tilde (allows minor updates)
        if (constraint.StartsWith("~"))
        {
            var target = ParseVersion(constraint[1..]);
            if (target == null)
                return false;

            return parsedVersion.Major == target.Major &&
                   parsedVersion.Minor == target.Minor &&
                   parsedVersion >= target;
        }

        return false;
    }

    /// <summary>
    /// Gets the latest version from a collection of version strings.
    /// </summary>
    public string? GetLatestVersion(IEnumerable<string> versions)
    {
        var parsed = versions
            .Select(v => (Original: v, Parsed: ParseVersion(v)))
            .Where(x => x.Parsed != null)
            .OrderByDescending(x => x.Parsed)
            .FirstOrDefault();

        return parsed.Parsed != null ? parsed.Original : null;
    }

    /// <summary>
    /// Gets version statistics (major, minor, patch, preview/prerelease indicator).
    /// </summary>
    public VersionInfo GetVersionInfo(string versionString)
    {
        var version = ParseVersion(versionString);

        return new VersionInfo
        {
            Original = versionString,
            Major = version?.Major ?? 0,
            Minor = version?.Minor ?? 0,
            Patch = version?.Build >= 0 ? version.Build : 0,
            IsPrerelease = versionString.Contains("-") || versionString.Contains("beta") || versionString.Contains("alpha"),
            IsStable = !versionString.Contains("-") && !versionString.Contains("beta") && !versionString.Contains("alpha")
        };
    }

    /// <summary>
    /// Validates if a version string is in proper semantic versioning format.
    /// </summary>
    public bool IsValidSemanticVersion(string version)
    {
        var parsed = ParseVersion(version);
        return parsed != null && parsed.Major >= 0 && parsed.Minor >= 0;
    }
}

/// <summary>
/// Contains parsed version information.
/// </summary>
public class VersionInfo
{
    public required string Original { get; set; }
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public bool IsPrerelease { get; set; }
    public bool IsStable { get; set; }
}
