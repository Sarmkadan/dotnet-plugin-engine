#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Services.Abstractions;

/// <summary>
/// Service interface for managing plugin and assembly versioning.
/// </summary>
public interface IVersioningService
{
    /// <summary>
    /// Validates a version string.
    /// </summary>
    bool ValidateVersion(string version);

    /// <summary>
    /// Checks if a version satisfies a constraint.
    /// </summary>
    bool IsSatisfiedBy(string constraint, string version);

    /// <summary>
    /// Compares two versions.
    /// </summary>
    int CompareVersions(string version1, string version2);

    /// <summary>
    /// Parses a semantic version string.
    /// </summary>
    SemanticVersion ParseVersion(string versionString);

    /// <summary>
    /// Gets all versions for an entity.
    /// </summary>
    Task<IEnumerable<VersionInfo>> GetVersionHistoryAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the version.
    /// </summary>
    string IncrementVersion(string currentVersion, VersionPart part);

    /// <summary>
    /// Checks if two versions are compatible.
    /// </summary>
    bool AreCompatible(string version1, string version2);

    /// <summary>
    /// Gets the latest version for an entity.
    /// </summary>
    Task<VersionInfo?> GetLatestVersionAsync(Guid entityId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a semantic version.
/// </summary>
public sealed class SemanticVersion
{
    /// <summary>
    /// Gets or sets the major version number.
    /// </summary>
    public int Major { get; set; }
    /// <summary>
    /// Gets or sets the minor version number.
    /// </summary>
    public int Minor { get; set; }
    /// <summary>
    /// Gets or sets the patch version number.
    /// </summary>
    public int Patch { get; set; }
    /// <summary>
    /// Gets or sets the prerelease label.
    /// </summary>
    public string Prerelease { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the build metadata.
    /// </summary>
    public string Metadata { get; set; } = string.Empty;

    /// <summary>
    /// Returns a string representation of the semantic version.
    /// </summary>
    /// <returns>A string in the format Major.Minor.Patch[-Prerelease][+Metadata].</returns>
    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (!string.IsNullOrWhiteSpace(Prerelease))
            version += $"-{Prerelease}";
        if (!string.IsNullOrWhiteSpace(Metadata))
            version += $"+{Metadata}";
        return version;
    }
}

/// <summary>
/// Represents which part of a version to increment.
/// </summary>
public enum VersionPart
{
    /// <summary>
    /// Increment the major version.
    /// </summary>
    Major = 0,
    /// <summary>
    /// Increment the minor version.
    /// </summary>
    Minor = 1,
    /// <summary>
    /// Increment the patch version.
    /// </summary>
    Patch = 2
}
