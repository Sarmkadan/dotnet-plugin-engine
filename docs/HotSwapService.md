# HotSwapService

The `HotSwapService` manages runtime plugin swapping and rollback operations within the `dotnet-plugin-engine` system. It tracks swap history, enforces constraints on plugin replacement, and allows callbacks to be registered around swap events.

## API

### `HotSwapService`

Initializes a new instance of the `HotSwapService`. The service is responsible for managing plugin swap operations, maintaining swap history, and enforcing swap constraints.

### `async Task<PluginOperationResult> SwapPluginAsync`

Initiates an asynchronous plugin swap operation. Replaces the currently loaded plugin with a new version while preserving state where possible.

- **Parameters**:
  - `newPlugin`: The new plugin instance to swap in.
- **Return value**: A `Task<PluginOperationResult>` indicating success or failure of the swap operation.
- **Exceptions**: Throws if the swap is not allowed (`CanSwap` returns `false`) or if the operation fails during execution.

### `async Task<PluginOperationResult> RollbackSwapAsync`

Asynchronously reverts the most recent successful plugin swap, restoring the previous plugin version.

- **Return value**: A `Task<PluginOperationResult>` indicating success or failure of the rollback.
- **Exceptions**: Throws if no swap history exists or if the rollback operation fails.

### `Task<PluginOperationResult<SwapRecord?>> GetLastSwapRecordAsync`

Retrieves the most recent swap record, if any.

- **Return value**: A `Task<PluginOperationResult<SwapRecord?>>` containing the last `SwapRecord` or `null` if no swaps have occurred.
- **Exceptions**: Throws if the operation fails to retrieve the record.

### `Task<PluginOperationResult<List<SwapRecord>>> GetSwapHistoryAsync`

Retrieves the complete history of plugin swaps.

- **Return value**: A `Task<PluginOperationResult<List<SwapRecord>>>` containing the list of all swap records.
- **Exceptions**: Throws if the operation fails to retrieve the history.

### `void RegisterPostSwapCallback(Action<SwapRecord> callback)`

Registers a callback to be invoked after a successful plugin swap.

- **Parameters**:
  - `callback`: The action to invoke with the resulting `SwapRecord`.
- **Exceptions**: Throws if the callback registration fails (e.g., duplicate registration not allowed).

### `void UnregisterPostSwapCallback(Action<SwapRecord> callback)`

Removes a previously registered post-swap callback.

- **Parameters**:
  - `callback`: The action to remove.
- **Exceptions**: Throws if the callback is not registered.

### `bool CanSwap`

Determines whether a plugin swap is currently allowed based on system state.

- **Return value**: `true` if a swap can proceed; otherwise, `false`.
- **Thread safety**: Safe to call concurrently.

## Usage

### Example: Swapping a Plugin with Rollback Support
