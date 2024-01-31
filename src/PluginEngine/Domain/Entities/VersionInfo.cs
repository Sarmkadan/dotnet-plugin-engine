#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemVersion = System.Version;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Represents versioning information for plugins and assemblies.
/// </summary>
public sealed class VersionInfo
{
    /// <summary>
    /// Gets the unique identifier for this version record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the entity ID (Plugin or Assembly).
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Gets or sets the version string.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the version release date.
    /// </summary>
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the release notes.
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a pre-release version.
    /// </summary>
    public bool IsPrerelease { get; set; } = false;

    /// <summary>
    /// Gets or sets the pre-release identifier (alpha, beta, rc, etc).
    /// </summary>
    public string PrereleaseIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the build metadata.
    /// </summary>
    public string BuildMetadata { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the compatibility information.
    /// </summary>
    public string Compatibility { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this version is currently active/recommended.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the version deprecation information.
    /// </summary>
    public string DeprecationNotice { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the download count.
    /// </summary>
    public int DownloadCount { get; set; } = 0;

    /// <summary>
    /// Gets the full semantic version string.
    /// </summary>
    public string GetSemanticVersion()
    {
        var version = Version;

        if (IsPrerelease && !string.IsNullOrWhiteSpace(PrereleaseIdentifier))
            version = $"{version}-{PrereleaseIdentifier}";

        if (!string.IsNullOrWhiteSpace(BuildMetadata))
            version = $"{version}+{BuildMetadata}";

        return version;
    }

    /// <summary>
    /// Gets the formatted version string for display.
    /// </summary>
    public string GetDisplayString()
    {
        var display = GetSemanticVersion();
        if (IsPrerelease)
            display += " (pre-release)";

        if (!string.IsNullOrWhiteSpace(DeprecationNotice))
            display += " [DEPRECATED]";

        return display;
    }

    /// <summary>
    /// Checks if this version is compatible with another version.
    /// </summary>
    public bool IsCompatibleWith(string otherVersion)
    {
        if (string.IsNullOrWhiteSpace(otherVersion))
            return false;

        if (!SystemVersion.TryParse(this.Version, out var thisVer) || !SystemVersion.TryParse(otherVersion, out var otherVer))
            return false;

        // Compatible if major versions match
        return thisVer.Major == otherVer.Major;
    }

    /// <summary>
    /// Increments the version patch number.
    /// </summary>
    public void IncrementPatch()
    {
        if (SystemVersion.TryParse(this.Version, out var version))
        {
            Version = new SystemVersion(version.Major, version.Minor, version.Build + 1).ToString();
        }
    }

    /// <summary>
    /// Increments the version minor number.
    /// </summary>
    public void IncrementMinor()
    {
        if (SystemVersion.TryParse(this.Version, out var version))
        {
            Version = new SystemVersion(version.Major, version.Minor + 1, 0).ToString();
        }
    }

    /// <summary>
    /// Increments the version major number.
    /// </summary>
    public void IncrementMajor()
    {
        if (SystemVersion.TryParse(this.Version, out var version))
        {
            Version = new SystemVersion(version.Major + 1, 0, 0).ToString();
        }
    }

    /// <summary>
    /// Validates the version information.
    /// </summary>
    public bool IsValid()
    {
        return EntityId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(Version) &&
               SystemVersion.TryParse(this.Version, out _);
    }
}
