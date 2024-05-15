# HotReloadService

Provides runtime support for detecting file changes in plugins and applying hot‑reload updates without restarting the host application. The service coordinates file system monitoring, invokes user‑provided callbacks, and reports statistics about reload attempts.

## API

### HotReloadService()
Initializes a new instance of the `HotReloadService`.  
- **Parameters:** None.  
- **Return value:** A ready‑to‑use service instance.  
- **Exceptions:** May throw `ArgumentNullException` if required dependencies supplied via dependency injection are null; may throw `ObjectDisposedException` if the service has already been disposed.

### StartHotReloadMonitoringAsync()
Begins monitoring the file system for changes that trigger hot reload.  
- **Parameters:** None.  
- **Return value:** A `Task` that completes when monitoring is active.  
- **Exceptions:**  
  - `InvalidOperationException` – monitoring is already started.  
  - `ObjectDisposedException` – the service has been disposed.  
  - `PlatformNotSupportedException` – the underlying OS does not support the required file watcher.

### StopHotReloadMonitoringAsync()
Stops the file system monitoring and releases associated resources.  
- **Parameters:** None.  
- **Return value:** A `Task` that completes when monitoring has been stopped.  
- **Exceptions:**  
  - `InvalidOperationException` – monitoring is not currently running.  
  - `ObjectDisposedException` – the service has been disposed.

### CanHotReload
Gets a value indicating whether the current runtime and operating system support hot reload.  
- **Parameters:** None.  
- **Return value:** `true` if hot reload is available; otherwise `false`.  
- **Exceptions:** None.

### HotReloadPluginAsync()
Attempts to apply a hot reload to the currently loaded plugin.  
- **Parameters:** None.  
- **Return value:** A `Task<bool>` where `true` indicates the reload succeeded and `false` indicates it was skipped or failed.  
- **Exceptions:**  
  - `ArgumentNullException` – the plugin context required for the operation is null.  
  - `InvalidOperationException` – hot reload monitoring is not active.  
  - `PluginLoadException` – the plugin could not be reloaded due to load or version mismatches.  
  - `ObjectDisposedException` – the service has been disposed.

### GetStatisticsAsync()
Retrieves statistics about hot reload activity since the service started.  
- **Parameters:** None.  
- **Return value:** A `Task<HotReloadStatistics>` containing counters such as total attempts, successes, and failures.  
- **Exceptions:**  
  - `ObjectDisposedException` – the service has been disposed.  
  - `InvalidOperationException` – statistics are not available because monitoring has never been started.

### RegisterHotReloadCallback(Action<HotReloadContext> callback)
Registers a delegate to be invoked each time a hot reload occurs.  
- **Parameters:**  
  - `callback` – The method to call when a hot reload is performed; receives a `HotReloadContext` describing the reload.  
- **Return value:** None.  
- **Exceptions:**  
  - `ArgumentNullException` – `callback` is `null`.  
  - `ObjectDisposedException` – the service has been disposed.  
  - `InvalidOperationException` – the service is not initialized correctly.

### UnregisterHotReloadCallback(Action<HotReloadContext> callback)
Removes a previously registered hot reload callback.  
- **Parameters:**  
  - `callback` – The delegate that was previously registered.  
- **Return value:** None.  
- **Exceptions:**  
  - `ArgumentNullException` – `callback` is `null`.  
  - `InvalidOperationException` – the callback was not found (may have already been unregistered).  
  - `ObjectDisposedException` – the service has been disposed.

### GetHotReloadStatusAsync()
Queries the current hot reload status (e.g., idle, monitoring, reloading).  
- **Parameters:** None.  
- **Return value:** A `Task<HotReloadStatus?>` where the result is the current status or `null` if the status cannot be determined.  
- **Exceptions:**  
  - `ObjectDisposedException` – the service has been disposed.  
  - `InvalidOperationException` – the service is not in a state where status can be reported.

### RemoveCallbacksForContext(object pluginContext)
Removes all hot reload callbacks associated with a specific plugin context.  
- **Parameters:**  
  - `pluginContext` – The identifier of the plugin whose callbacks should be removed.  
- **Return value:** None.  
- **Exceptions:**  
  - `ArgumentNullException` – `pluginContext` is `null`.  
  - `ObjectDisposedException` – the service has been disposed.  
  - `InvalidOperationException` – no callbacks are registered for the supplied context.

## Usage

### Basic monitoring and manual reload
```csharp
using System.Threading.Tasks;
using DotNetPluginEngine.Services;

public class PluginHost
{
    private readonly HotReloadService _hotReload;

    public PluginHost(HotReloadService hotReload)
    {
        _hotReload = hotReload;
    }

    public async Task InitializeAsync()
    {
        if (_hotReload.CanHotReload)
        {
            await _hotReload.StartHotReloadMonitoringAsync();

            // Optional: subscribe to reload events
            _hotReload.RegisterHotReloadCallback(ctx =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Hot reload triggered for plugin {ctx.PluginId} at {ctx.Timestamp}");
            });
        }
    }

    public async Task TryReloadPluginAsync()
    {
        if (_hotReload.CanHotReload)
        {
            bool reloaded = await _hotReload.HotReloadPluginAsync();
            System.Diagnostics.Debug.WriteLine(
                reloaded ? "Plugin hot‑reloaded successfully." : "Hot reload not applied.");
        }
    }

    public async Task ShutdownAsync()
    {
        await _hotReload.StopHotReloadMonitoringAsync();
        // Callbacks are automatically cleared when the service is disposed.
    }
}
```

### Retrieving statistics and status
```csharp
using System.Threading.Tasks;
using DotNetPluginEngine.Services;

public class DiagnosticReporter
{
    private readonly HotReloadService _hotReload;

    public DiagnosticReporter(HotReloadService hotReload)
    {
        _hotReload = hotReload;
    }

    public async Task ReportAsync()
    {
        var status = await _hotReload.GetHotReloadStatusAsync();
        var stats  = await _hotReload.GetStatisticsAsync();

        System.Diagnostics.Console.WriteLine(
            $"Hot reload status: {status ?? "unknown"}");
        System.Diagnostics.Console.WriteLine(
            $"Attempts: {stats.Attempts}, Successes: {stats.Successes}, Failures: {stats.Failures}");
    }
}
```

## Notes
- The service is **thread‑safe** for all public members; concurrent calls to `StartHotReloadMonitoringAsync`, `StopHotReloadMonitoringAsync`, `HotReloadPluginAsync`, and the callback registration methods will not corrupt internal state.  
- Callbacks are invoked on a background thread owned by the service; they should avoid long‑running work or blocking calls to prevent delaying subsequent reloads.  
- If `CanHotReload` returns `false`, all reload‑related methods (`StartHotReloadMonitoringAsync`, `StopHotReloadMonitoringAsync`, `HotReloadPluginAsync`, `GetHotReloadStatusAsync`) will still execute but will have no effect; callers should check this property before attempting reloads.  
- Failing to unregister a callback via `UnregisterHotReloadCallback` or `RemoveCallbacksForContext` can lead to memory leaks because the service holds strong references to the delegates.  
- `RemoveCallbacksForContext` is intended for scenarios where a plugin is unloaded; it safely removes all associated callbacks without affecting others.  
- The service throws `ObjectDisposedException` on any member after `Dispose` has been called; callers should ensure the service lifetime exceeds the usage scope.  
- On platforms lacking file‑system watch support (e.g., certain constrained environments), `StartHotReloadMonitoringAsync` will throw `PlatformNotSupportedException`. In such cases, hot reload functionality is unavailable and `CanHotReload` will be `false`.
