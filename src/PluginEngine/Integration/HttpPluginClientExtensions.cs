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
/// Extension methods for <see cref="HttpPluginClient"/> providing enhanced functionality for
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
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
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
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="relativeUrl"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static HttpRequestMessage CreateRequest(
        this HttpPluginClient client,
        HttpMethod method,
        string relativeUrl,
        HttpContent? content = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(relativeUrl);

        var baseUrl = client._registryBaseUrl;
        var requestUri = new Uri(baseUrl + relativeUrl, UriKind.Absolute);

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
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="pluginInfo"/> is <see langword="null"/>.</exception>
    public static async Task<bool> UploadPluginMetadataAsync(
        this HttpPluginClient client,
        PluginInfo pluginInfo)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(pluginInfo);

        var baseUrl = client._registryBaseUrl;
        if (string.IsNullOrEmpty(baseUrl))
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
            var response = await client._httpClient.SendAsync(request, cts.Token);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            var logger = client._logger;
            logger?.LogError(ex, "Error uploading plugin metadata for {PluginName}", pluginInfo.Name);
            return false;
        }
    }

    /// <summary>
    /// Checks if a specific plugin has a security update available.
    /// </summary>
    /// <param name="client">The HttpPluginClient instance.</param>
    /// <param name="pluginId">The ID of the plugin to check.</param>
    /// <returns>True if a security update is available; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async Task<bool> HasSecurityUpdateAsync(
        this HttpPluginClient client,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(client);

        var updates = await client.CheckForUpdatesAsync(new List<Guid> { pluginId });
        return updates.Count > 0 && updates[0].IsSecurityUpdate;
    }
}
