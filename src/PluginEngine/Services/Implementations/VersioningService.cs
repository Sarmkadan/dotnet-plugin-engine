// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Services.Abstractions;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Service implementation for managing plugin and assembly versioning.
/// </summary>
public class VersioningService : IVersioningService
{
    /// <summary>
    /// Validates a version string.
    /// </summary>
    public bool ValidateVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return false;

        return Version.TryParse(version, out _);
    }

    /// <summary>
    /// Checks if a version satisfies a constraint.
    /// </summary>
    public bool IsSatisfiedBy(string constraint, string version)
    {
        if (string.IsNullOrWhiteSpace(constraint) || string.IsNullOrWhiteSpace(version))
            return false;

        if (!constraint.Contains(">=") && !constraint.Contains("<=") && !constraint.Contains("=="))
            return Version.TryParse(version, out var v) && v.ToString().StartsWith(constraint);

        if (constraint.StartsWith(">="))
        {
            var minVersion = constraint.Substring(2).Trim();
            return Version.TryParse(minVersion, out var min) &&
                   Version.TryParse(version, out var v) &&
                   v >= min;
        }

        if (constraint.StartsWith("=="))
        {
            var exactVersion = constraint.Substring(2).Trim();
            return Version.TryParse(exactVersion, out var exact) &&
                   Version.TryParse(version, out var v) &&
                   v == exact;
        }

        return false;
    }

    /// <summary>
    /// Compares two versions.
    /// </summary>
    public int CompareVersions(string version1, string version2)
    {
        if (!Version.TryParse(version1, out var v1) || !Version.TryParse(version2, out var v2))
            throw new ArgumentException("Invalid version format.");

        return v1.CompareTo(v2);
    }

    /// <summary>
    /// Parses a semantic version string.
    /// </summary>
    public SemanticVersion ParseVersion(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
            throw new ArgumentException("Version string cannot be empty.", nameof(versionString));

        var version = new SemanticVersion();
        var parts = versionString.Split(new[] { '-', '+' }, StringSplitOptions.None);

        if (Version.TryParse(parts[0], out var parsed))
        {
            version.Major = parsed.Major;
            version.Minor = parsed.Minor;
            version.Patch = parsed.Build;
        }

        if (versionString.Contains('-'))
        {
            var prereleasePart = versionString.Substring(versionString.IndexOf('-') + 1);
            if (prereleasePart.Contains('+'))
            {
                version.Prerelease = prereleasePart.Substring(0, prereleasePart.IndexOf('+'));
                version.Metadata = prereleasePart.Substring(prereleasePart.IndexOf('+') + 1);
            }
            else
            {
                version.Prerelease = prereleasePart;
            }
        }
        else if (versionString.Contains('+'))
        {
            version.Metadata = versionString.Substring(versionString.IndexOf('+') + 1);
        }

        return version;
    }

    /// <summary>
    /// Gets all versions for an entity.
    /// </summary>
    public Task<IEnumerable<Domain.Entities.VersionInfo>> GetVersionHistoryAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        // This would typically query a repository
        return Task.FromResult(Enumerable.Empty<Domain.Entities.VersionInfo>());
    }

    /// <summary>
    /// Increments the version.
    /// </summary>
    public string IncrementVersion(string currentVersion, VersionPart part)
    {
        if (!Version.TryParse(currentVersion, out var version))
            throw new ArgumentException("Invalid version format.", nameof(currentVersion));

        return part switch
        {
            VersionPart.Major => new Version(version.Major + 1, 0, 0).ToString(),
            VersionPart.Minor => new Version(version.Major, version.Minor + 1, 0).ToString(),
            VersionPart.Patch => new Version(version.Major, version.Minor, version.Build + 1).ToString(),
            _ => throw new ArgumentException("Invalid version part.", nameof(part))
        };
    }

    /// <summary>
    /// Checks if two versions are compatible.
    /// </summary>
    public bool AreCompatible(string version1, string version2)
    {
        if (!Version.TryParse(version1, out var v1) || !Version.TryParse(version2, out var v2))
            return false;

        // Compatible if major versions match
        return v1.Major == v2.Major;
    }

    /// <summary>
    /// Gets the latest version for an entity.
    /// </summary>
    public Task<Domain.Entities.VersionInfo?> GetLatestVersionAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        // This would typically query a repository
        return Task.FromResult<Domain.Entities.VersionInfo?>(null);
    }
}
