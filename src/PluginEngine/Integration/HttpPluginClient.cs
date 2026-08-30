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
    private const string RegistryBaseUrlConfigKey = "PluginRegistry:BaseUrl";
    private const string HealthRoute = "/health";
    private const string PluginsRoute = "/plugins";
    private const string PluginRouteFormat = PluginsRoute + "/{0}";
    private const string PluginEventsRouteFormat = PluginRouteFormat + "/events";
    private const string PluginVersionsRouteFormat = PluginRouteFormat + "/versions";
    private const string PluginUploadRoute = PluginsRoute + "/upload";
    private const string PluginSearchRoute = PluginsRoute + "/search";
    private const string PluginUpdateCheckRoute = PluginsRoute + "/check-updates";
    private const string JsonContentType = "application/json";

    private static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan NotificationTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PluginInfoTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan UploadTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PluginSearchTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan PluginVersionsTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan UpdateCheckTimeout = TimeSpan.FromSeconds(30);

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
        _registryBaseUrl = configuration?[RegistryBaseUrlConfigKey];
    }

    /// <summary>
    /// Sends a GET request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to send the request to.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The HTTP response message.</returns>
    public Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        return _httpClient.GetAsync(url, cancellationToken);
    }

    /// <summary>
    /// Sends a GET request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to send the request to.</param>
    /// <returns>The HTTP response message.</returns>
    public Task<HttpResponseMessage> GetAsync(string url)
        => GetAsync(url, CancellationToken.None);

    /// <summary>
    /// Checks if the plugin registry is available.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>True if the registry is available; otherwise, false.</returns>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_registryBaseUrl))
            return false;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(HealthCheckTimeout);
            using var response = await _httpClient.GetAsync(_registryBaseUrl + HealthRoute, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            _logger.LogWarning(ex, "Registry availability check failed");
            return false;
        }
    }

    /// <summary>
    /// Checks if the plugin registry is available.
    /// </summary>
    /// <returns>True if the registry is available; otherwise, false.</returns>
    public Task<bool> IsAvailableAsync()
        => IsAvailableAsync(CancellationToken.None);

    /// <summary>
    /// Sends a notification to the plugin registry.
    /// </summary>
    /// <param name="eventType">The type of event.</param>
    /// <param name="notification">The notification details.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public async Task SendNotificationAsync(string eventType, PluginNotification notification, CancellationToken cancellationToken = default)
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

            using var content = new StringContent(payload, System.Text.Encoding.UTF8, JsonContentType);
            var url = _registryBaseUrl + string.Format(
                CultureInfo.InvariantCulture,
                PluginEventsRouteFormat,
                notification.PluginId);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(NotificationTimeout);
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
        catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            _logger.LogError(ex, "Error sending plugin notification for {PluginName}", notification.PluginName);
        }
    }

    /// <summary>
    /// Sends a notification to the plugin registry.
    /// </summary>
    /// <param name="eventType">The type of event.</param>
    /// <param name="notification">The notification details.</param>
    public Task SendNotificationAsync(string eventType, PluginNotification notification)
        => SendNotificationAsync(eventType, notification, CancellationToken.None);

    /// <summary>
    /// Retrieves plugin information from the registry.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The plugin information, or null if not found or an error occurred.</returns>
    public async Task<PluginInfo?> GetPluginInfoAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_registryBaseUrl))
        {
            _logger.LogDebug("Registry not configured, cannot retrieve plugin info");
            return null;
        }

        try
        {
            var url = _registryBaseUrl + string.Format(CultureInfo.InvariantCulture, PluginRouteFormat, pluginId);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(PluginInfoTimeout);
            using var response = await _httpClient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve plugin info: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);
            return System.Text.Json.JsonSerializer.Deserialize<PluginInfo>(content, JsonOptions);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            _logger.LogError(ex, "Error retrieving plugin info for {PluginId}", pluginId);
            return null;
        }
    }

    /// <summary>
    /// Retrieves plugin information from the registry.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The plugin information, or null if not found or an error occurred.</returns>
    public Task<PluginInfo?> GetPluginInfoAsync(Guid pluginId)
        => GetPluginInfoAsync(pluginId, CancellationToken.None);

    /// <summary>
    /// Uploads a plugin to the remote registry.
    /// </summary>
    /// <param name="filePath">The path to the plugin file to upload.</param>
    /// <param name="description">Optional description of the plugin.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>True if the upload was successful; otherwise, false.</returns>
    public async Task<bool> UploadPluginAsync(string filePath, string description = "", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_registryBaseUrl) || !File.Exists(filePath))
            return false;

        try
        {
            using var fileStream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));
            content.Add(new StringContent(description), "description");

            var url = _registryBaseUrl + PluginUploadRoute;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(UploadTimeout);
            using var response = await _httpClient.PostAsync(url, content, cts.Token);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            _logger.LogError(ex, "Error uploading plugin: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// Uploads a plugin to the remote registry.
    /// </summary>
    /// <param name="filePath">The path to the plugin file to upload.</param>
    /// <param name="description">Optional description of the plugin.</param>
    /// <returns>True if the upload was successful; otherwise, false.</returns>
    public Task<bool> UploadPluginAsync(string filePath, string description = "")
        => UploadPluginAsync(filePath, description, CancellationToken.None);

    /// <summary>
    /// Searches the remote registry for plugins matching a free-text query.
    /// </summary>
    /// <param name="query">The search query. Cannot be null or whitespace.</param>
    /// <param name="limit">Maximum number of results to return. Must be greater than zero.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The matching plugins, or an empty list when the registry is not configured or unreachable.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limit"/> is not positive.</exception>
    public async Task<List<PluginInfo>> SearchPluginsAsync(string query, int limit = 20, CancellationToken cancellationToken = default)
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
            var url = _registryBaseUrl + PluginSearchRoute +
                      $"?query={Uri.EscapeDataString(query)}" +
                      $"&limit={limit.ToString(CultureInfo.InvariantCulture)}";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(PluginSearchTimeout);
            using var response = await _httpClient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Plugin search failed: {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);
            return System.Text.Json.JsonSerializer.Deserialize<List<PluginInfo>>(content, JsonOptions) ?? [];
        }
        catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            _logger.LogError(ex, "Error searching plugins for query: {Query}", query);
            return [];
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
    public Task<List<PluginInfo>> SearchPluginsAsync(string query, int limit = 20)
        => SearchPluginsAsync(query, limit, CancellationToken.None);

    /// <summary>
    /// Retrieves all published versions of a plugin from the remote registry.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The published versions, or an empty list when the registry is not configured or unreachable.</returns>
    public async Task<List<PluginVersionInfo>> GetPluginVersionsAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_registryBaseUrl))
        {
            _logger.LogDebug("Registry not configured, cannot retrieve plugin versions");
            return [];
        }

        try
        {
            var url = _registryBaseUrl + string.Format(
                CultureInfo.InvariantCulture,
                PluginVersionsRouteFormat,
                pluginId);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(PluginVersionsTimeout);
            using var response = await _httpClient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve plugin versions: {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);
            return System.Text.Json.JsonSerializer.Deserialize<List<PluginVersionInfo>>(content, JsonOptions) ?? [];
        }
        catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            _logger.LogError(ex, "Error retrieving versions for plugin: {PluginId}", pluginId);
            return [];
        }
    }

    /// <summary>
    /// Retrieves all published versions of a plugin from the remote registry.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The published versions, or an empty list when the registry is not configured or unreachable.</returns>
    public Task<List<PluginVersionInfo>> GetPluginVersionsAsync(Guid pluginId)
        => GetPluginVersionsAsync(pluginId, CancellationToken.None);

    /// <summary>
    /// Checks for plugin updates from the registry.
    /// </summary>
    /// <param name="pluginIds">The plugin identifiers to check for updates.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A list of update information for the plugins.</returns>
    public async Task<List<PluginUpdateInfo>> CheckForUpdatesAsync(List<Guid> pluginIds, CancellationToken cancellationToken = default)
    {
        var updates = new List<PluginUpdateInfo>();

        if (string.IsNullOrEmpty(_registryBaseUrl))
            return updates;

        try
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(pluginIds);
            using var content = new StringContent(payload, System.Text.Encoding.UTF8, JsonContentType);
            var url = _registryBaseUrl + PluginUpdateCheckRoute;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(UpdateCheckTimeout);
            using var response = await _httpClient.PostAsync(url, content, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                updates = System.Text.Json.JsonSerializer.Deserialize<List<PluginUpdateInfo>>(
                    responseContent, JsonOptions) ?? [];
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            _logger.LogError(ex, "Error checking for plugin updates");
        }

        return updates;
    }

    /// <summary>
    /// Checks for plugin updates from the registry.
    /// </summary>
    /// <param name="pluginIds">The plugin identifiers to check for updates.</param>
    /// <returns>A list of update information for the plugins.</returns>
    public Task<List<PluginUpdateInfo>> CheckForUpdatesAsync(List<Guid> pluginIds)
        => CheckForUpdatesAsync(pluginIds, CancellationToken.None);
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
