// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// HTTP client for communicating with remote plugin registries and services.
/// Handles plugin updates, notifications, and metadata synchronization.
/// </summary>
public class HttpPluginClient : IIntegrationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpPluginClient> _logger;
    private readonly string? _registryBaseUrl;

    public string ProviderName => "HttpRegistry";

    public HttpPluginClient(
        HttpClient httpClient,
        ILogger<HttpPluginClient> logger,
        IConfiguration? configuration = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _registryBaseUrl = configuration?["PluginRegistry:BaseUrl"];
    }

    public async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrEmpty(_registryBaseUrl))
            return false;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.GetAsync(_registryBaseUrl + "/health", cts.Token);
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

            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var url = $"{_registryBaseUrl}/plugins/{notification.PluginId}/events";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await _httpClient.PostAsync(url, content, cts.Token);

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
            var response = await _httpClient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve plugin info: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);
            var pluginInfo = System.Text.Json.JsonSerializer.Deserialize<PluginInfo>(content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return pluginInfo;
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
            var response = await _httpClient.PostAsync(url, content, cts.Token);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading plugin: {FilePath}", filePath);
            return false;
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
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var url = $"{_registryBaseUrl}/plugins/check-updates";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await _httpClient.PostAsync(url, content, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                updates = System.Text.Json.JsonSerializer.Deserialize<List<PluginUpdateInfo>>(
                    responseContent,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
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
public class PluginUpdateInfo
{
    public required Guid PluginId { get; set; }
    public required string CurrentVersion { get; set; }
    public required string AvailableVersion { get; set; }
    public required string DownloadUrl { get; set; }
    public bool IsSecurityUpdate { get; set; }
    public string? ReleaseNotes { get; set; }
}
