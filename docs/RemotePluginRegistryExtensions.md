# RemotePluginRegistryExtensions

Provides a set of extension methods for `IRemotePluginRegistry` that simplify common remote plugin lookup and download operations. These methods encapsulate asynchronous HTTP calls to a remote plugin repository and return strongly typed results or `null` when no matching data is found.

## API

### `SearchByNameOrTagAsync`

```csharp
public static async Task<List<PluginInfo>> SearchByNameOrTagAsync(
    this IRemotePluginRegistry registry,
    string searchTerm)
```

Searches the remote registry for plugins whose name or tag contains the specified `searchTerm`. The search is case-insensitive and matches substrings.

- **Parameters**  
  - `registry` – The remote registry instance.  
  - `searchTerm` – The term to search for in plugin names and tags. Must not be `null` or empty.

- **Returns**  
  A `Task<List<PluginInfo>>` that resolves to a list of matching `PluginInfo` objects. If no matches are found, the list is empty.

- **Throws**  
  - `ArgumentNullException` – if `registry` is `null`.  
  - `ArgumentException` – if `searchTerm` is `null` or empty.  
  - `HttpRequestException` – if the remote registry is unreachable or returns an error status code.

### `GetLatestStableVersionAsync`

```csharp
public static async Task<PluginVersionInfo?> GetLatestStableVersionAsync(
    this IRemotePluginRegistry registry,
    string pluginName)
```

Retrieves the latest stable (non-prerelease) version of the plugin identified by `pluginName`.

- **Parameters**  
  - `registry` – The remote registry instance.  
  - `pluginName` – The exact name of the plugin. Must not be `null` or empty.

- **Returns**  
  A `Task<PluginVersionInfo?>` that resolves to the latest stable version information, or `null` if no stable version exists or the plugin is not found.

- **Throws**  
  - `ArgumentNullException` – if `registry` is `null`.  
  - `ArgumentException` – if `pluginName` is `null` or empty.  
  - `HttpRequestException` – if the remote registry is unreachable or returns an error status code.

### `GetLatestPrereleaseVersionAsync`

```csharp
public static async Task<PluginVersionInfo?> GetLatestPrereleaseVersionAsync(
    this IRemotePluginRegistry registry,
    string pluginName)
```

Retrieves the latest prerelease version (e.g., alpha, beta, rc) of the plugin identified by `pluginName`. If no prerelease version exists, returns the latest stable version instead.

- **Parameters**  
  - `registry` – The remote registry instance.  
  - `pluginName` – The exact name of the plugin. Must not be `null` or empty.

- **Returns**  
  A `Task<PluginVersionInfo?>` that resolves to the latest prerelease version information, or `null` if no version (stable or prerelease) exists for the plugin.

- **Throws**  
  - `ArgumentNullException` – if `registry` is `null`.  
  - `ArgumentException` – if `pluginName` is `null` or empty.  
  - `HttpRequestException` – if the remote registry is unreachable or returns an error status code.

### `DownloadLatestStableAsync`

```csharp
public static async Task<string?> DownloadLatestStableAsync(
    this IRemotePluginRegistry registry,
    string pluginName)
```

Downloads the latest stable version of the plugin to a temporary location and returns the file path of the downloaded package.

- **Parameters**  
  - `registry` – The remote registry instance.  
  - `pluginName` – The exact name of the plugin. Must not be `null` or empty.

- **Returns**  
  A `Task<string?>` that resolves to the absolute file path of the downloaded plugin package, or `null` if no stable version exists or the download fails.

- **Throws**  
  - `ArgumentNullException` – if `registry` is `null`.  
  - `ArgumentException` – if `pluginName` is `null` or empty.  
  - `HttpRequestException` – if the remote registry is unreachable or returns an error status code.  
  - `IOException` – if the file cannot be written to the temporary directory.

## Usage

### Example 1: Search for plugins and download the latest stable version

```csharp
using PluginEngine;
using PluginEngine.Remote;

public async Task DownloadFirstMatchingPluginAsync(IRemotePluginRegistry registry)
{
    var results = await registry.SearchByNameOrTagAsync("logging");
    if (results.Count == 0)
    {
        Console.WriteLine("No plugins found.");
        return;
    }

    var firstPlugin = results[0];
    var filePath = await registry.DownloadLatestStableAsync(firstPlugin.Name);
    if (filePath != null)
    {
        Console.WriteLine($"Downloaded to {filePath}");
    }
}
```

### Example 2: Retrieve latest prerelease version information

```csharp
using PluginEngine;
using PluginEngine.Remote;

public async Task ShowLatestPrereleaseAsync(IRemotePluginRegistry registry)
{
    var versionInfo = await registry.GetLatestPrereleaseVersionAsync("MyPlugin");
    if (versionInfo == null)
    {
        Console.WriteLine("No version found for MyPlugin.");
        return;
    }

    Console.WriteLine($"Version: {versionInfo.Version}, Released: {versionInfo.ReleaseDate}");
}
```

## Notes

- All methods are extension methods on `IRemotePluginRegistry` and are intended to be used with a properly configured registry instance (e.g., one that has an `HttpClient` with a base address set).
- The `searchTerm` parameter in `SearchByNameOrTagAsync` performs a substring match; passing a very short or common term may return a large result set.
- `GetLatestPrereleaseVersionAsync` falls back to the latest stable version if no prerelease exists. This behavior is by design and may cause confusion if a plugin has only stable releases.
- `DownloadLatestStableAsync` writes the downloaded file to the system’s temporary directory (as returned by `Path.GetTempPath()`). The caller is responsible for managing the file’s lifecycle.
- Network failures, timeouts, or invalid plugin names will cause an `HttpRequestException`. The caller should handle these exceptions appropriately.
- These methods are thread-safe in the sense that they do not modify any shared state. However, the underlying `IRemotePluginRegistry` implementation (e.g., its `HttpClient`) must be used in a thread-safe manner. Concurrent calls to these methods on the same registry instance are safe as long as the registry itself is designed for concurrent use.
