# PluginDiscoveryModels

Data models used by `PluginDiscoveryService` in `PluginEngine.Utils.Helpers` to describe discovered plugin files, filter them, and summarize a discovery run: `PluginCandidateInfo`, `PluginDiscoveryFilter`, and `DiscoveryStatistics`.

## PluginCandidateInfo

Information about a single plugin file found during discovery. Instances are created by `PluginDiscoveryService.InspectPluginAsync` (and therefore by `DiscoverPluginsAsync`), one per plugin file.

| Property | Type | Description |
| --- | --- | --- |
| `FilePath` | `string` | The full path to the plugin file. Required. |
| `FileName` | `string` | The file name including its extension, for example `Acme.Plugin.dll`. Required. |
| `AssemblyName` | `string` | The file name without its extension, for example `Acme.Plugin`. This is the value `FilterPlugins` matches `NamePattern` against. Required. |
| `FileSize` | `long` | The size of the plugin file in bytes. Required. |
| `ModifiedAtUtc` | `DateTime` | The last write time of the plugin file, in UTC. Required. |
| `IsValid` | `bool` | `true` if the file could be loaded with `Assembly.LoadFrom`; otherwise `false`. Required. |
| `DiscoveredAtUtc` | `DateTime` | The UTC time at which the file was inspected. Required. |
| `Version` | `string?` | The value of `AssemblyVersionAttribute` on the assembly, or `null` if the attribute is absent or the assembly could not be loaded. |
| `ProductName` | `string?` | The value of `AssemblyProductAttribute`, or `null`. |
| `Company` | `string?` | The value of `AssemblyCompanyAttribute`, or `null`. |
| `Description` | `string?` | The value of `AssemblyDescriptionAttribute`, or `null`. |
| `CustomAttributes` | `List<string>` | The names of the attribute types found on the assembly (for example `AssemblyTitleAttribute`), not their values. Get-only; initialized to an empty list and filled by the service. |

The four required fields that are not set by the service (`FilePath`, `FileName`, `AssemblyName`, `FileSize`) are always set during inspection, so every candidate returned by the service has them populated.

### How the metadata is populated

- `IsValid` is `true` only when `Assembly.LoadFrom` succeeds. A load failure is logged as a warning and the candidate is still returned, with `IsValid = false`.
- The `Version`, `ProductName`, `Company`, `Description`, and `CustomAttributes` properties are filled only for loaded assemblies. For an invalid candidate they keep their defaults (`null` or an empty list).
- Metadata extraction is best-effort. If reading the attributes throws, the error is logged at debug level and the properties read so far are kept.

### ToString

Returns `PluginCandidateInfo: FileName=..., AssemblyName=..., Version=..., IsValid=...`, with `Version` shown as `null` when it is not set.

---

## PluginDiscoveryFilter

Criteria used by `PluginDiscoveryService.FilterPlugins` to select candidates. Every property has a default that means "do not filter on this criterion", except `ValidOnly`, which defaults to `true`.

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `ValidOnly` | `bool` | `true` | When `true`, only candidates with `IsValid == true` are kept. |
| `MinimumVersionInfo` | `string?` | `null` | The minimum acceptable version. Candidates whose `Version` is lower are removed. `null` disables the check. |
| `NamePattern` | `string?` | `null` | A regular expression matched against `AssemblyName`. `null` disables the check. |
| `MaxFileSizeBytes` | `long?` | `null` | The largest allowed `FileSize`, in bytes, inclusive. `null` disables the check. |

### ToString

Returns `PluginDiscoveryFilter: ValidOnly=..., MinimumVersionInfo=..., NamePattern=..., MaxFileSizeBytes=...`, with each unset value shown as `null`.

---

## DiscoveryStatistics

Aggregate figures for a set of plugin candidates, produced by `PluginDiscoveryService.GetStatistics`.

| Property | Type | Description |
| --- | --- | --- |
| `TotalCandidates` | `int` | The number of candidates in the input list. |
| `ValidPlugins` | `int` | The number of candidates with `IsValid == true`. |
| `InvalidPlugins` | `int` | The number of candidates with `IsValid == false`. |
| `TotalSizeBytes` | `long` | The sum of `FileSize` over all candidates, valid and invalid. |
| `DiscoveredAtUtc` | `DateTime` | The UTC time at which the statistics were generated. |
| `ValidPercentage` | `double` | Read-only. The share of valid plugins, from `0` to `100`. Computed as `ValidPlugins / TotalCandidates * 100`, or `0` when `TotalCandidates` is `0`. |

For any statistics object produced by `GetStatistics`, `ValidPlugins + InvalidPlugins == TotalCandidates`.

### ToString

Returns `DiscoveryStatistics: TotalCandidates=..., ValidPlugins=..., InvalidPlugins=...`. It does not include `TotalSizeBytes`, `DiscoveredAtUtc`, or `ValidPercentage`.

---

## How FilterPlugins applies the filter

```csharp
public List<PluginCandidateInfo> FilterPlugins(
    List<PluginCandidateInfo> candidates,
    PluginDiscoveryFilter? filter = null)
```

- **Default filter**: if `filter` is `null`, a new `PluginDiscoveryFilter` is used. Its defaults apply, so invalid candidates are removed even when no filter is passed.
- **Order and combination**: the criteria are applied as a chain of `Where` clauses, in this order. A candidate is kept only if it passes every clause. The order does not change the result.
  1. `ValidOnly`: when `true`, `IsValid` must be `true`.
  2. `MinimumVersionInfo`: when set, the candidate's `Version` must be non-null and `VersionHelper.CompareVersions(Version, MinimumVersionInfo) >= 0`.
  3. `NamePattern`: when set, `Regex.IsMatch(AssemblyName, NamePattern)` must be `true`. The match is unanchored, so use `^` and `$` for an exact match.
  4. `MaxFileSizeBytes`: when set, `FileSize <= MaxFileSizeBytes`.
- **Version rules**: a candidate with no `Version` is removed whenever `MinimumVersionInfo` is set. `CompareVersions` returns `-1` when either string cannot be parsed, so an unparsable candidate version, or an unparsable `MinimumVersionInfo`, also removes the candidate.
- **Result**: a new list containing the surviving candidates in their original order. The input list is not modified.
- **Exceptions**: an invalid `NamePattern` throws `ArgumentException` from `Regex`. The method does not catch it.

---

## How GetStatistics aggregates

```csharp
public DiscoveryStatistics GetStatistics(List<PluginCandidateInfo> candidates)
```

`GetStatistics` makes a single pass over the list it is given, once per counter. It does not apply a filter, so pass the list you want summarized: the raw result of `DiscoverPluginsAsync` for totals across the directory, or the result of `FilterPlugins` for the subset you kept.

- `TotalCandidates` is the number of items in the list.
- `ValidPlugins` counts items with `IsValid == true`.
- `InvalidPlugins` counts items with `IsValid == false`.
- `TotalSizeBytes` sums `FileSize` across all items, including invalid ones.
- `DiscoveredAtUtc` is set to the current UTC time when the statistics are generated. It is not taken from the candidates' `DiscoveredAtUtc` values.
- `ValidPercentage` is not stored. It is computed from `ValidPlugins` and `TotalCandidates` each time it is read.

An empty list produces zero counts, a total size of `0`, and a `ValidPercentage` of `0`.

## Usage

```csharp
var candidates = await discoveryService.DiscoverPluginsAsync(pluginDirectory);

var compatible = discoveryService.FilterPlugins(candidates, new PluginDiscoveryFilter
{
    MinimumVersionInfo = "1.2.0",
    NamePattern = @"^Acme\.",
    MaxFileSizeBytes = 5 * 1024 * 1024
});

var summary = discoveryService.GetStatistics(compatible);
Console.WriteLine($"{summary.ValidPlugins}/{summary.TotalCandidates} valid ({summary.ValidPercentage:F0}%)");
```

Because `ValidOnly` defaults to `true`, the example above excludes invalid candidates. Set `ValidOnly = false` to keep them.
