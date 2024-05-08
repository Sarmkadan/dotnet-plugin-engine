# MarketplaceEntry

The `MarketplaceEntry` type encapsulates the metadata and version information for a plugin published in the marketplace. It provides properties to identify the plugin, describe its attributes, track usage statistics, and query compatibility with the host application.

## API

### Id  
`public Guid Id`  
A globally unique identifier for this marketplace entry. Intended to be immutable after creation; changing it may break references stored elsewhere.

### Name  
`public string Name`  
The display name of the plugin. Should not be null or empty; consumers may treat null/empty as invalid metadata.

### LatestVersion  
`public string LatestVersion`  
The version string of the most recent release of the plugin (e.g., `"1.3.0"`). Follows semantic versioning conventions but is not enforced by the type.

### Author  
`public string Author`  
The name or identifier of the plugin’s author or maintaining organization.

### Description  
`public string Description`  
A detailed description of the plugin’s functionality, intended for display in the marketplace UI.

### Tags  
`public List<string> Tags`  
A mutable list of strings used to categorize the plugin (e.g., `["utility", "ui"]`). May be empty; null values within the list are not expected.

### Downloads  
`public long Downloads`  
The total number of times the plugin has been downloaded. Expected to be non‑negative; decreasing this value manually is discouraged.

### Rating  
`public double Rating`  
The average user rating, typically ranging from `0.0` to `5.0`. Values outside this range may indicate corrupted data.

### PublishedAtUtc  
`public DateTime PublishedAtUtc`  
The UTC timestamp when the entry was first published. The `Kind` property should be `DateTimeKind.Utc`.

### UpdatedAtUtc  
`public DateTime UpdatedAtUtc`  
The UTC timestamp when the entry was last modified. Should be greater than or equal to `PublishedAtUtc`.

### IsVerified  
`public bool IsVerified`  
Indicates whether the entry has passed the marketplace verification process (e.g., security scan, signature check).

### LicenseType  
`public string LicenseType`  
A string describing the licensing model under which the plugin is distributed (e.g., `"MIT"`, `"GPLv3"`, `"Proprietary"`).

### RepositoryUrl  
`public string RepositoryUrl`  
The URL to the source code repository or project homepage for the plugin.

### AvailableVersions  
`public List<PluginVersionInfo> AvailableVersions`  
A mutable list containing information about every version of the plugin that is available for download. Each element provides version number, release date, and associated assets.

### PluginId  
`public Guid PluginId`  
A identifier that links this entry to the underlying plugin entity. May differ from `Id` if the marketplace groups multiple entries under a single plugin.

### PluginName  
`public string PluginName`  
The name of the plugin as stored in the plugin’s own metadata. Often duplicates `Name` but is kept for consistency with internal plugin representations.

### GeneratedAtUtc  
`public DateTime GeneratedAtUtc`  
The UTC timestamp when this `MarketplaceEntry` instance was created or last regenerated from source data. Useful for caching scenarios.

### Record  
`public void Record()`  
Records an interaction with the entry, such as a download or view, by incrementing internal counters (e.g., `Downloads`).  
- **Parameters:** None.  
- **Return value:** None.  
- **Exceptions:** May throw `InvalidOperationException` if the entry is marked as read‑only or if the underlying storage is unavailable.

### GetStatus  
`public CompatibilityStatus GetStatus()`  
Evaluates whether the plugin represented by this entry is compatible with the current host environment.  
- **Parameters:** None.  
- **Return value:** A `CompatibilityStatus` value indicating compatibility (e.g., `Compatible`, `Incompatible`, `Unknown`).  
- **Exceptions:** May throw `InvalidOperationException` if essential metadata (such as `AvailableVersions`) is missing or malformed.

### GetCompatiblePluginVersions  
`public IReadOnlyList<string> GetCompatiblePluginVersions()`  
Returns a read‑only list of version strings that are known to be compatible with the host’s current version.  
- **Parameters:** None.  
- **Return value:** An `IReadOnlyList<string>` containing version identifiers; the list may be empty if no compatible versions exist.  
- **Exceptions:** May throw `NotSupportedException` if the plugin does not provide version compatibility information.

## Usage

### Example 1: Checking compatibility and recording a download
```csharp
using System;
using DotnetPluginEngine; // namespace containing MarketplaceEntry

public class MarketplaceBrowser
{
    public void TryInstall(MarketplaceEntry entry)
    {
        // Verify the entry is verified before proceeding
        if (!entry.IsVerified)
        {
            Console.WriteLine($"Plugin {entry.Name} is not verified.");
            return;
        }

        // Determine compatibility with the current host
        var status = entry.GetStatus();
        if (status != CompatibilityStatus.Compatible)
        {
            Console.WriteLine($"Plugin {entry.Name} is not compatible (status: {status}).");
            return;
        }

        // Record the download attempt
        entry.Record();
        Console.WriteLine($"Recorded download for {entry.Name} (version {entry.LatestVersion}).");
    }
}
```

### Example 2: Enumerating available versions and selecting a compatible one
```csharp
using System;
using System.Linq;
using DotnetPluginEngine;

public class VersionSelector
{
    public string SelectLatestCompatibleVersion(MarketplaceEntry entry)
    {
        // Get the list of versions known to work with the host
        IReadOnlyList<string> compatible = entry.GetCompatiblePluginVersions();

        if (compatible.Count == 0)
            throw new InvalidOperationException("No compatible versions available for the current host.");

        // Assuming version strings are sortable (semantic versioning)
        return compatible.OrderByDescending(v => Version.Parse(v)).First();
    }
}
```

## Notes

- All string‑valued properties may be `null` if the source data omitted the field; consumers should guard against null reference exceptions where appropriate.
- The `Tags`, `AvailableVersions`, and similar mutable collections are exposed as `List<T>` to allow the marketplace service to populate them after construction. External code should treat these lists as read‑only unless it owns the instance.
- `Downloads` is expected to only increase; manually setting it to a lower value may produce inconsistent analytics.
- DateTime properties should always be expressed in UTC (`DateTimeKind.Utc`). Supplying a local time can lead to incorrect age calculations or sorting.
- The `Record`, `GetStatus`, and `GetCompatiblePluginVersions` methods access internal state that may be modified by other threads. If the instance is shared across threads, external synchronization is required to avoid race conditions. The methods themselves do not lock internally.
- `GetStatus` and `GetCompatiblePluginVersions` derive their results from the current contents of `AvailableVersions` and other metadata; if those collections are mutated after the call, the returned status may become stale.
- The type does not implement any interface (e.g., `IEquatable<MarketplaceEntry>`) and provides no custom `Equals` or `GetHashCode` implementation; reference equality is the default behavior.
