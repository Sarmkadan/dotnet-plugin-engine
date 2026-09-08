# IRemotePluginRegistry

`IRemotePluginRegistry` defines asynchronous operations for finding, inspecting,
downloading, and publishing plugins in a remote registry. It is declared in the
`PluginEngine.Integration` namespace.

## Method signatures

| Method | Parameters | Return value |
| --- | --- | --- |
| `SearchAsync` | `string query`, `int limit = 20` | `Task<List<PluginInfo>>` containing the matching plugins |
| `GetPluginAsync` | `Guid pluginId` | `Task<PluginInfo?>`; the result may be `null` |
| `GetVersionsAsync` | `Guid pluginId` | `Task<List<PluginVersionInfo>>` containing the available versions |
| `DownloadPluginAsync` | `Guid pluginId`, `string version`, `string downloadPath` | `Task<string?>`; a non-null result is the downloaded file path |
| `PublishPluginAsync` | `string filePath`, `PluginPublishMetadata metadata` | `Task<bool>` indicating whether publishing succeeded |

The interface does not define cancellation-token parameters, result ordering, or
exception behavior. Those details are implementation-specific.

## Search and download example

The following example searches for a plugin and downloads the version reported
by the first search result. Search results are not guaranteed to be ordered, so
production code should select a result using criteria appropriate to the caller.

```csharp
using PluginEngine.Integration;

static async Task<string?> FindAndDownloadAsync(
    IRemotePluginRegistry registry,
    string destinationDirectory)
{
    List<PluginInfo> matches = await registry.SearchAsync("analytics", limit: 10);
    PluginInfo? plugin = matches.FirstOrDefault();

    if (plugin is null)
    {
        return null;
    }

    return await registry.DownloadPluginAsync(
        plugin.Id,
        plugin.Version,
        destinationDirectory);
}
```

Callers must handle both an empty search result and a `null` download result.

## Concrete `RemotePluginRegistry` behavior

The included `RemotePluginRegistry` implementation validates that the search
query is not blank and that the limit is positive. It caches searches and
version lists, and it caches non-null plugin lookups.

For downloads, the implementation:

1. Looks up the plugin by ID and uses its `PluginInfo.DownloadUrl`.
2. Returns `null` if the plugin has no download URL or the HTTP response is not
   successful.
3. Creates the destination directory when needed.
4. Writes the response to `<plugin name>.<requested version>.dll` and returns
   that path.
5. Catches download errors, logs them, and returns `null`.

The `version` argument contributes to the output filename; the implementation
does not use it to choose a version-specific download URL.

When publishing, `RemotePluginRegistry` returns `false` if the file does not
exist or if publishing throws. It sends the file together with a description
formed from `PluginPublishMetadata.Description` and
`PluginPublishMetadata.Author`. The other metadata properties are not forwarded
by this implementation.

## PluginVersionInfo

`PluginVersionInfo` contains registry information about one plugin version.

| Property | Type | Description |
| --- | --- | --- |
| `Version` | `required string` | Version text supplied by the registry |
| `PublishedAtUtc` | `required DateTime` | UTC publication timestamp |
| `DownloadUrl` | `required string` | Download URL supplied for the version |
| `IsStable` | `bool` | Indicates whether the version is marked stable |
| `IsPrerelease` | `bool` | Indicates whether the version is marked as a prerelease |
| `ReleaseNotes` | `string?` | Optional release notes |

The class does not enforce a relationship between `IsStable` and
`IsPrerelease`; consumers should not assume that setting one changes the other.

## PluginPublishMetadata

`PluginPublishMetadata` describes a plugin being published.

| Property | Type | Description |
| --- | --- | --- |
| `PluginName` | `required string` | Plugin name |
| `Version` | `required string` | Version text |
| `Description` | `required string` | Plugin description |
| `Author` | `required string` | Plugin author |
| `Company` | `string?` | Optional company name |
| `Tags` | `List<string>` | Tags; initialized to an empty list |
| `LicenseType` | `string?` | Optional license description or identifier |

The `required` modifier requires callers to initialize the four required
properties when constructing an instance. The model itself contains no methods
and performs no validation.

```csharp
var metadata = new PluginPublishMetadata
{
    PluginName = "Analytics.Plugin",
    Version = "1.2.0",
    Description = "Adds analytics reporting",
    Author = "Example Team",
    Company = "Example Company",
    Tags = ["analytics", "reporting"],
    LicenseType = "MIT"
};

bool published = await registry.PublishPluginAsync(
    "./artifacts/Analytics.Plugin.dll",
    metadata);
```
