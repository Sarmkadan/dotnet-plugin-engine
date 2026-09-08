# Error codes

The plugin engine contains two separate error-code schemes:

- `ErrorCodes` defines string constants for naming plugin-engine errors.
- `PluginOperationResult.ErrorCode` is an `int?` populated on failed operation results.

The source does not define a conversion between the string constants and the numeric result codes. Successful results clear `ErrorCode` and `ErrorDetails`; failed results retain the supplied numeric code and details ([source](../src/PluginEngine/Results/PluginOperationResult.cs#L25)).

## Result API signatures

| API | Signature | Error-code behavior | Source |
| --- | --- | --- | --- |
| Non-generic failure | `PluginOperationResult CreateFailure(string message, int errorCode = 500, string? details = null, long durationMs = 0)` | Uses `500` when the caller omits `errorCode`. | [PluginOperationResult.cs](../src/PluginEngine/Results/PluginOperationResult.cs#L101) |
| Generic failure | `PluginOperationResult<T> CreateFailure(string message, int errorCode = 500, string? details = null, long durationMs = 0)` | Uses `500` when the caller omits `errorCode`; failure data is `default`. | [PluginOperationResult.cs](../src/PluginEngine/Results/PluginOperationResult.cs#L199) |
| Non-generic exception conversion | `PluginOperationResult FromException(Exception ex, long durationMs = 0)` | Maps the exception type to `1001`, `1002`, `1003`, or `500`. | [PluginOperationResult.cs](../src/PluginEngine/Results/PluginOperationResult.cs#L113) |
| Generic exception conversion | `PluginOperationResult<T> FromException(Exception ex, long durationMs = 0)` | Uses the same exception mapping and returns no data. | [PluginOperationResult.cs](../src/PluginEngine/Results/PluginOperationResult.cs#L211) |

Both failure factories require a non-empty failure message; construction throws `ArgumentException` when a failed result has a null, empty, or whitespace message ([source](../src/PluginEngine/Results/PluginOperationResult.cs#L42)). `FromException` throws `ArgumentNullException` for a null exception ([non-generic source](../src/PluginEngine/Results/PluginOperationResult.cs#L117), [generic source](../src/PluginEngine/Results/PluginOperationResult.cs#L215)).

## String codes

All string codes are declared by `PluginEngine.Constants.ErrorCodes`. Their descriptions below reflect the names and XML documentation in the constants file; no runtime mapping to `PluginOperationResult.ErrorCode` is present.

| Constant | String value | Declared meaning | Source |
| --- | --- | --- | --- |
| `GenericError` | `PLUGIN_ERROR` | Generic plugin error | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L132) |
| `PluginNotFound` | `PLUGIN_NOT_FOUND` | Plugin not found | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L137) |
| `PluginAlreadyLoaded` | `PLUGIN_ALREADY_LOADED` | Plugin already loaded | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L142) |
| `PluginLoadFailed` | `PLUGIN_LOAD_FAILED` | Plugin load failed | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L147) |
| `DependencyResolutionFailed` | `DEPENDENCY_RESOLUTION_FAILED` | Dependency resolution failed | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L152) |
| `VersionMismatch` | `VERSION_MISMATCH` | Version mismatch | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L157) |
| `InvalidConfiguration` | `INVALID_CONFIGURATION` | Invalid plugin configuration | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L162) |
| `HotReloadFailed` | `HOT_RELOAD_FAILED` | Hot reload failed | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L167) |
| `CircularDependency` | `CIRCULAR_DEPENDENCY` | Circular dependency | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L172) |
| `OperationTimeout` | `OPERATION_TIMEOUT` | Operation timeout | [PluginEngineConstants.cs](../src/PluginEngine/Constants/PluginEngineConstants.cs#L177) |

## Numeric codes

### Exception mapping and defaults

| Code | Visible behavior | Source |
| --- | --- | --- |
| `500` | Default for both `CreateFailure` overloads and the fallback for exception types not otherwise matched by `GetErrorCode`. `CheckCompatibilityAsync` also uses it if a failed matrix result has no code. | [PluginOperationResult.cs](../src/PluginEngine/Results/PluginOperationResult.cs#L101), [PluginOperationResult.cs](../src/PluginEngine/Results/PluginOperationResult.cs#L134), [PluginMarketplaceService.cs](../src/PluginEngine/Marketplace/PluginMarketplaceService.cs#L179) |
| `1001` | Produced by `FromException` for `PluginLoadException`. | [PluginOperationResult.cs](../src/PluginEngine/Results/PluginOperationResult.cs#L134) |
| `1002` | Produced by `FromException` for `DependencyResolutionException`. `PluginDependencyResolver.GetInstallOrderAsync` also returns it explicitly when topological sorting throws that exception, logged there as a circular dependency. | [PluginOperationResult.cs](../src/PluginEngine/Results/PluginOperationResult.cs#L134), [PluginDependencyResolver.cs](../src/PluginEngine/Services/Implementations/PluginDependencyResolver.cs#L44) |
| `1003` | Produced by `FromException` for `VersionMismatchException`. | [PluginOperationResult.cs](../src/PluginEngine/Results/PluginOperationResult.cs#L134) |

### Service-returned codes

| Code | Service and condition | Source |
| --- | --- | --- |
| `400` | `HotSwapService.SwapPluginAsync`: `newAssemblyPath` is null, empty, or whitespace. | [HotSwapService.cs](../src/PluginEngine/Services/Implementations/HotSwapService.cs#L33) |
| `400` | `MarketplaceBrowserService.BrowseCategoryAsync`: `categoryId` is null, empty, or whitespace. | [MarketplaceBrowserService.cs](../src/PluginEngine/Marketplace/MarketplaceBrowserService.cs#L224) |
| `404` | `HotSwapService.SwapPluginAsync`: the replacement assembly file does not exist, or the requested plugin is not loaded. | [HotSwapService.cs](../src/PluginEngine/Services/Implementations/HotSwapService.cs#L40) |
| `404` | `HotSwapService.RollbackSwapAsync`: no successful swap that has not already been rolled back is found. | [HotSwapService.cs](../src/PluginEngine/Services/Implementations/HotSwapService.cs#L122) |
| `404` | `PluginDependencyResolver.BuildResolutionPlanAsync`: the root plugin is not loaded. | [PluginDependencyResolver.cs](../src/PluginEngine/Services/Implementations/PluginDependencyResolver.cs#L87) |
| `404` | `PluginMarketplaceService.GetEntryAsync`: the registry returns no plugin information. | [PluginMarketplaceService.cs](../src/PluginEngine/Marketplace/PluginMarketplaceService.cs#L108) |
| `409` | `HotSwapService.SwapPluginAsync`: `CanSwap` rejects the loaded plugin's current status. | [HotSwapService.cs](../src/PluginEngine/Services/Implementations/HotSwapService.cs#L49) |
| `502` | `PluginMarketplaceService.InstallAsync`: the registry download returns no file path. | [PluginMarketplaceService.cs](../src/PluginEngine/Marketplace/PluginMarketplaceService.cs#L189) |

Other exceptions caught by these services are passed to `FromException`, so their numeric code follows the exception mapping above. For example, hot-swap and rollback failures use `FromException` ([source](../src/PluginEngine/Services/Implementations/HotSwapService.cs#L89)).

## C# example

Create a failure with a service-style numeric code and inspect it through the result API:

```csharp
using PluginEngine.Results;

PluginOperationResult result = PluginOperationResult.CreateFailure(
    "Plugin not found in the marketplace.",
    errorCode: 404);

if (!result.Success && result.ErrorCode == 404)
{
    Console.WriteLine(result.Message);
}
```

String constants remain a separate vocabulary and can be used where a string identifier is required:

```csharp
using PluginEngine.Constants;

string code = ErrorCodes.PluginNotFound; // "PLUGIN_NOT_FOUND"
```
