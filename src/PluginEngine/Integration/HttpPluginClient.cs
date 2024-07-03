#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// HTTP client for communicating with remote plugin registries and services.
/// Handles plugin updates, notifications, and metadata synchronization.
/// </summary>
public sealed class HttpPluginClient : IIntegrationClient
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal readonly HttpClient _httpClient;
    internal readonly ILogger<HttpPluginClient> _logger;
    internal readonly string? _registryBaseUrl;

    public required Guid PluginId { get; set; }
    public required string CurrentVersion { get; set; } = string.Empty;
    public required string AvailableVersion { get; set; } = string.Empty;
    public required string DownloadUrl { get; set; } = string.Empty;
    public bool IsSecurityUpdate { get; set; }
    public string? ReleaseNotes { get; set; }

    public string ProviderName => "HttpRegistry";

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpPluginClient"/> class.
    /// </summary>
    /// <param name="httpClient">The underlying HTTP client.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">Optional configuration providing <c>PluginRegistry:BaseUrl</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> or <paramref name="logger"/> is null.</exception>
    public HttpPluginClient(
        HttpClient httpClient,
        ILogger<HttpPluginClient> logger,
        IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _logger = logger;
        _registryBaseUrl = configuration?["PluginRegistry:BaseUrl"];
    }

    public Task<HttpResponseMessage> GetAsync(string url)
        => _httpClient.GetAsync(url);

    public async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrEmpty(_registryBaseUrl))
            return false;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var response = await _httpClient.GetAsync(_registryBaseUrl + "/health", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Registry availability check failed");
            return false;
        }
    }

    public async Task SendNotificationAsync(string eventType, PluginNotification notification)
    {
        if (string.IsNullOrEmpty(_registryBaseUrl))
        {
            _logger.LogDebug("Registry not configured, skipping notification");
            return;
        }

        try
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                notification.PluginId,
                notification.PluginName,
                eventType,
                notification.OccurredAtUtc,
                notification.Metadata
            });

            using var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var url = $"{_registryBaseUrl}/plugins/{notification.PluginId}/events";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var response = await _httpClient.PostAsync(url, content, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to send plugin notification: {StatusCode} - {ReasonPhrase}",
                    response.StatusCode,
                    response.ReasonPhrase);
            }
            else
            {
                _logger.LogInformation("Sent plugin event notification: {PluginName} - {EventType}",
                    notification.PluginName, eventType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending plugin notification for {PluginName}", notification.PluginName);
        }
    }

    public async Task<PluginInfo?> GetPluginInfoAsync(Guid pluginId)
    {
        if (string.IsNullOrEmpty(_registryBaseUrl))
        {
            _logger.LogDebug("Registry not configured, cannot retrieve plugin info");
            return null;
        }

        try
        {
            var url = $"{_registryBaseUrl}/plugins/{pluginId}";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var response = await _httpClient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve plugin info: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);
            return System.Text.Json.JsonSerializer.Deserialize<PluginInfo>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plugin info for {PluginId}", pluginId);
            return null;
        }
    }

    /// <summary>
    /// Uploads a plugin to the remote registry.
    /// </summary>
    public async Task<bool> UploadPluginAsync(string filePath, string description = "")
    {
        if (string.IsNullOrEmpty(_registryBaseUrl) || !File.Exists(filePath))
            return false;

        try
        {
            using var fileStream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));
            content.Add(new StringContent(description), "description");

            var url = $"{_registryBaseUrl}/plugins/upload";

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            using var response = await _httpClient.PostAsync(url, content, cts.Token);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading plugin: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// Searches the remote registry for plugins matching a free-text query.
    /// </summary>
    /// <param name="query">The search query. Cannot be null or whitespace.</param>
    /// <param name="limit">Maximum number of results to return. Must be greater than zero.</param>
    /// <returns>The matching plugins, or an empty list when the registry is not configured or unreachable.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limit"/> is not positive.</exception>
    public async Task<List<PluginInfo>> SearchPluginsAsync(string query, int limit = 20)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        if (string.IsNullOrEmpty(_registryBaseUrl))
        {
            _logger.LogDebug("Registry not configured, cannot search plugins");
            return [];
        }

        try
        {
            var url = $"{_registryBaseUrl}/plugins/search" +
                      $"?query={Uri.EscapeDataString(query)}" +
                      $"&limit={limit.ToString(CultureInfo.InvariantCulture)}";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var response = await _httpClient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Plugin search failed: {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);
            return System.Text.Json.JsonSerializer.Deserialize<List<PluginInfo>>(content, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching plugins for query: {Query}", query);
            return [];
        }
    }

    /// <summary>
    /// Retrieves all published versions of a plugin from the remote registry.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The published versions, or an empty list when the registry is not configured or unreachable.</returns>
    public async Task<List<PluginVersionInfo>> GetPluginVersionsAsync(Guid pluginId)
    {
        if (string.IsNullOrEmpty(_registryBaseUrl))
        {
            _logger.LogDebug("Registry not configured, cannot retrieve plugin versions");
            return [];
        }

        try
        {
            var url = $"{_registryBaseUrl}/plugins/{pluginId}/versions";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var response = await _httpClient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve plugin versions: {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);
            return System.Text.Json.JsonSerializer.Deserialize<List<PluginVersionInfo>>(content, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving versions for plugin: {PluginId}", pluginId);
            return [];
        }
    }

    /// <summary>
    /// Checks for plugin updates from the registry.
    /// </summary>
    public async Task<List<PluginUpdateInfo>> CheckForUpdatesAsync(List<Guid> pluginIds)
    {
        var updates = new List<PluginUpdateInfo>();

        if (string.IsNullOrEmpty(_registryBaseUrl))
            return updates;

        try
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(pluginIds);
            using var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var url = $"{_registryBaseUrl}/plugins/check-updates";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var response = await _httpClient.PostAsync(url, content, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                updates = System.Text.Json.JsonSerializer.Deserialize<List<PluginUpdateInfo>>(
                    responseContent, JsonOptions) ?? [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for plugin updates");
        }

        return updates;
    }
}

/// <summary>
/// Represents available update information for a plugin.
/// </summary>
public sealed class PluginUpdateInfo
{
    public required Guid PluginId { get; set; }
    public required string CurrentVersion { get; set; }
    public required string AvailableVersion { get; set; }
    public required string DownloadUrl { get; set; }
    public bool IsSecurityUpdate { get; set; }
    public string? ReleaseNotes { get; set; }
}
