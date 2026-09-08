# PluginMarketplaceService

`PluginMarketplaceService` is the default `IPluginMarketplaceService` implementation. It uses an `IRemotePluginRegistry` to search for, inspect, and download plugins, an `IMemoryCache` to cache compatibility matrices, and an `ILogger<PluginMarketplaceService>` for diagnostics.

The constructor rejects a `null` registry, cache, or logger with `ArgumentNullException`.

## Public API

| Method | Signature | Behavior |
| --- | --- | --- |
| `SearchAsync` | `Task<PluginOperationResult<List<MarketplaceEntry>>> SearchAsync(MarketplaceSearchFilter filter, CancellationToken cancellationToken = default)` | Searches the registry and maps each returned `PluginInfo` to a marketplace entry. |
| `GetEntryAsync` | `Task<PluginOperationResult<MarketplaceEntry>> GetEntryAsync(Guid pluginId, CancellationToken cancellationToken = default)` | Retrieves a plugin and, when found, attaches its complete registry version list. |
| `GetCompatibilityMatrixAsync` | `Task<PluginOperationResult<VersionCompatibilityMatrix>> GetCompatibilityMatrixAsync(Guid pluginId, CancellationToken cancellationToken = default)` | Returns a cached matrix or builds one for engine versions `8.0`, `9.0`, and `10.0`. |
| `CheckCompatibilityAsync` | `Task<PluginOperationResult<CompatibilityStatus>> CheckCompatibilityAsync(Guid pluginId, string pluginVersion, string engineVersion, CancellationToken cancellationToken = default)` | Gets the matrix and looks up one plugin-version/engine-version pair. |
| `InstallAsync` | `Task<PluginOperationResult> InstallAsync(Guid pluginId, string version, string targetDirectory, CancellationToken cancellationToken = default)` | Ensures the target directory exists, then asks the registry to download the requested version into it. |

Although every method accepts a `CancellationToken`, the registry interface used by this implementation has no cancellation-token parameters. Consequently, `SearchAsync`, `GetEntryAsync`, `GetCompatibilityMatrixAsync`, and `InstallAsync` do not observe or forward their token. `CheckCompatibilityAsync` passes its token to `GetCompatibilityMatrixAsync`, where it is likewise not observed.

## SearchAsync

`SearchAsync` calls `IRemotePluginRegistry.SearchAsync` with two values:

- `filter.Query`, or an empty string when the query is `null`.
- `filter.PageSize` as the result limit.

It does not use `Tags`, `MinVersion`, `TargetFramework`, `OnlyVerified`, `Page`, or `SortOrder`. Each registry `PluginInfo` is mapped to `Id`, `Name`, `LatestVersion`, `Author`, and `Description`; null author and description values become empty strings. Other `MarketplaceEntry` members retain their model defaults.

Success returns the mapped list and the message `Found {count} plugin(s).`. Any exception is logged and converted with `PluginOperationResult<List<MarketplaceEntry>>.FromException`.

## GetEntryAsync

The method first calls `GetPluginAsync(pluginId)`. A null result produces a failure with message `Plugin not found in the marketplace.` and error code `404`, without requesting versions.

For a found plugin, it calls `GetVersionsAsync(pluginId)`, performs the same entry mapping as search, assigns the returned list to `AvailableVersions`, and returns success with `Plugin entry retrieved.`. Exceptions are logged and converted with `FromException`.

## Compatibility matrix and cache

`GetCompatibilityMatrixAsync` uses the cache key `mp_matrix_{pluginId}`. A non-null `VersionCompatibilityMatrix` already stored under that key is returned immediately with `Matrix served from cache.`.

On a cache miss, the service requests both the plugin information and version list. It creates a matrix whose `PluginId` is the requested ID and whose `PluginName` is the registry name, or `pluginId.ToString()` when the registry returns no plugin. Every returned plugin version is evaluated against exactly these known engine-version strings:

- `8.0`
- `9.0`
- `10.0`

The completed matrix is cached with an absolute expiration relative to now of 30 minutes. Cache hits do not extend that expiration. Only a successfully built matrix is cached; exceptions are logged and returned through `FromException`. A newly generated matrix returns `Compatibility matrix generated.`.

## Compatibility heuristic

The heuristic evaluates conditions in this order:

1. If `engineVersion` cannot be parsed by `Version.TryParse`, the result is `Unknown`.
2. If `PluginVersionInfo.IsPrerelease` is `true`, the result is `Compatible` when the parsed engine major is at least 10; otherwise it is `Unknown`. This branch takes precedence over `IsStable`.
3. If `IsStable` is `false`, the result is `Unknown`.
4. For a stable, non-prerelease version, age is `(DateTime.UtcNow - PublishedAtUtc).TotalDays` and the following thresholds apply.

| Publication age | Engine major | Status |
| --- | --- | --- |
| `<= 365` days | Any parsed major | `Compatible` |
| `> 365` and `<= 730` days | `>= 9` | `Compatible` |
| `> 365` and `<= 730` days | `< 9` | `Deprecated` |
| `> 730` days | `>= 10` | `Deprecated` |
| `> 730` days | `< 10` | `Incompatible` |

Because the comparisons are inclusive, a version exactly 365 days old is in the first band and one exactly 730 days old is in the second band. A future publication date has a negative age and is therefore also in the `<= 365` band.

## CheckCompatibilityAsync

This method obtains the matrix through `GetCompatibilityMatrixAsync`, so it shares the same cache and heuristic. If matrix generation fails, it returns a failure with the matrix result's message and its error code, defaulting to `500` only when that error code is null.

On success it calls `VersionCompatibilityMatrix.GetStatus(pluginVersion, engineVersion)`. Matrix keys are compared case-insensitively. A plugin version, engine version, or pairing not recorded in the matrix returns `Unknown`. The successful message is `Compatibility status: {status}.`.

## InstallAsync

If `targetDirectory` does not exist, the method creates it. It then calls `DownloadPluginAsync(pluginId, version, targetDirectory)`. It does not call the compatibility APIs or perform any additional validation or extraction.

A null path from the registry produces error code `502` with `Download failed: the registry returned no file.`. A non-null path produces `Plugin installed successfully at: {filePath}`. Exceptions from directory creation or download are logged and converted with `PluginOperationResult.FromException`.

## Example

The service can be resolved through its interface and used to check compatibility before requesting installation:

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Marketplace;
using PluginEngine.Results;

IPluginMarketplaceService marketplace =
    serviceProvider.GetRequiredService<IPluginMarketplaceService>();

Guid pluginId = Guid.Parse("4acfd7bb-5e39-4388-924d-f8c476d650f1");
var compatibility = await marketplace.CheckCompatibilityAsync(
    pluginId,
    pluginVersion: "2.1.0",
    engineVersion: "10.0");

if (compatibility.Success &&
    compatibility.Data == CompatibilityStatus.Compatible)
{
    PluginOperationResult install = await marketplace.InstallAsync(
        pluginId,
        version: "2.1.0",
        targetDirectory: "./plugins");

    Console.WriteLine(install.Message);
}
```

The compatibility check in this example is caller policy. `InstallAsync` itself does not require a compatible status.
