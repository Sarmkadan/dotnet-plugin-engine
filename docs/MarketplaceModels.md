# Marketplace Models

The types in `PluginEngine.Marketplace` describe marketplace listings, search criteria, and compatibility between published plugin versions and host engine versions.

## Signature summary

| Type | Member | Signature |
|---|---|---|
| `CompatibilityStatus` | Values | `Compatible`, `Incompatible`, `Deprecated`, `Unknown` |
| `MarketplaceSortOrder` | Values | `Relevance`, `Downloads`, `Rating`, `LastUpdated`, `Name` |
| `MarketplaceEntry` | Properties | `Guid Id { get; set; }`, `string Name { get; set; }`, `string LatestVersion { get; set; }`, `string Author { get; set; }`, `string Description { get; set; }` |
| `MarketplaceEntry` | Discovery and statistics | `List<string> Tags { get; set; }`, `long Downloads { get; set; }`, `double Rating { get; set; }`, `bool IsVerified { get; set; }` |
| `MarketplaceEntry` | Publication and source details | `DateTime PublishedAtUtc { get; set; }`, `DateTime UpdatedAtUtc { get; set; }`, `string LicenseType { get; set; }`, `string RepositoryUrl { get; set; }`, `List<PluginVersionInfo> AvailableVersions { get; set; }` |
| `VersionCompatibilityMatrix` | Properties | `Guid PluginId { get; init; }`, `string PluginName { get; init; }`, `DateTime GeneratedAtUtc { get; init; }`, `IReadOnlyDictionary<string, IReadOnlyDictionary<string, CompatibilityStatus>> Entries { get; }` |
| `VersionCompatibilityMatrix` | `Record` | `void Record(string pluginVersion, string engineVersion, CompatibilityStatus status)` |
| `VersionCompatibilityMatrix` | `GetStatus` | `CompatibilityStatus GetStatus(string pluginVersion, string engineVersion)` |
| `VersionCompatibilityMatrix` | `GetCompatiblePluginVersions` | `IReadOnlyList<string> GetCompatiblePluginVersions(string engineVersion)` |
| `MarketplaceSearchFilter` | Properties | `string? Query { get; set; }`, `List<string> Tags { get; set; }`, `string? MinVersion { get; set; }`, `string? TargetFramework { get; set; }`, `bool OnlyVerified { get; set; }`, `int Page { get; set; }`, `int PageSize { get; set; }`, `MarketplaceSortOrder SortOrder { get; set; }` |

## `CompatibilityStatus`

`CompatibilityStatus` describes one plugin-version and engine-version pairing:

- `Compatible`: fully supported and tested.
- `Incompatible`: known to be broken or unsupported.
- `Deprecated`: works, but is no longer maintained or recommended.
- `Unknown`: compatibility has not been verified. `GetStatus` also returns this value when a pairing has not been recorded.

## `MarketplaceSortOrder`

`MarketplaceSortOrder` selects the requested ordering of marketplace results:

- `Relevance`: search relevance score.
- `Downloads`: total downloads, descending.
- `Rating`: average community rating, descending.
- `LastUpdated`: last-published date, descending.
- `Name`: plugin name in alphabetical order.

## `MarketplaceEntry`

`MarketplaceEntry` is a marketplace listing containing discovery metadata and published versions. A new entry receives a generated `Id`; its string properties are empty, `Tags` and `AvailableVersions` are empty lists, and both UTC timestamps are initialized from `DateTime.UtcNow`. Numeric values and `IsVerified` retain their normal default values.

`Rating` represents a value on a 0.0–5.0 scale. `LicenseType` is intended for an SPDX identifier such as `MIT` or `Apache-2.0`. `AvailableVersions` contains registry-provided `PluginVersionInfo` items.

## `VersionCompatibilityMatrix`

The matrix is keyed first by plugin version and then by engine version. Both key levels use ordinal, case-insensitive comparison. `PluginId`, `PluginName`, and `GeneratedAtUtc` are init-only; `GeneratedAtUtc` defaults to the current UTC time.

`Entries` exposes all recorded rows through read-only dictionary interfaces. Its outer dictionary is produced when the property is read.

### `Record`

`Record` creates a row when the plugin version has not been seen and stores the supplied status under the engine version. Recording the same case-insensitive plugin-version and engine-version pairing again replaces its previous status.

### `GetStatus`

`GetStatus` returns the recorded status for a pairing. It returns `CompatibilityStatus.Unknown` if either the plugin-version row or engine-version entry is absent.

### `GetCompatiblePluginVersions`

`GetCompatiblePluginVersions` returns the plugin-version keys whose entry for the supplied engine version is exactly `CompatibilityStatus.Compatible`. Versions marked `Deprecated`, `Incompatible`, or `Unknown`, and versions without an entry for that engine version, are excluded.

### Example

```csharp
using PluginEngine.Marketplace;

var matrix = new VersionCompatibilityMatrix
{
    PluginId = Guid.NewGuid(),
    PluginName = "Sample.Plugin"
};

matrix.Record("2.1.0", "10.0", CompatibilityStatus.Compatible);
matrix.Record("2.0.0", "10.0", CompatibilityStatus.Deprecated);
matrix.Record("2.1.0", "9.0", CompatibilityStatus.Incompatible);

CompatibilityStatus known = matrix.GetStatus("2.1.0", "10.0");
CompatibilityStatus missing = matrix.GetStatus("1.0.0", "10.0"); // Unknown

IReadOnlyList<string> compatibleWithTen =
    matrix.GetCompatiblePluginVersions("10.0"); // Contains "2.1.0"
```

## `MarketplaceSearchFilter`

`MarketplaceSearchFilter` carries criteria for marketplace queries. `Query`, `MinVersion`, and `TargetFramework` default to `null`; `Tags` starts as an empty list, and its documented matching meaning is that a result must contain at least one supplied tag. `OnlyVerified` defaults to `false`.

Pagination defaults to page `1` with `20` results per page. The registry caps the page size at 100. `SortOrder` defaults to `MarketplaceSortOrder.Relevance`.
