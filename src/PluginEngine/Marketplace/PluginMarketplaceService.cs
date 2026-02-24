// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Marketplace;

/// <summary>
/// Contract for the plugin marketplace: browsing, compatibility checking, and installation.
/// </summary>
public interface IPluginMarketplaceService
{
    /// <summary>
    /// Searches the marketplace using the provided filter criteria and returns matching entries.
    /// </summary>
    /// <param name="filter">Search and pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PluginOperationResult<List<MarketplaceEntry>>> SearchAsync(
        MarketplaceSearchFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the full marketplace listing for a single plugin, including all published versions.
    /// </summary>
    /// <param name="pluginId">The unique plugin identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PluginOperationResult<MarketplaceEntry>> GetEntryAsync(
        Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the version compatibility matrix for a plugin, mapping each published version
    /// to known host engine versions and their <see cref="CompatibilityStatus"/>.
    /// Results are cached for <c>30</c> minutes to minimise registry round-trips.
    /// </summary>
    /// <param name="pluginId">The unique plugin identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PluginOperationResult<VersionCompatibilityMatrix>> GetCompatibilityMatrixAsync(
        Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a specific plugin version is compatible with the given engine version.
    /// </summary>
    /// <param name="pluginId">The unique plugin identifier.</param>
    /// <param name="pluginVersion">The plugin version string to evaluate.</param>
    /// <param name="engineVersion">The host engine version to check against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PluginOperationResult<CompatibilityStatus>> CheckCompatibilityAsync(
        Guid pluginId, string pluginVersion, string engineVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads and installs a specific plugin version from the marketplace into the target directory.
    /// </summary>
    /// <param name="pluginId">The unique plugin identifier.</param>
    /// <param name="version">The version string to install.</param>
    /// <param name="targetDirectory">Destination directory on the local file system.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PluginOperationResult> InstallAsync(
        Guid pluginId, string version, string targetDirectory,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IPluginMarketplaceService"/>, backed by the remote
/// plugin registry. Compatibility matrices are cached in memory to reduce network overhead.
/// </summary>
public class PluginMarketplaceService : IPluginMarketplaceService
{
    private readonly IRemotePluginRegistry _registry;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PluginMarketplaceService> _logger;

    private static readonly TimeSpan MatrixCacheTtl = TimeSpan.FromMinutes(30);
    private static readonly string[] KnownEngineVersions = ["8.0", "9.0", "10.0"];

    /// <summary>
    /// Initialises a new instance of <see cref="PluginMarketplaceService"/>.
    /// </summary>
    public PluginMarketplaceService(
        IRemotePluginRegistry registry,
        IMemoryCache cache,
        ILogger<PluginMarketplaceService> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _cache    = cache    ?? throw new ArgumentNullException(nameof(cache));
        _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult<List<MarketplaceEntry>>> SearchAsync(
        MarketplaceSearchFilter filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var raw = await _registry.SearchAsync(filter.Query ?? string.Empty, filter.PageSize);
            var entries = raw.Select(ToMarketplaceEntry).ToList();

            _logger.LogDebug("Marketplace search '{Query}' returned {Count} result(s)", filter.Query, entries.Count);
            return PluginOperationResult<List<MarketplaceEntry>>.CreateSuccess(entries, $"Found {entries.Count} plugin(s).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Marketplace search failed for query: {Query}", filter.Query);
            return PluginOperationResult<List<MarketplaceEntry>>.FromException(ex);
        }
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult<MarketplaceEntry>> GetEntryAsync(
        Guid pluginId, CancellationToken cancellationToken = default)
    {
        try
        {
            var info = await _registry.GetPluginAsync(pluginId);
            if (info is null)
                return PluginOperationResult<MarketplaceEntry>.CreateFailure("Plugin not found in the marketplace.", 404);

            var versions = await _registry.GetVersionsAsync(pluginId);
            var entry = ToMarketplaceEntry(info);
            entry.AvailableVersions = versions;

            return PluginOperationResult<MarketplaceEntry>.CreateSuccess(entry, "Plugin entry retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve marketplace entry for plugin {PluginId}", pluginId);
            return PluginOperationResult<MarketplaceEntry>.FromException(ex);
        }
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult<VersionCompatibilityMatrix>> GetCompatibilityMatrixAsync(
        Guid pluginId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"mp_matrix_{pluginId}";

        if (_cache.TryGetValue(cacheKey, out VersionCompatibilityMatrix? cached) && cached is not null)
            return PluginOperationResult<VersionCompatibilityMatrix>.CreateSuccess(cached, "Matrix served from cache.");

        try
        {
            var info     = await _registry.GetPluginAsync(pluginId);
            var versions = await _registry.GetVersionsAsync(pluginId);

            var matrix = new VersionCompatibilityMatrix
            {
                PluginId   = pluginId,
                PluginName = info?.Name ?? pluginId.ToString()
            };

            foreach (var ver in versions)
            {
                foreach (var engineVer in KnownEngineVersions)
                    matrix.Record(ver.Version, engineVer, DetermineCompatibility(ver, engineVer));
            }

            _cache.Set(cacheKey, matrix, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = MatrixCacheTtl
            });

            _logger.LogDebug(
                "Built compatibility matrix for plugin {PluginId}: {VersionCount} version(s) × {EngineCount} engine version(s)",
                pluginId, versions.Count, KnownEngineVersions.Length);

            return PluginOperationResult<VersionCompatibilityMatrix>.CreateSuccess(matrix, "Compatibility matrix generated.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build compatibility matrix for plugin {PluginId}", pluginId);
            return PluginOperationResult<VersionCompatibilityMatrix>.FromException(ex);
        }
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult<CompatibilityStatus>> CheckCompatibilityAsync(
        Guid pluginId, string pluginVersion, string engineVersion,
        CancellationToken cancellationToken = default)
    {
        var matrixResult = await GetCompatibilityMatrixAsync(pluginId, cancellationToken);
        if (!matrixResult.Success)
            return PluginOperationResult<CompatibilityStatus>.CreateFailure(matrixResult.Message, matrixResult.ErrorCode ?? 500);

        var status = matrixResult.Data!.GetStatus(pluginVersion, engineVersion);
        return PluginOperationResult<CompatibilityStatus>.CreateSuccess(status, $"Compatibility status: {status}.");
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult> InstallAsync(
        Guid pluginId, string version, string targetDirectory,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var filePath = await _registry.DownloadPluginAsync(pluginId, version, targetDirectory);
            if (filePath is null)
                return PluginOperationResult.CreateFailure("Download failed: the registry returned no file.", 502);

            _logger.LogInformation("Installed plugin {PluginId} v{Version} -> {Directory}", pluginId, version, targetDirectory);
            return PluginOperationResult.CreateSuccess($"Plugin installed successfully at: {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install plugin {PluginId} v{Version}", pluginId, version);
            return PluginOperationResult.FromException(ex);
        }
    }

    private static MarketplaceEntry ToMarketplaceEntry(PluginInfo info) => new()
    {
        Id            = info.Id,
        Name          = info.Name,
        LatestVersion = info.Version,
        Author        = info.Author      ?? string.Empty,
        Description   = info.Description ?? string.Empty
    };

    /// <summary>
    /// Derives a compatibility status from the plugin version's stability flags and publication age.
    /// Newer stable releases are marked Compatible; aging ones become Deprecated then Incompatible
    /// on older engine majors. Prereleases are only marked Compatible on the latest engine.
    /// </summary>
    private static CompatibilityStatus DetermineCompatibility(PluginVersionInfo ver, string engineVersion)
    {
        if (!Version.TryParse(engineVersion, out var engine))
            return CompatibilityStatus.Unknown;

        if (ver.IsPrerelease)
            return engine.Major >= 10 ? CompatibilityStatus.Compatible : CompatibilityStatus.Unknown;

        if (!ver.IsStable)
            return CompatibilityStatus.Unknown;

        var ageDays = (DateTime.UtcNow - ver.PublishedAtUtc).TotalDays;

        return ageDays switch
        {
            <= 365  => CompatibilityStatus.Compatible,
            <= 730  => engine.Major >= 9  ? CompatibilityStatus.Compatible : CompatibilityStatus.Deprecated,
            _       => engine.Major >= 10 ? CompatibilityStatus.Deprecated  : CompatibilityStatus.Incompatible
        };
    }
}
