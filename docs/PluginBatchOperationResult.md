# PluginBatchOperationResult

Batch result wrapper for operations that affect multiple plugins. It collects individual `PluginOperationResult` values, keeps success and failure counts, and provides an overall success flag and a text summary.

Declared in `PluginEngine.Results` (`src/PluginEngine/Results/PluginOperationResult.cs`). The class is `sealed` and has a parameterless constructor.

## API

### `public List<(Guid PluginId, string PluginName, PluginOperationResult Result)> Results { get; }`

Ordered list of the per-plugin results recorded in the batch. The list is read-only as a property (no setter) and is initialized to an empty list. Entries are added only through `AddResult`.

---

### `public int SuccessCount { get; set; }`

Number of recorded operations whose `Success` was `true`. Incremented by `AddResult`.

- The setter is public, so the count can be changed without touching `Results`.

---

### `public int FailureCount { get; set; }`

Number of recorded operations whose `Success` was `false`. Incremented by `AddResult`.

- The setter is public, so the count can be changed without touching `Results`.

---

### `public int TotalCount => Results.Count`

Total number of operations recorded in the batch. Computed from `Results`, not from `SuccessCount + FailureCount`.

---

### `public long TotalDurationMs { get; set; }`

Total execution time of the batch in milliseconds.

- `AddResult` does not change this value. When the batch is built with `ToBatchResult`, it is set to the sum of the `DurationMs` values of the input results.

---

### `public bool IsSuccessful => FailureCount == 0 || SuccessCount > FailureCount`

Overall success flag for the batch ("all or most succeeded").

- Returns `true` when there are no failures, including an empty batch.
- Returns `true` when successes outnumber failures.
- Returns `false` when failures are present and the number of failures is equal to or greater than the number of successes.

| SuccessCount | FailureCount | IsSuccessful |
|--------------|--------------|--------------|
| 0            | 0            | `true`       |
| 3            | 0            | `true`       |
| 3            | 1            | `true`       |
| 2            | 2            | `false`      |
| 0            | 1            | `false`      |

---

### `public void AddResult(Guid pluginId, string pluginName, PluginOperationResult result)`

Adds a result for a plugin to the batch.

- **Parameters**
  - `pluginId`: The identifier of the plugin the operation ran for.
  - `pluginName`: The display name of the plugin.
  - `result`: The result of the operation.
- **Behavior**
  - Appends `(pluginId, pluginName, result)` to `Results`.
  - Increments `SuccessCount` when `result.Success` is `true`; otherwise increments `FailureCount`.
  - Does not change `TotalDurationMs`.
  - Classification uses only `result.Success`. A result with `Success == true` and an `ErrorCode` is counted as a success.
- **Throws**
  - None is thrown by the method itself. `result` is not null-checked: passing `null` adds the entry to `Results` and then throws `NullReferenceException` when `result.Success` is read, leaving the counters unchanged.

---

### `public string GetSummary()`

Returns a one-line summary of the batch.

- **Returns**
  - A string in the form `Batch Operation: {SuccessCount} succeeded, {FailureCount} failed, Total time: {TotalDurationMs}ms`.
  - Example: `Batch Operation: 3 succeeded, 1 failed, Total time: 250ms`.
- **Throws**
  - None.

---

## Extension Methods

Defined in `PluginOperationResultExtensions` (`src/PluginEngine/Results/PluginOperationResultExtensions.cs`). Both overloads create a new `PluginBatchOperationResult`, add every input result through `AddResult`, and set `TotalDurationMs` to the sum of the input `DurationMs` values.

### `public static PluginBatchOperationResult ToBatchResult(this IEnumerable<PluginOperationResult> results)`

Converts a collection of plugin operation results into a batch.

- **Parameters**
  - `results`: The results to convert.
- **Returns**
  - A new `PluginBatchOperationResult`. Each entry uses `Guid.Empty` as the plugin identifier and `"Operation"` as the plugin name, because the input carries no plugin information.
- **Throws**
  - `ArgumentNullException`: If `results` is `null`.

---

### `public static PluginBatchOperationResult ToBatchResult(this IEnumerable<(Guid PluginId, string PluginName, PluginOperationResult Result)> results)`

Converts a collection of identified plugin operation results into a batch.

- **Parameters**
  - `results`: The tuples to convert. Each tuple's plugin identifier and name are kept in the batch.
- **Returns**
  - A new `PluginBatchOperationResult` with the original plugin identifiers and names.
- **Throws**
  - `ArgumentNullException`: If `results` is `null`.

---

## Usage

```csharp
var batch = new PluginBatchOperationResult();

batch.AddResult(pluginId, "Logger", PluginOperationResult.CreateSuccess("Loaded", 120));
batch.AddResult(otherId, "Parser", PluginOperationResult.CreateFailure("Missing dependency", 1002, durationMs: 15));

batch.SuccessCount;     // 1
batch.FailureCount;     // 1
batch.TotalCount;       // 2
batch.IsSuccessful;     // false (failures are not fewer than successes)
batch.GetSummary();     // "Batch Operation: 1 succeeded, 1 failed, Total time: 0ms"

// Build the same kind of batch from a collection of plain results
var fromResults = new[]
{
    PluginOperationResult.CreateSuccess("OK", 40),
    PluginOperationResult.CreateSuccess("OK", 60)
}.ToBatchResult();

fromResults.Results[0].PluginName;   // "Operation"
fromResults.TotalDurationMs;         // 100
```
