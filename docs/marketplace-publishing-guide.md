# Plugin Marketplace Publishing Guide

This guide covers how to publish a plugin to the marketplace, configure the marketplace
client, and troubleshoot common issues.

## Overview

The marketplace system is built around three components:

- **`IPluginMarketplaceService`** – high-level API used by host applications to search,
  inspect, and install plugins.
- **`IRemotePluginRegistry`** – low-level HTTP client that talks to the registry back-end.
- **`MarketplaceExtensions`** – `IServiceCollection` helpers for wiring everything up.

```
Host app
  └─ IPluginMarketplaceService
       └─ IRemotePluginRegistry ──► Registry API
```

## Setting Up the Marketplace Client

Register the marketplace services during application startup:

```csharp
using PluginEngine.Marketplace;

services.AddPluginEngine(options =>
{
    options.PluginDirectory = "./plugins";
});

services.AddPluginMarketplace(options =>
{
    options.RegistryBaseUrl = "https://registry.example.com";
    options.ApiKey          = Environment.GetEnvironmentVariable("PLUGIN_REGISTRY_KEY");
    options.TimeoutSeconds  = 30;
});
```

## Searching the Marketplace

```csharp
var marketplace = serviceProvider.GetRequiredService<IPluginMarketplaceService>();

var result = await marketplace.SearchAsync(new MarketplaceSearchFilter
{
    Query      = "logging",
    Tags       = ["audit", "structured"],
    OnlyVerified = true,
    SortOrder  = MarketplaceSortOrder.Downloads,
    Page       = 1,
    PageSize   = 20
});

if (result.Success)
{
    foreach (var entry in result.Data!)
        Console.WriteLine($"{entry.Name} {entry.LatestVersion} by {entry.Author}  ★ {entry.Rating:F1}");
}
```

### Sort Options

| `MarketplaceSortOrder` value | Description |
|------------------------------|-------------|
| `Relevance` | Best textual match (default) |
| `Downloads` | Most downloaded first |
| `Rating` | Highest community rating first |
| `LastUpdated` | Most recently published first |
| `Name` | Alphabetical |

## Installing a Plugin

```csharp
var installResult = await marketplace.InstallAsync(
    pluginId        : entry.Id,
    version         : entry.LatestVersion,
    targetDirectory : Path.Combine(AppContext.BaseDirectory, "plugins"));

if (installResult.Success)
    Console.WriteLine(installResult.Message);
else
    Console.WriteLine($"Install failed: {installResult.Message}");
```

After installation, load the plugin via the engine:

```csharp
var loader = serviceProvider.GetRequiredService<IPluginLoaderService>();
var plugin = await loader.LoadPluginAsync(
    Path.Combine("plugins", $"{entry.Name}.dll"));
```

## Checking Compatibility

Before installing, verify the plugin is compatible with the running engine version:

```csharp
var engineVersion = typeof(PluginEngine.PluginEngine)
    .Assembly.GetName().Version?.ToString(2) ?? "10.0";

var statusResult = await marketplace.CheckCompatibilityAsync(
    pluginId      : entry.Id,
    pluginVersion : entry.LatestVersion,
    engineVersion : engineVersion);

if (statusResult.Data == CompatibilityStatus.Compatible)
    Console.WriteLine("Safe to install.");
else
    Console.WriteLine($"Compatibility warning: {statusResult.Data}");
```

### Compatibility Matrix

```csharp
var matrixResult = await marketplace.GetCompatibilityMatrixAsync(entry.Id);
var matrix = matrixResult.Data!;

// List all versions compatible with engine 10.0
var compatible = matrix.GetCompatiblePluginVersions("10.0");
Console.WriteLine($"Versions compatible with engine 10.0: {string.Join(", ", compatible)}");

// Inspect a specific pairing
var status = matrix.GetStatus("2.1.0", "9.0");
Console.WriteLine($"Plugin 2.1.0 on engine 9.0: {status}");
```

Compatibility is determined automatically using publication age and stability flags:

| Plugin age | Engine 10.0 | Engine 9.0 | Engine 8.0 |
|-----------|-------------|-----------|-----------|
| ≤ 1 year  | Compatible  | Compatible | Compatible |
| 1–2 years | Compatible  | Compatible | Deprecated |
| > 2 years | Deprecated  | Incompatible | Incompatible |

Pre-release versions are only marked `Compatible` on the latest engine major.

## Publishing a Plugin

### 1. Prepare the Plugin Package

Build a release artifact and include a `plugin.json` metadata file:

```json
{
  "name": "MyPlugin",
  "version": "1.2.0",
  "description": "Short description shown in the marketplace listing.",
  "author": "Your Name",
  "license": "MIT",
  "repositoryUrl": "https://github.com/example/my-plugin",
  "engineVersionConstraint": ">=10.0",
  "tags": ["logging", "structured", "audit"]
}
```

```bash
dotnet publish -c Release -o ./publish
# The publish directory should contain MyPlugin.dll and plugin.json
```

### 2. Create an Archive

```bash
cd publish
zip -r ../my-plugin-1.2.0.zip .
```

### 3. Register via the Registry API

Use your API key to register the new version:

```http
POST /api/v1/plugins
Authorization: Bearer <API_KEY>
Content-Type: multipart/form-data

--boundary
Content-Disposition: form-data; name="metadata"
Content-Type: application/json

{ "name": "MyPlugin", "version": "1.2.0", ... }

--boundary
Content-Disposition: form-data; name="archive"; filename="my-plugin-1.2.0.zip"
Content-Type: application/octet-stream

<binary data>
```

The registry responds with the assigned plugin `id` – store this for future version updates.

### 4. Verify the Listing

```csharp
var entryResult = await marketplace.GetEntryAsync(pluginId);
Console.WriteLine(entryResult.Data?.Name);   // "MyPlugin"
Console.WriteLine(entryResult.Data?.LatestVersion);  // "1.2.0"
```

## Best Practices

- **Semantic versioning** – use `MAJOR.MINOR.PATCH`. Increment `MAJOR` on breaking changes so
  the compatibility matrix can flag older engine versions.
- **Stable pre-releases** – use a pre-release suffix (`1.3.0-beta.1`) for unstable builds so
  the marketplace marks them `Unknown` on older engines.
- **Keep `plugin.json` in sync** with your `.csproj` `<Version>` property.
- **Tag generously** – tags drive discovery; include both broad (`logging`) and specific
  (`serilog-sink`) terms.
- **Maintain a CHANGELOG** and link it in `repositoryUrl` to help users decide whether to upgrade.

## Caching

Compatibility matrices are cached in-memory for **30 minutes** to reduce registry round-trips.
Force a refresh by restarting the application or clearing the `IMemoryCache` entry with key
`mp_matrix_{pluginId}`.

## Troubleshooting

### `Plugin not found in the marketplace` (HTTP 404)

- Confirm the `pluginId` GUID is correct.
- Check the registry base URL is reachable:
  ```bash
  curl -I https://registry.example.com/health
  ```

### Download fails with `502`

The registry returned an empty download URL. Verify the version string matches exactly the
version published in the registry (case-sensitive):

```csharp
var entry = (await marketplace.GetEntryAsync(pluginId)).Data!;
Console.WriteLine(string.Join(", ", entry.AvailableVersions.Select(v => v.Version)));
```

### Compatibility always `Unknown`

Pre-release versions resolve to `Unknown` on engine versions below 10.0. Publish a stable
release (`IsPrerelease = false`, `IsStable = true`) or target a newer engine.

### API key rejected

Ensure the key is passed correctly and has not expired. Rotate the key in the registry
dashboard and update the environment variable.

## See Also

- [Getting Started](./getting-started.md)
- [API Reference](./api-reference.md)
- [Architecture Guide](./architecture.md)
