# IHotSwapService

`IHotSwapService` represents the outcome of a hot‑swap operation for a plugin within the `dotnet-plugin-engine`. It provides read‑only information about the plugin involved, the assemblies before and after the swap, timing details, success status, and any error or rollback information.

## API

| Member | Type | Purpose | Remarks |
|--------|------|---------|---------|
| `PluginId` | `Guid` | Unique identifier of the plugin that was swapped. | Set when the hot‑swap operation begins; never changes after the instance is created. |
| `PreviousAssemblyPath` | `string` | Filesystem path to the assembly that was loaded before the swap. | May be `null` or empty if no prior assembly existed (e.g., first load). |
| `NewAssemblyPath` | `string` | Filesystem path to the assembly that was loaded after the swap. | May be `null` or empty if the swap failed and no new assembly was loaded. |
| `Success` | `bool` | Indicates whether the hot‑swap completed without error. | `true` when the operation succeeded; `false` otherwise. |
| `SwappedAtUtc` | `DateTime` | UTC timestamp when the swap attempt finished. | Reflects the moment the operation concluded, regardless of success. |
| `Duration` | `TimeSpan` | Elapsed time taken to perform the swap operation. | Measured from start to finish; zero if the operation was not executed. |
| `ErrorMessage` | `string?` | Descriptive message if the swap failed; otherwise `null`. | Populated only when `Success` is `false`. |
| `RolledBack` | `bool` | Indicates whether a rollback to the previous assembly was performed after a failure. | `true` when the engine reverted to `PreviousAssemblyPath` following an error; `false` if no rollback was attempted or possible. |

All members are read‑only getters; accessing them does not throw exceptions under normal circumstances. Invalid internal state (e.g., a corrupted instance) could result in an `InvalidOperationException`, but such scenarios are not expected when the object is obtained from the engine.

## Usage

```csharp
// Example 1: Performing a hot swap and inspecting the result
IHotSwapService swapService = pluginEngine.HotSwap(pluginId, newAssemblyPath);
if (swapService.Success)
{
    Console.WriteLine($"Plugin {swapService.PluginId} swapped successfully at {swapService.SwappedAtUtc:O}");
    Console.WriteLine($"Duration: {swapService.Duration}");
}
else
{
    Console.WriteLine($"Swap failed: {swapService.ErrorMessage}");
    if (swapService.RolledBack)
    {
        Console.WriteLine("Rolled back to previous assembly.");
    }
}
```

```csharp
// Example 2: Logging swap details for auditing regardless of outcome
void LogSwapResult(IHotSwapService result)
{
    var log = new
    {
        PluginId = result.PluginId,
        PreviousAssembly = result.PreviousAssemblyPath,
        NewAssembly = result.NewAssemblyPath,
        Success = result.Success,
        TimestampUtc = result.SwappedAtUtc,
        DurationMs = result.Duration.TotalMilliseconds,
        Error = result.ErrorMessage,
        RolledBack = result.RolledBack
    };
    logger.Information("HotSwapResult: {@SwapLog}", log);
}
```

## Notes

- The properties are intended to be immutable after the `IHotSwapService` instance is created; therefore, the object is safe to read concurrently from multiple threads without additional synchronization.
- If `Success` is `false`, `ErrorMessage` will contain a non‑null explanation; when `Success` is `true`, `ErrorMessage` is guaranteed to be `null`.
- `RolledBack` being `true` implies that a failure occurred and the engine successfully restored the previous assembly; it does not guarantee that `PreviousAssemblyPath` is non‑null, as a rollback may be a no‑op if no prior assembly existed.
- `PluginId` may be `Guid.Empty` if the engine could not associate the operation with a known plugin (e.g., during early initialization), though typical usage yields a valid GUID.
- Path values are provided as they were supplied to the engine; they are not normalized or validated by the interface itself. Consumers should treat them as opaque strings and perform any required filesystem checks independently.
