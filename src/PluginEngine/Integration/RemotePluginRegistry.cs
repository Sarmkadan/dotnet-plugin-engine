// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// Manages interaction with a remote plugin registry for discovery, updates, and publishing.
/// Caches registry data locally to minimize network requests.
/// </summary>
public class RemotePluginRegistry
{
    private readonly HttpPluginClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RemotePluginRegistry> _logger;
    private readonly VersionHelper _versionHelper;

    public RemotePluginRegistry(
        HttpPluginClient httpClient,
        IMemoryCache cache,
        ILogger<RemotePluginRegistry> logger,
        VersionHelper versionHelper)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _versionHelper = versionHelper;
    }

    /// <summary>
    /// Searches the registry for plugins matching search criteria.
    /// </summary>
    public async Task<List<PluginInfo>> SearchAsync(string query, int limit = 20)
    {
        var cacheKey = $"registry_search_{query}_{limit}";

        if (_cache.TryGetValue(cacheKey, out List<PluginInfo>? cached))
            return cached ?? [];

        var results = new List<PluginInfo>();

        // In production, would call actual HTTP endpoint
        // For now, returning empty list as foundation for real implementation
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };

        _cache.Set(cacheKey, results, cacheOptions);
        return results;
    }

    /// <summary>
    /// Gets information about a specific plugin from the registry.
    /// </summary>
    public async Task<PluginInfo?> GetPluginAsync(Guid pluginId)
    {
        var cacheKey = $"registry_plugin_{pluginId}";

        if (_cache.TryGetValue(cacheKey, out PluginInfo? cached))
            return cached;

        var pluginInfo = await _httpClient.GetPluginInfoAsync(pluginId);

        if (pluginInfo != null)
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

    /// <summary>
    /// Gets all available versions of a plugin.
    /// </summary>
    public async Task<List<PluginVersionInfo>> GetVersionsAsync(Guid pluginId)
    {
        var cacheKey = $"registry_versions_{pluginId}";

        if (_cache.TryGetValue(cacheKey, out List<PluginVersionInfo>? cached))
            return cached ?? [];

        // Foundation for real HTTP-based retrieval
        var versions = new List<PluginVersionInfo>();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
        };

        _cache.Set(cacheKey, versions, cacheOptions);
        return versions;
    }

    /// <summary>
    /// Downloads a plugin from the registry.
    /// </summary>
    public async Task<string?> DownloadPluginAsync(Guid pluginId, string version, string downloadPath)
    {
        try
        {
            var pluginInfo = await GetPluginAsync(pluginId);
            if (pluginInfo?.DownloadUrl == null)
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

            var fileName = Path.Combine(downloadPath, $"{pluginInfo.Name}.{version}.dll");
            using var fileStream = File.Create(fileName);
            await response.Content.CopyToAsync(fileStream);

            _logger.LogInformation("Downloaded plugin: {PluginId} v{Version} -> {FilePath}",
                pluginId, version, fileName);

            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading plugin: {PluginId}", pluginId);
            return null;
        }
    }

    /// <summary>
    /// Publishes a plugin to the registry.
    /// </summary>
    public async Task<bool> PublishPluginAsync(string filePath, PluginPublishMetadata metadata)
    {
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
    }

    /// <summary>
    /// Invalidates cached data for a specific plugin.
    /// </summary>
    public void InvalidateCache(Guid pluginId)
    {
        _cache.Remove($"registry_plugin_{pluginId}");
        _cache.Remove($"registry_versions_{pluginId}");
    }
}

/// <summary>
/// Contains version-specific plugin information from registry.
/// </summary>
public class PluginVersionInfo
{
    public required string Version { get; set; }
    public required DateTime PublishedAtUtc { get; set; }
    public required string DownloadUrl { get; set; }
    public bool IsStable { get; set; }
    public bool IsPrerelease { get; set; }
    public string? ReleaseNotes { get; set; }
}

/// <summary>
/// Metadata for publishing a plugin to the registry.
/// </summary>
public class PluginPublishMetadata
{
    public required string PluginName { get; set; }
    public required string Version { get; set; }
    public required string Description { get; set; }
    public required string Author { get; set; }
    public string? Company { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? LicenseType { get; set; }
}
