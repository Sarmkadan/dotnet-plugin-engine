#nullable enable
using System.Diagnostics;

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// Manages interaction with a remote plugin registry for discovery, updates, and publishing.
/// Caches registry data locally to minimize network requests.
/// </summary>
public sealed class RemotePluginRegistry : IRemotePluginRegistry
{
    private readonly HttpPluginClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RemotePluginRegistry> _logger;
    private readonly VersionHelper _versionHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemotePluginRegistry"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    public RemotePluginRegistry(
        HttpPluginClient httpClient,
        IMemoryCache cache,
        ILogger<RemotePluginRegistry> logger,
        VersionHelper versionHelper)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(versionHelper);

        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _versionHelper = versionHelper;
    }

    /// <summary>
    /// Searches the registry for plugins matching search criteria.
    /// </summary>
    /// <param name="query">The free-text search query. Cannot be null or whitespace.</param>
    /// <param name="limit">Maximum number of results. Must be greater than zero.</param>
    /// <returns>The matching plugins, empty when the registry returns nothing.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limit"/> is not positive.</exception>
    public async Task<List<PluginInfo>> SearchAsync(string query, int limit = 20)
    {
        _logger.LogDebug("Starting registry plugin search for {Query} with limit {Limit}", query, limit);

        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var cacheKey = $"registry_search_{query}_{limit.ToString(CultureInfo.InvariantCulture)}";

            if (_cache.TryGetValue(cacheKey, out List<PluginInfo>? cached))
                return cached ?? [];

            var results = await _httpClient.SearchPluginsAsync(query, limit);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };

            _cache.Set(cacheKey, results, cacheOptions);
            _logger.LogDebug("Registry search for '{Query}' returned {Count} result(s)", query, results.Count);

            return results;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogDebug("Registry plugin search completed in {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Gets information about a specific plugin from the registry.
    /// </summary>
    public async Task<PluginInfo?> GetPluginAsync(Guid pluginId)
    {
        _logger.LogDebug("Starting registry plugin lookup for {PluginId}", pluginId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var cacheKey = $"registry_plugin_{pluginId}";

            if (_cache.TryGetValue(cacheKey, out PluginInfo? cached))
                return cached;

            var pluginInfo = await _httpClient.GetPluginInfoAsync(pluginId);

            if (pluginInfo is not null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                };

                _cache.Set(cacheKey, pluginInfo, cacheOptions);
            }

            return pluginInfo;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogDebug("Registry plugin lookup for {PluginId} completed in {ElapsedMs} ms", pluginId, stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Gets all available versions of a plugin.
    /// </summary>
    public async Task<List<PluginVersionInfo>> GetVersionsAsync(Guid pluginId)
    {
        _logger.LogDebug("Starting registry version lookup for {PluginId}", pluginId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var cacheKey = $"registry_versions_{pluginId}";

            if (_cache.TryGetValue(cacheKey, out List<PluginVersionInfo>? cached))
                return cached ?? [];

            var versions = await _httpClient.GetPluginVersionsAsync(pluginId);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            };

            _cache.Set(cacheKey, versions, cacheOptions);
            return versions;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogDebug("Registry version lookup for {PluginId} completed in {ElapsedMs} ms", pluginId, stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Downloads a plugin from the registry.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="version"/> or <paramref name="downloadPath"/> is null or whitespace.</exception>
    public async Task<string?> DownloadPluginAsync(Guid pluginId, string version, string downloadPath)
    {
        _logger.LogInformation("Starting plugin download for {PluginId} version {Version} to {DownloadPath}",
            pluginId, version, downloadPath);

        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadPath);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var pluginInfo = await GetPluginAsync(pluginId);
            if (pluginInfo?.DownloadUrl is null)
            {
                _logger.LogWarning("No download URL found for plugin: {PluginId}", pluginId);
                return null;
            }

            using var response = await _httpClient.GetAsync(pluginInfo.DownloadUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to download plugin: {StatusCode}", response.StatusCode);
                return null;
            }

            Directory.CreateDirectory(downloadPath);

            var fileName = Path.Combine(downloadPath, $"{pluginInfo.Name}.{version}.dll");
            await using (var fileStream = File.Create(fileName))
            {
                await response.Content.CopyToAsync(fileStream);
            }

            _logger.LogInformation("Downloaded plugin: {PluginId} v{Version} -> {FilePath}",
                pluginId, version, fileName);

            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading plugin: {PluginId}", pluginId);
            return null;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Plugin download for {PluginId} completed in {ElapsedMs} ms",
                pluginId, stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Publishes a plugin to the registry.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is null.</exception>
    public async Task<bool> PublishPluginAsync(string filePath, PluginPublishMetadata metadata)
    {
        _logger.LogInformation("Starting plugin publish from {FilePath}", filePath);

        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(metadata);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("Plugin file not found: {FilePath}", filePath);
                return false;
            }

            var description = $"{metadata.Description} (Author: {metadata.Author})";
            var success = await _httpClient.UploadPluginAsync(filePath, description);

            if (success)
            {
                _logger.LogInformation("Published plugin to registry: {PluginName}",
                    metadata.PluginName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing plugin: {FilePath}", filePath);
            return false;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Plugin publish from {FilePath} completed in {ElapsedMs} ms",
                filePath, stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Invalidates cached data for a specific plugin.
    /// </summary>
    public void InvalidateCache(Guid pluginId)
    {
        _logger.LogDebug("Starting registry cache invalidation for {PluginId}", pluginId);

        _cache.Remove($"registry_plugin_{pluginId}");
        _cache.Remove($"registry_versions_{pluginId}");
    }
}

/// <summary>
/// Contains version-specific plugin information from registry.
/// </summary>
public sealed class PluginVersionInfo
{
    /// <summary>
    /// The version of the plugin.
    /// </summary>
    public required string Version { get; set; }
    /// <summary>
    /// The date and time when the plugin was published (in UTC).
    /// </summary>
    public required DateTime PublishedAtUtc { get; set; }
    /// <summary>
    /// The URL from which the plugin can be downloaded.
    /// </summary>
    public required string DownloadUrl { get; set; }
    /// <summary>
    /// Indicates whether the plugin version is stable.
    /// </summary>
    public bool IsStable { get; set; }
    /// <summary>
    /// Indicates whether the plugin version is a pre-release.
    /// </summary>
    public bool IsPrerelease { get; set; }
    /// <summary>
    /// The release notes for the plugin version.
    /// </summary>
    public string? ReleaseNotes { get; set; }
}

/// <summary>
/// Metadata for publishing a plugin to the registry.
/// </summary>
public sealed class PluginPublishMetadata
{
    /// <summary>
    /// The name of the plugin.
    /// </summary>
    public required string PluginName { get; set; }
    /// <summary>
    /// The version of the plugin.
    /// </summary>
    public required string Version { get; set; }
    /// <summary>
    /// The description of the plugin.
    /// </summary>
    public required string Description { get; set; }
    /// <summary>
    /// The author of the plugin.
    /// </summary>
    public required string Author { get; set; }
    /// <summary>
    /// The company that created the plugin (optional).
    /// </summary>
    public string? Company { get; set; }
    /// <summary>
    /// The tags associated with the plugin.
    /// </summary>
    public List<string> Tags { get; set; } = [];
    /// <summary>
    /// The license type of the plugin (optional).
    /// </summary>
    public string? LicenseType { get; set; }
}
