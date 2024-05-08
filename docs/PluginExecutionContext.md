# PluginExecutionContext

The `PluginExecutionContext` encapsulates all runtime information for a single execution of a plugin within the dotnet‑plugin‑engine. It holds identifiers, timestamps, state, user‑defined data, outcome details, and performance metrics, allowing the engine and plugins to coordinate execution lifecycle and collect diagnostics.

## API

| Member | Type | Purpose | Parameters | Return Value | Throws |
|--------|------|---------|------------|--------------|--------|
| `ExecutionId` | `Guid` (required) | Unique identifier for this plugin execution. Set via object initializer. | – | – | – |
| `Plugin` | `Plugin` (required) | Reference to the plugin instance being executed. | – | – | – |
| `OperationType` | `string` (required) | Logical name of the operation being performed (e.g., "Create", "Update", "Delete"). | – | – | – |
| `StartedAtUtc` | `DateTime` | Timestamp when the execution began (UTC). | – | – | – |
| `CompletedAtUtc` | `DateTime?` | Timestamp when the execution finished (UTC); null while still running. | – | – | – |
| `State` | `ExecutionState` | Current lifecycle state (`NotStarted`, `Running`, `Completed`, `Failed`, `Canceled`). | – | – | – |
| `Data` | `Dictionary<string, object>` | Bag for arbitrary data shared between the engine, plugins, and hosts. Keys are strings; values are any object. | – | – | – |
| `Exception` | `Exception?` | If the execution ended in failure, holds the captured exception; otherwise null. | – | – | – |
| `Result` | `object?` | Output produced by the plugin on successful completion; meaningful only when `State` is `Completed`. | – | – | – |
| `Metrics` | `ExecutionMetrics` | Structured performance counters (CPU time, memory, GC counts, custom metrics). | – | – | – |
| `CompleteSuccess` | `void` | Marks the execution as successful. Sets `CompletedAtUtc` to now, `State` to `Completed`, and clears `Exception`. | – | – | `InvalidOperationException` if called after the execution has already reached a terminal state (`Completed`, `Failed`, or `Canceled`). |
| `CompleteFailed` | `void` | Marks the execution as failed. Sets `CompletedAtUtc` to now, `State` to `Failed`, and preserves any existing `Exception`. | – | – | `InvalidOperationException` if called after the execution has already reached a terminal state. |
| `Cancel` | `void` | Requests cancellation of the execution. Sets `CompletedAtUtc` to now, `State` to `Canceled`. Does not alter `Exception` or `Result`. | – | – | `InvalidOperationException` if called after the execution has already reached a terminal state. |
| `GetSummary` | `ExecutionSummary` | Returns a snapshot summarizing the execution (identifiers, timestamps, state, outcome, and key metrics). | – | `ExecutionSummary` | – |
| `CpuTimeMs` | `long` | Total CPU time consumed by the plugin, in milliseconds. | – | – | – |
| `MemoryBytesAllocated` | `long` | Total managed memory allocated during the execution, in bytes. | – | – | – |
| `GarbageCollections` | `int` | Number of garbage collections that occurred during the execution. | – | – | – |
| `CollectedAtUtc` | `DateTime` | Timestamp when the performance metrics were last sampled (UTC). | – | – | – |
| `CustomMetrics` | `Dictionary<string, long>` | Extensible metric store; keys are metric names, values are 64‑bit integer counts. | – | – | – |

## Usage

### Example 1: Executing a plugin and capturing success

```csharp
var ctx = new PluginExecutionContext
{
    ExecutionId = Guid.NewGuid(),
    Plugin = myPlugin,
    OperationType = "Create",
    StartedAtUtc = DateTime.UtcNow
};

try
{
    // Invoke plugin logic; assume it populates ctx.Data and ctx.Result
    myPlugin.Execute(ctx);

    ctx.CompleteSuccess(); // marks execution as finished successfully
}
catch (Exception ex)
{
    ctx.Exception = ex;
    ctx.CompleteFailed();   // marks execution as finished with failure
}

var summary = ctx.GetSummary();
// summary can be logged or returned to the caller
```

### Example 2: Canceling a long‑running plugin

```csharp
var ctx = new PluginExecutionContext
{
    ExecutionId = Guid.NewGuid(),
    Plugin = longRunningPlugin,
    OperationType = "Import",
    StartedAtUtc = DateTime.UtcNow
};

// Start execution on a background thread
Task.Run(() => longRunningPlugin.Execute(ctx));

// Somewhere else, request cancellation
ctx.Cancel(); // sets State to Canceled and records completion time

// After the plugin can poll ctx.State
```

## Notes

- The `required` modifiers on `ExecutionId`, `Plugin`, and `OperationType` enforce that these properties must be supplied via an object initializer; constructing the context without them results in a compile‑time error.
- The context is **not thread‑safe**. Concurrent reads or writes to any member from multiple threads without external synchronization may lead to race conditions, especially for mutable collections like `Data` and `CustomMetrics`.
- Calling `CompleteSuccess`, `CompleteFailed`, or `Cancel` after the context has already transitioned to a terminal state (`Completed`, `Failed`, or `Canceled`) throws an `InvalidOperationException` to prevent state corruption.
- `CompletedAtUtc` remains `null` until one of the completion methods is invoked; reading it before completion yields `null`.
- `Metrics` is populated by the engine during execution; plugins should not overwrite it directly unless they intend to replace the entire `ExecutionMetrics` instance.
- Custom metrics added to `CustomMetrics` should use long‑integer values; the engine does not interpret their semantics.
- The `Result` property is meaningful only when `State` equals `Completed`; reading it in other states may return a stale or default value.
