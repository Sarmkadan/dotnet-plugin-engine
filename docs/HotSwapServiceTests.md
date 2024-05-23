# HotSwapServiceTests

Unit tests for the `HotSwapService` that verify plugin assembly swapping, rollback, and history tracking functionality. The test class exercises both success and failure paths for plugin swapping operations, validates callback invocation, and ensures proper state management during hot-swap operations.

## API

### `HotSwapServiceTests`
Initializes a new instance of the `HotSwapServiceTests` class with default test dependencies (e.g., mock `IPluginLoader`, `IAssemblyResolver`, and `IHistoryTracker`). The test fixture sets up a controlled environment for plugin hot-swap operations.

### `void Dispose()`
Releases all resources used by the test fixture, including unmanaged resources and mock objects. Called automatically by the test framework after each test execution.

### `void CanSwap_ActiveOrLoadedPlugin_ReturnsTrue()`
Verifies that `CanSwap` returns `true` when the plugin is either actively running or loaded in memory. No parameters or return value inspection is required as this is a boolean assertion test.

### `void CanSwap_NonRunningPlugin_ReturnsFalse()`
Verifies that `CanSwap` returns `false` when the plugin is neither running nor loaded. No parameters or return value inspection is required as this is a boolean assertion test.

### `void CanSwap_NullPlugin_ReturnsFalse()`
Verifies that `CanSwap` returns `false` when the plugin argument is `null`. No parameters or return value inspection is required as this is a boolean assertion test.

### `async Task SwapPluginAsync_EmptyPath_ReturnsFailure()`
Tests that calling `SwapPluginAsync` with an empty or whitespace path returns a failure result. Validates that the service rejects invalid file paths early in the pipeline.

### `async Task SwapPluginAsync_MissingFile_ReturnsFailure()`
Tests that calling `SwapPluginAsync` with a non-existent file path returns a failure result. Validates that the service performs file existence checks before attempting to load or swap assemblies.

### `async Task SwapPluginAsync_PluginNotLoaded_ReturnsFailure()`
Tests that calling `SwapPluginAsync` when the target plugin is not currently loaded returns a failure result. Validates that the service checks plugin load state before proceeding with a swap.

### `async Task SwapPluginAsync_Success_RecordsSwapHistory()`
Tests that a successful `SwapPluginAsync` call records the swap event in the history tracker. Validates that the service maintains an audit trail of hot-swap operations for rollback and diagnostics.

### `async Task SwapPluginAsync_Success_InvokesPostSwapCallback()`
Tests that a successful `SwapPluginAsync` call invokes any registered post-swap callbacks. Validates that the service honors callback contracts after a successful swap operation.

### `async Task RollbackSwapAsync_NoHistory_ReturnsFailure()`
Tests that calling `RollbackSwapAsync` when no swap history exists returns a failure result. Validates that the service checks for prior swap events before attempting rollback.

### `async Task RollbackSwapAsync_AfterSuccessfulSwap_ReloadsOldAssembly()`
Tests that calling `RollbackSwapAsync` after a successful swap reloads the original assembly and restores the previous state. Validates that the service can revert to a known-good state after a hot-swap.

### `async Task GetSwapHistoryAsync_NoSwaps_ReturnsEmptyList()`
Tests that calling `GetSwapHistoryAsync` when no swaps have occurred returns an empty list. Validates that the history tracker returns an empty collection when no operations have been recorded.

### `void UnregisterPostSwapCallback_RemovesCallback()`
Verifies that unregistering a post-swap callback removes it from the active callback list. No parameters or return value inspection is required as this is a callback management test.
