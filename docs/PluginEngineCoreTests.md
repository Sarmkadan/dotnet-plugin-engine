# PluginEngineCoreTests

The `PluginEngineCoreTests` class provides a suite of unit tests for the `PluginEngineCore` type. It validates the core engine's interactions with its dependencies (`IPluginManager`, `IPluginLoader`, `IDependencyResolver`, `IVersioningService`, `IHotReloadService`, and `PluginEngineOptions`) and verifies correct behavior of its public methods, including initialization, shutdown, status retrieval, plugin loading/unloading, and health reporting. The class implements `IDisposable` to clean up any test‑specific resources (e.g., mock objects, temporary directories).

## API

### `public PluginEngineCoreTests()`

Initializes a new instance of the test class. Sets up the necessary mock dependencies and test fixtures required by all test methods.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None.

### `public void Dispose()`

Releases all resources used by the test instance. Typically disposes of mock objects, cancellation token sources, and any other disposable test infrastructure.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None.

### `public void Constructor_WithNullPluginManager_ThrowsArgumentNullException()`

Verifies that the `PluginEngineCore` constructor throws an `ArgumentNullException` when the `pluginManager` parameter is `null`.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None (the test itself is expected to pass; the exception is asserted inside the test).

### `public void Constructor_WithNullPluginLoader_ThrowsArgumentNullException()`

Verifies that the `PluginEngineCore` constructor throws an `ArgumentNullException` when the `pluginLoader` parameter is `null`.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None.

### `public void Constructor_WithNullDependencyResolver_ThrowsArgumentNullException()`

Verifies that the `PluginEngineCore` constructor throws an `ArgumentNullException` when the `dependencyResolver` parameter is `null`.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None.

### `public void Constructor_WithNullVersioningService_ThrowsArgumentNullException()`

Verifies that the `PluginEngineCore` constructor throws an `ArgumentNullException` when the `versioningService` parameter is `null`.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None.

### `public void Constructor_WithNullHotReloadService_ThrowsArgumentNullException()`

Verifies that the `PluginEngineCore` constructor throws an `ArgumentNullException` when the `hotReloadService` parameter is `null`.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None.

### `public void Constructor_WithNullOptions_ThrowsArgumentNullException()`

Verifies that the `PluginEngineCore` constructor throws an `ArgumentNullException` when the `options` parameter is `null`.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None.

### `public async Task InitializeAsync_CallsPluginManagerInitialize()`

Ensures that `PluginEngineCore.InitializeAsync()` invokes `IPluginManager.InitializeAsync()` exactly once.  
**Parameters:** None.  
**Returns:** A `Task` representing the asynchronous test operation.  
**Throws:** None (test failure is reported by the test framework).

### `public async Task InitializeAsync_WithCancellationToken_PassesTokenToManager()`

Verifies that the `CancellationToken` supplied to `PluginEngineCore.InitializeAsync(CancellationToken)` is forwarded to `IPluginManager.InitializeAsync(CancellationToken)`.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None.

### `public async Task ShutdownAsync_CallsPluginManagerShutdown()`

Ensures that `PluginEngineCore.ShutdownAsync()` invokes `IPluginManager.ShutdownAsync()` exactly once.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None.

### `public async Task ShutdownAsync_WithCancellationToken_PassesTokenToManager()`

Verifies that the `CancellationToken` supplied to `PluginEngineCore.ShutdownAsync(CancellationToken)` is forwarded to `IPluginManager.ShutdownAsync(CancellationToken)`.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None.

### `public async Task GetStatusAsync_ReturnsStatusFromPluginManager()`

Confirms that `PluginEngineCore.GetStatusAsync()` returns the same `PluginEngineStatus` object that was returned by `IPluginManager.GetStatusAsync()`.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None.

### `public async Task LoadAllPluginsAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()`

Verifies that `PluginEngineCore.LoadAllPluginsAsync()` throws a `DirectoryNotFoundException` when the configured plugin directory does not exist.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None (the exception is asserted inside the test).

### `public async Task LoadAllPluginsAsync_CallsPluginLoaderWithCorrectDirectory()`

Ensures that `PluginEngineCore.LoadAllPluginsAsync()` calls `IPluginLoader.LoadAllAsync()` with the directory path specified in the engine options.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None.

### `public async Task LoadAllPluginsAsync_WithCancellationToken_PassesTokenToLoader()`

Verifies that the `CancellationToken` supplied to `PluginEngineCore.LoadAllPluginsAsync(CancellationToken)` is forwarded to `IPluginLoader.LoadAllAsync(CancellationToken)`.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None.

### `public async Task UnloadAllPluginsAsync_CallsUnloadOnEachPlugin()`

Confirms that `PluginEngineCore.UnloadAllPluginsAsync()` calls `IPlugin.UnloadAsync()` on every plugin currently managed by the engine.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None.

### `public async Task UnloadAllPluginsAsync_WithCancellationToken_PassesTokenToOperations()`

Verifies that the `CancellationToken` supplied to `PluginEngineCore.UnloadAllPluginsAsync(CancellationToken)` is forwarded to each plugin’s `UnloadAsync(CancellationToken)` call.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None.

### `public async Task GetHealthInfoAsync_ReturnsFormattedHealthReport()`

Ensures that `PluginEngineCore.GetHealthInfoAsync()` returns a properly formatted health report string, typically derived from the aggregated health status of all loaded plugins.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** None.

### `public void PluginManager_ReturnsConfiguredPluginManager()`

Verifies that the `PluginEngineCore.PluginManager` property returns the same `IPluginManager` instance that was passed to the constructor.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None.

## Usage

The following examples demonstrate typical ways to work with the `PluginEngineCoreTests` class.

### Example 1: Running tests manually with explicit cleanup

```csharp
using var tests = new PluginEngineCoreTests();

// Run a subset of tests
tests.Constructor_WithNullPluginManager_ThrowsArgumentNullException();
tests.InitializeAsync_CallsPluginManagerInitialize().GetAwaiter().GetResult();
tests.GetHealthInfoAsync_ReturnsFormattedHealthReport().GetAwaiter().GetResult();

// Dispose is called automatically at the end of the using block
```

### Example 2: Using the test class as a fixture in an xUnit test runner

```csharp
public class PluginEngineCoreIntegrationTests : IClassFixture<PluginEngineCoreTests>
{
    private readonly PluginEngineCoreTests _fixture;

    public PluginEngineCoreIntegrationTests(PluginEngineCoreTests fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void AllConstructorNullChecksPass()
    {
        _fixture.Constructor_WithNullPluginManager_ThrowsArgumentNullException();
        _fixture.Constructor_WithNullPluginLoader_ThrowsArgumentNullException();
        _fixture.Constructor_WithNullOptions_ThrowsArgumentNullException();
    }
}
```

## Notes

- **Edge cases:**  
  - Constructor tests rely on the exact parameter order and type of `PluginEngineCore`’s constructor. Any refactoring of the constructor signature will require updating these tests.  
  - The `LoadAllPluginsAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException` test assumes that the engine options contain a directory path that does not exist on the test machine. The test sets up the mock options accordingly.  
  - Cancellation token tests verify that the token passed to the engine method is the same object (or structurally identical) to the one received by the mocked dependency. They do not test actual cancellation behavior (e.g., `OperationCanceledException`).

- **Thread safety:**  
  - Instances of `PluginEngineCoreTests` are not thread‑safe. Tests should not be executed concurrently on the same instance.  
  - The class implements `IDisposable` to release resources that may be shared across tests (e.g., mock objects, cancellation token sources). Always dispose of the instance after use, preferably via a `using` statement.  
  - Async test methods use `Task` and are intended to be run sequentially within a single test run. Parallel test execution across different instances is safe as long as each instance has its own set of mocks.
