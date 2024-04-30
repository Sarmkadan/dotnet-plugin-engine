# HttpPluginClient

`HttpPluginClient` is a client class responsible for communicating with a remote plugin repository or update endpoint over HTTP. It provides methods to check plugin availability, retrieve plugin metadata, upload plugins, send notifications, and query for available updates. The class also exposes properties that describe the current plugin identity and the latest available version information retrieved from the remote service.

## API

### Constructors

#### `public HttpPluginClient`

Creates a new instance of the client. The specific constructor signature (e.g., accepting a base URL, authentication credentials, or an `HttpClient` instance) is defined by the implementation and is not listed among the documented public members; refer to the source for exact parameters.

### Properties

#### `public required Guid PluginId`

The unique identifier of the plugin this client instance represents. This value is required and must be set during initialization. It is used in all subsequent API calls to identify the plugin to the remote service.

#### `public required string CurrentVersion`

The version string of the plugin as currently installed or deployed. This is required and is typically sent to the remote service when checking for updates or uploading metadata.

#### `public required string AvailableVersion`

The version string of the latest available plugin release, as reported by the remote service after a successful update check. This property is required and should be populated with the result from `CheckForUpdatesAsync` or a similar operation.

#### `public required string DownloadUrl`

The URL from which the latest available plugin package can be downloaded. This property is required and is typically populated after a successful update check.

#### `public bool IsSecurityUpdate`

Indicates whether the available update is classified as a security update. This flag is set based on the response from the remote service during an update check.

#### `public string? ReleaseNotes`

Release notes associated with the available update. This property may be `null` if no release notes are provided by the remote service.

### Methods

#### `public Task<HttpResponseMessage> GetAsync`

Sends an HTTP GET request to a resource endpoint. The exact URI is determined by the implementation. Returns the raw `HttpResponseMessage` from the server. Throws standard `HttpRequestException` or `TaskCanceledException` on network failures or timeouts.

#### `public async Task<bool> IsAvailableAsync`

Checks whether the plugin identified by `PluginId` is recognized and available on the remote service. Returns `true` if the remote service confirms availability; otherwise `false`. May throw exceptions on connectivity issues or unexpected server responses (e.g., non-2xx status codes not explicitly handled).

#### `public async Task SendNotificationAsync`

Sends a notification to the remote service, typically used to inform the server of plugin state changes, installation events, or health status. The exact payload is determined by the implementation. Throws on network errors or server rejection.

#### `public async Task<PluginInfo?> GetPluginInfoAsync`

Retrieves metadata for the plugin identified by `PluginId` from the remote service. Returns a `PluginInfo` object if the plugin is found; returns `null` if the server indicates the plugin does not exist or is not accessible. Throws on transport failures or malformed responses.

#### `public async Task<bool> UploadPluginAsync`

Uploads the plugin package or metadata to the remote service. Returns `true` if the upload is accepted and successful; otherwise `false`. Throws on network errors, authentication failures, or server-side rejection that results in an exception.

#### `public async Task<List<PluginUpdateInfo>> CheckForUpdatesAsync`

Queries the remote service for available updates based on the current `PluginId` and `CurrentVersion`. Returns a list of `PluginUpdateInfo` objects describing available updates, ordered by the server. If no updates are available, the list is empty. Throws on connectivity failures or unexpected server errors.

## Usage

### Example 1: Checking Plugin Availability and Retrieving Info

```csharp
var client = new HttpPluginClient
{
    PluginId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
    CurrentVersion = "1.0.0",
    AvailableVersion = "",
    DownloadUrl = ""
};

bool isAvailable = await client.IsAvailableAsync();
if (isAvailable)
{
    PluginInfo? info = await client.GetPluginInfoAsync();
    if (info != null)
    {
        Console.WriteLine($"Plugin name: {info.Name}, Version: {info.Version}");
    }
}
else
{
    Console.WriteLine("Plugin not found on remote service.");
}
```

### Example 2: Checking for Updates and Handling a Security Update

```csharp
var client = new HttpPluginClient
{
    PluginId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
    CurrentVersion = "2.1.0",
    AvailableVersion = "",
    DownloadUrl = ""
};

List<PluginUpdateInfo> updates = await client.CheckForUpdatesAsync();
if (updates.Count > 0)
{
    var latest = updates[0];
    client.AvailableVersion = latest.Version;
    client.DownloadUrl = latest.DownloadUrl;
    client.IsSecurityUpdate = latest.IsSecurityUpdate;
    client.ReleaseNotes = latest.ReleaseNotes;

    if (latest.IsSecurityUpdate)
    {
        Console.WriteLine($"Critical security update available: {latest.Version}");
        Console.WriteLine($"Download: {latest.DownloadUrl}");
    }
}
```

## Notes

- All properties marked `required` must be initialized before the client is used; failure to do so will result in initialization errors.
- `IsAvailableAsync` and `GetPluginInfoAsync` may return `false` or `null` respectively when the plugin is not found; callers should handle these cases without treating them as exceptional conditions.
- `CheckForUpdatesAsync` returns an empty list when no updates are available; a non-empty list does not guarantee that the first entry is the most recent—ordering depends on the server implementation.
- Network-related exceptions (e.g., `HttpRequestException`, `TaskCanceledException`) can be thrown by any method that performs HTTP communication. Callers should implement appropriate retry and timeout strategies.
- This class is not inherently thread-safe. If multiple threads access the same instance concurrently, external synchronization is required, especially when updating properties like `AvailableVersion` or `DownloadUrl` based on async operation results.
