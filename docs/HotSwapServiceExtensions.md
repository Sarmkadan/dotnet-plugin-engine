# HotSwapServiceExtensions

Extension methods for the `HotSwapService` that provide convenient ways to manage plugin hot-swapping operations. These methods simplify interaction with the plugin engine's hot-swap capabilities by exposing common operations through a fluent, task-based API.

## API

### `CanSwap()`
Determines whether a plugin can be swapped at runtime based on the current engine state and plugin compatibility rules.

- **Returns**: `bool` – `true` if a swap operation is currently permitted; otherwise, `false`.
- **Remarks**: This check does not guarantee that a subsequent swap will succeed, as runtime conditions may change.

### `GetLastSwapRecordAsync()`
Retrieves the most recent swap operation record, if any exists.

- **Returns**: `Task<PluginOperationResult<SwapRecord?>>` – A task that resolves to an operation result containing the last swap record or `null` if no swaps have occurred.
- **Remarks**: Returns a failed result if no swap history exists or if retrieval fails.

### `GetSwapHistoryAsync()`
Retrieves the full history of swap operations performed during the current session.

- **Returns**: `Task<PluginOperationResult<List<SwapRecord>>>` – A task that resolves to an operation result containing the list of all swap records.
- **Remarks**: The list is ordered chronologically from oldest to newest. Returns a failed result if retrieval fails.

### `RegisterPostSwapCallback(Action<SwapRecord> callback)`
Registers a callback to be invoked after each successful plugin swap operation.

- **Parameters**:
  - `callback` – The delegate to invoke after a swap completes successfully. Receives the swap record as an argument.
- **Remarks**: Multiple callbacks can be registered. Callbacks are invoked in the order they were registered. No action is taken if the same callback is registered multiple times.

### `UnregisterPostSwapCallback(Action<SwapRecord> callback)`
Removes a previously registered post-swap callback.

- **Parameters**:
  - `callback` – The delegate to remove from the callback list.
- **Remarks**: If the callback was not registered, this method has no effect.

### `RollbackSwapAsync()`
Reverts the most recent successful plugin swap, restoring the previous plugin state.

- **Returns**: `Task<PluginOperationResult>` – A task that resolves to an operation result indicating success or failure.
- **Remarks**: Fails if no swap history exists or if the rollback cannot be performed due to engine state.

### `SwapPluginAsync(PluginIdentity newPlugin)`
Initiates an asynchronous plugin swap using the specified plugin identity.

- **Parameters**:
  - `newPlugin` – The identity of the plugin to activate in place of the current one.
- **Returns**: `Task<PluginOperationResult>` – A task that resolves to an operation result indicating success or failure.
- **Remarks**: Fails if the plugin is incompatible, not found, or if the engine is in an invalid state.

### `SwapPluginAsync(PluginIdentity newPlugin, bool force)`
Initiates an asynchronous plugin swap with an option to bypass compatibility checks.

- **Parameters**:
  - `newPlugin` – The identity of the plugin to activate.
  - `force` – If `true`, attempts to swap even if compatibility checks would normally prevent it.
- **Returns**: `Task<PluginOperationResult>` – A task that resolves to an operation result indicating success or failure.
- **Remarks**: Use with caution; forcing a swap may result in runtime errors or undefined behavior.

### `HasSwapHistoryAsync()`
Determines whether any swap operations have been recorded during the current session.

- **Returns**: `Task<bool>` – A task that resolves to `true` if at least one swap has occurred; otherwise, `false`.
- **Remarks**: Returns `false` if retrieval of history fails.

### `GetSwapHistoryCountAsync()`
Returns the total number of swap operations recorded during the current session.

- **Returns**: `Task<int>` – A task that resolves to the count of swap records, or `0` if retrieval fails.
- **Remarks**: The count reflects the number of successful swaps only.

### `GetLastSuccessfulSwapAsync()`
Retrieves the most recent successful swap operation record.

- **Returns**: `Task<PluginOperationResult<SwapRecord?>>` – A task that resolves to an operation result containing the last successful swap record or `null` if none exists.
- **Remarks**: Returns a failed result if no successful swaps have occurred.

### `IsLastSwapSuccessfulAsync()`
Checks whether the most recent swap operation completed successfully.

- **Returns**: `Task<bool>` – A task that resolves to `true` if the last swap was successful; otherwise, `false`.
- **Remarks**: Returns `false` if no swaps have occurred or if the result cannot be determined.

## Usage
