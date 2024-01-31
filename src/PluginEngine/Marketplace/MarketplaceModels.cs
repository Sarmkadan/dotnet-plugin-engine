#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Marketplace;

/// <summary>
/// Compatibility status between a plugin version and a host engine version.
/// </summary>
public enum CompatibilityStatus
{
    /// <summary>Fully supported and tested against this engine version.</summary>
    Compatible,

    /// <summary>Known to be broken or unsupported on this engine version.</summary>
    Incompatible,

    /// <summary>Works but the combination is no longer maintained or recommended.</summary>
    Deprecated,

    /// <summary>Compatibility has not been verified for this pairing.</summary>
    Unknown
}

/// <summary>
/// Sort order options for marketplace search results.
/// </summary>
public enum MarketplaceSortOrder
{
    /// <summary>Sorted by search relevance score.</summary>
    Relevance,

    /// <summary>Sorted by total download count, descending.</summary>
    Downloads,

    /// <summary>Sorted by average community rating, descending.</summary>
    Rating,

    /// <summary>Sorted by last published date, descending.</summary>
    LastUpdated,

    /// <summary>Sorted alphabetically by plugin name.</summary>
    Name
}

/// <summary>
/// Represents a plugin listing in the marketplace, including discovery metadata
/// and the list of published versions.
/// </summary>
public sealed class MarketplaceEntry
{
    /// <summary>Gets or sets the unique plugin identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the plugin name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the latest stable version string.</summary>
    public string LatestVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the plugin author or maintainer.</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable plugin description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets tags used for marketplace discovery.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets or sets the total number of downloads.</summary>
    public long Downloads { get; set; }

    /// <summary>Gets or sets the average community rating on a 0.0–5.0 scale.</summary>
    public double Rating { get; set; }

    /// <summary>Gets or sets when the plugin was first published.</summary>
    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets when the plugin was last updated.</summary>
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets whether this plugin has been verified by the marketplace.</summary>
    public bool IsVerified { get; set; }

    /// <summary>Gets or sets the SPDX license identifier (e.g., "MIT", "Apache-2.0").</summary>
    public string LicenseType { get; set; } = string.Empty;

    /// <summary>Gets or sets the source repository URL.</summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the full version history fetched from the registry.</summary>
    public List<PluginVersionInfo> AvailableVersions { get; set; } = [];
}

/// <summary>
/// Cross-reference matrix that maps every published plugin version to the set of
/// host engine versions and the <see cref="CompatibilityStatus"/> for each pairing.
/// </summary>
public sealed class VersionCompatibilityMatrix
{
    private readonly Dictionary<string, Dictionary<string, CompatibilityStatus>> _matrix =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the plugin identifier this matrix belongs to.</summary>
    public Guid PluginId { get; init; }

    /// <summary>Gets the plugin display name.</summary>
    public string PluginName { get; init; } = string.Empty;

    /// <summary>Gets when this matrix was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a read-only view of all compatibility entries, keyed by plugin version then engine version.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, CompatibilityStatus>> Entries =>
        _matrix.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyDictionary<string, CompatibilityStatus>)kvp.Value);

    /// <summary>
    /// Records the compatibility status for a plugin version / engine version pair.
    /// </summary>
    /// <param name="pluginVersion">The plugin version string (e.g., "2.1.0").</param>
    /// <param name="engineVersion">The host engine version string (e.g., "10.0").</param>
    /// <param name="status">The resolved compatibility status.</param>
    public void Record(string pluginVersion, string engineVersion, CompatibilityStatus status)
    {
        if (!_matrix.TryGetValue(pluginVersion, out var row))
        {
            row = new Dictionary<string, CompatibilityStatus>(StringComparer.OrdinalIgnoreCase);
            _matrix[pluginVersion] = row;
        }

        row[engineVersion] = status;
    }

    /// <summary>
    /// Returns the compatibility status for the given pairing.
    /// Returns <see cref="CompatibilityStatus.Unknown"/> when no data is recorded.
    /// </summary>
    /// <param name="pluginVersion">The plugin version to look up.</param>
    /// <param name="engineVersion">The engine version to look up.</param>
    public CompatibilityStatus GetStatus(string pluginVersion, string engineVersion)
    {
        if (_matrix.TryGetValue(pluginVersion, out var row) &&
            row.TryGetValue(engineVersion, out var status))
            return status;

        return CompatibilityStatus.Unknown;
    }

    /// <summary>
    /// Returns all plugin versions that are <see cref="CompatibilityStatus.Compatible"/>
    /// with the specified engine version.
    /// </summary>
    /// <param name="engineVersion">The engine version to filter by.</param>
    public IReadOnlyList<string> GetCompatiblePluginVersions(string engineVersion) =>
        _matrix
            .Where(kvp => kvp.Value.TryGetValue(engineVersion, out var s) && s == CompatibilityStatus.Compatible)
            .Select(kvp => kvp.Key)
            .ToList();
}

/// <summary>
/// Search and filter criteria passed to marketplace queries.
/// </summary>
public sealed class MarketplaceSearchFilter
{
    /// <summary>Gets or sets the free-text search query.</summary>
    public string? Query { get; set; }

    /// <summary>Gets or sets tags that results must contain at least one of.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets or sets the minimum acceptable plugin version.</summary>
    public string? MinVersion { get; set; }

    /// <summary>Gets or sets the required target framework moniker (e.g., "net10.0").</summary>
    public string? TargetFramework { get; set; }

    /// <summary>Gets or sets whether to return only marketplace-verified plugins.</summary>
    public bool OnlyVerified { get; set; }

    /// <summary>Gets or sets the 1-based result page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Gets or sets the number of results per page (capped at 100 by the registry).</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>Gets or sets the desired sort order for returned results.</summary>
    public MarketplaceSortOrder SortOrder { get; set; } = MarketplaceSortOrder.Relevance;
}
