# HttpPluginClientExtensions

Provides a set of static extension methods for `HttpRequestMessage` that simplify interaction with a plugin engine server. These methods handle common tasks such as attaching authentication headers, constructing requests, uploading plugin metadata, and checking for security updates, allowing callers to focus on business logic rather than low-level HTTP details.

## API

### `AddPluginAuthHeaders`

```csharp
public static HttpRequestMessage AddPluginAuthHeaders(
    this HttpRequestMessage request,
    string pluginId,
    string apiKey)
```

Adds authentication headers to an existing `HttpRequestMessage` based on the specified plugin identity and API key.

- **Parameters**  
  - `request` (`HttpRequestMessage`): The HTTP request to modify.  
  - `pluginId` (`string`): The unique identifier of the plugin.  
  - `apiKey` (`string`): The API key used for authentication.

- **Returns**  
  The same `HttpRequestMessage` instance with the authentication headers applied, enabling method chaining.

- **Exceptions**  
  - `ArgumentNullException`: Thrown if `request`, `pluginId`, or `apiKey` is `null`.  
  - `InvalidOperationException`: Thrown if the request already contains conflicting authentication headers.

---

### `CreateRequest`

```csharp
public static HttpRequestMessage CreateRequest(
    this HttpRequestMessage request,
    string pluginId,
    HttpMethod method,
    string relativeUri)
```

Creates a new `HttpRequestMessage` configured for a specific plugin endpoint. The method sets the request URI, HTTP method, and adds the plugin identifier as a header.

- **Parameters**  
  - `request` (`HttpRequestMessage`): The base request used to derive the new request (typically a pre-configured instance with base address).  
  - `pluginId` (`string`): The plugin identifier to include in the request headers.  
  - `method` (`HttpMethod`): The HTTP method for the request (e.g., `GET`, `POST`).  
  - `relativeUri` (`string`): The relative path of the endpoint (e.g., `"metadata"`).

- **Returns**  
  A new `HttpRequestMessage` with the specified method, combined URI, and plugin header.

- **Exceptions**  
  - `ArgumentNullException`: Thrown if any parameter is `null`.  
  - `UriFormatException`: Thrown if `relativeUri` is not a valid relative URI.

---

### `UploadPluginMetadataAsync`

```csharp
public static async Task<bool> UploadPluginMetadataAsync(
    this HttpRequestMessage request,
    PluginMetadata metadata,
    CancellationToken cancellationToken = default)
```

Asynchronously uploads plugin metadata to the server. The metadata is serialized and sent as the request body.

- **Parameters**  
  - `request` (`HttpRequestMessage`): The request to send (should already be configured with the correct endpoint and authentication).  
  - `metadata` (`PluginMetadata`): The metadata object to upload.  
  - `cancellationToken` (`CancellationToken`): Optional token to cancel the operation.

- **Returns**  
  `true` if the upload succeeded (HTTP 2xx status); otherwise `false`.

- **Exceptions**  
  - `ArgumentNullException`: Thrown if `request` or `metadata` is `null`.  
  - `HttpRequestException`: Thrown on network failures or non-success status codes when the response cannot be parsed.  
  - `OperationCanceledException`: Thrown if the operation is cancelled via the token.

---

### `HasSecurityUpdateAsync`

```csharp
public static async Task<bool> HasSecurityUpdateAsync(
    this HttpRequestMessage request,
    string pluginId,
    CancellationToken cancellationToken = default)
```

Asynchronously checks whether a security update is available for the specified plugin.

- **Parameters**  
  - `request` (`HttpRequestMessage`): The request to send (should include base address and authentication).  
  - `pluginId` (`string`): The plugin identifier to check.  
  - `cancellationToken` (`CancellationToken`): Optional token to cancel the operation.

- **Returns**  
  `true` if a security update is available; otherwise `false`.

- **Exceptions**  
  - `ArgumentNullException`: Thrown if `request` or `pluginId` is `null`.  
  - `HttpRequestException`: Thrown on network failures or unexpected server responses.  
  - `OperationCanceledException`: Thrown if the operation is cancelled.

## Usage

### Example 1: Creating and sending an authenticated request

```csharp
using System.Net.Http;
using System.Threading.Tasks;

public async Task<bool> CheckPluginStatusAsync(string pluginId, string apiKey)
{
    using var client = new HttpClient();
    using var baseRequest = new HttpRequestMessage();
    baseRequest.RequestUri = new Uri("https://plugins.example.com/api/");

    var request = baseRequest
        .CreateRequest(pluginId, HttpMethod.Get, "status")
        .AddPluginAuthHeaders(pluginId, apiKey);

    var response = await client.SendAsync(request);
    return response.IsSuccessStatusCode;
}
```

### Example 2: Uploading metadata and checking for updates

```csharp
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public async Task<bool> PublishAndCheckAsync(
    HttpClient client,
    string pluginId,
    string apiKey,
    PluginMetadata metadata,
    CancellationToken ct)
{
    using var baseRequest = new HttpRequestMessage();
    baseRequest.RequestUri = new Uri("https://plugins.example.com/api/");

    var uploadRequest = baseRequest
        .CreateRequest(pluginId, HttpMethod.Post, "metadata")
        .AddPluginAuthHeaders(pluginId, apiKey);

    bool uploadOk = await uploadRequest.UploadPluginMetadataAsync(metadata, ct);
    if (!uploadOk) return false;

    var checkRequest = baseRequest
        .CreateRequest(pluginId, HttpMethod.Get, "security")
        .AddPluginAuthHeaders(pluginId, apiKey);

    return await checkRequest.HasSecurityUpdateAsync(pluginId, ct);
}
```

## Notes

- All methods are static and do not maintain internal state; they are thread-safe when called with distinct `HttpRequestMessage` instances. However, `HttpRequestMessage` itself is not thread-safe, so concurrent modification of the same request object must be avoided.
- Passing `null` for any required parameter will result in an `ArgumentNullException`. Always validate inputs before calling these methods.
- Network-related exceptions (`HttpRequestException`, `OperationCanceledException`) should be handled by the caller, especially in long-running or cancellation-sensitive scenarios.
- The `UploadPluginMetadataAsync` method expects the `request` to already point to the correct upload endpoint; it does not modify the request URI. Similarly, `HasSecurityUpdateAsync` relies on the request’s base address and the provided `pluginId` to construct the query.
- When using `CreateRequest`, the returned `HttpRequestMessage` is a new instance and should be disposed appropriately (e.g., via `using`). The original `request` parameter is not modified.
