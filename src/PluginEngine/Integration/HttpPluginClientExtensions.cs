#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace PluginEngine.Integration;

/// <summary>
/// Extension methods for HttpPluginClient providing enhanced functionality for
/// plugin registry operations and HTTP request handling.
/// </summary>
public static class HttpPluginClientExtensions
{
    /// <summary>
    /// Adds standard authentication headers to an HTTP request message for plugin registry operations.
    /// Includes provider information for traceability.
    /// </summary>
    /// <param name="client">The HttpPluginClient instance.</param>
    /// <param name="request">The HTTP request message to add headers to.</param>
    /// <returns>The HttpRequestMessage with authentication headers added.</returns>
    public static HttpRequestMessage AddPluginAuthHeaders(this HttpPluginClient client, HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        request.Headers.Add("X-Plugin-Provider", client.ProviderName);

        return request;
    }

    /// <summary>
    /// Creates and configures an HTTP request message with proper headers and content type.
    /// </summary>
    /// <param name="client">The HttpPluginClient instance.</param>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="relativeUrl">The relative URL for the request.</param>
    /// <param name="content">Optional HTTP content to send with the request.</param>
    /// <returns>A configured HttpRequestMessage ready for sending.</returns>
    public static HttpRequestMessage CreateRequest(
        this HttpPluginClient client,
        HttpMethod method,
        string relativeUrl,
        HttpContent? content = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(relativeUrl);

        var baseUrl = GetRegistryBaseUrl(client);
        var requestUri = new Uri(baseUrl + relativeUrl, UriKind.RelativeOrAbsolute);

        var request = new HttpRequestMessage(method, requestUri)
        {
            Content = content
        };

        return client.AddPluginAuthHeaders(request);
    }

    /// <summary>
    /// Uploads plugin metadata to the registry without a file.
    /// Useful for registering plugins that are already deployed elsewhere.
    /// </summary>
    /// <param name="client">The HttpPluginClient instance.</param>
    /// <param name="pluginInfo">The plugin metadata to register.</param>
    /// <returns>True if the upload was successful; otherwise false.</returns>
    public static async Task<bool> UploadPluginMetadataAsync(
        this HttpPluginClient client,
        PluginInfo pluginInfo)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(pluginInfo);

        if (string.IsNullOrEmpty(GetRegistryBaseUrl(client)))
        {
            return false;
        }

        try
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(pluginInfo);
            var content = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json);

            var url = "/plugins/metadata";
            var request = client.CreateRequest(HttpMethod.Post, url, content);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await client.GetAsync(request.RequestUri?.ToString() ?? url);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            var logger = GetLogger(client);
            if (logger != null)
            {
                logger.LogError(ex, "Error uploading plugin metadata for {PluginName}", pluginInfo.Name);
            }
            return false;
        }
    }

    /// <summary>
    /// Checks if a specific plugin has a security update available.
    /// </summary>
    /// <param name="client">The HttpPluginClient instance.</param>
    /// <param name="pluginId">The ID of the plugin to check.</param>
    /// <returns>True if a security update is available; otherwise false.</returns>
    public static async Task<bool> HasSecurityUpdateAsync(
        this HttpPluginClient client,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(client);

        var updates = await client.CheckForUpdatesAsync(new List<Guid> { pluginId });
        return updates.Count > 0 && updates[0].IsSecurityUpdate;
    }

    /// <summary>
    /// Gets the configured registry base URL from the HttpPluginClient.
    /// </summary>
    /// <param name="client">The HttpPluginClient instance.</param>
    /// <returns>The registry base URL or null if not configured.</returns>
    private static string? GetRegistryBaseUrl(HttpPluginClient client)
    {
        var field = typeof(HttpPluginClient).GetField(
            "_registryBaseUrl",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        return field?.GetValue(client) as string;
    }

    /// <summary>
    /// Gets the logger instance from HttpPluginClient for error logging.
    /// </summary>
    /// <param name="client">The HttpPluginClient instance.</param>
    /// <returns>The ILogger instance or null if not available.</returns>
    private static ILogger<HttpPluginClient>? GetLogger(HttpPluginClient client)
    {
        var field = typeof(HttpPluginClient).GetField(
            "_logger",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        return field?.GetValue(client) as ILogger<HttpPluginClient>;
    }
}
