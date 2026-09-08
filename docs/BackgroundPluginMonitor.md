# `BackgroundPluginMonitor`

`BackgroundPluginMonitor` is a sealed `BackgroundService` whose source is located at
`src/PluginEngine/BackgroundServices/BackgroundPluginMonitor.cs`. It sets up file-system
notifications for plugin assemblies and starts the configured hot-reload service.

## Build status

The class is currently **excluded from the `PluginEngine` build**. The project file contains
a `Compile Remove` entry for `BackgroundServices\BackgroundPluginMonitor.cs`, so the type is
not present in the compiled assembly and cannot currently be registered or instantiated by a
consumer. The example below illustrates the constructor dependencies and intended hosted-service
registration if that exclusion is removed; it does not describe the current compiled API.

## Signatures

| Member | Signature | Behavior |
| --- | --- | --- |
| Constructor | `BackgroundPluginMonitor(IPluginManagerService pluginManager, IHotReloadService hotReloadService, ILogger<BackgroundPluginMonitor> logger, IOptions<PluginEngineOptions> options)` | Stores the supplied services, logger, and current options value. |
| Background execution | `protected override Task ExecuteAsync(CancellationToken stoppingToken)` | Exits immediately when hot reload is disabled; otherwise initializes the watcher, starts hot-reload monitoring, and waits for cancellation. |
| Stop | `public override Task StopAsync(CancellationToken cancellationToken)` | Logs that the service is stopping and delegates to `BackgroundService.StopAsync`. |
| Watcher initialization | `private void InitializeFileSystemWatcher()` | Creates and subscribes a watcher when the configured plugin directory exists. Initialization failures are logged and swallowed. |
| Created handler | `private void OnPluginFileCreated(object sender, FileSystemEventArgs e)` | Schedules an attempted plugin load after a one-second delay. |
| Changed handler | `private void OnPluginFileChanged(object sender, FileSystemEventArgs e)` | Schedules a debug log after a 500-millisecond delay; it does not reload the plugin. |
| Deleted handler | `private void OnPluginFileDeleted(object sender, FileSystemEventArgs e)` | Logs the deletion; it does not unload the plugin. |

## What it monitors

When `PluginEngineOptions.EnableHotReload` is `false`, execution logs that monitoring is
disabled and returns without creating a watcher or starting the hot-reload service.

When enabled, the service attempts to watch `PluginEngineOptions.PluginDirectory`. If that
directory does not exist, it logs a warning and does not create its own watcher. It still calls
`IHotReloadService.StartHotReloadMonitoringAsync()` after the initialization attempt.

Its `FileSystemWatcher` has the following settings:

- `Filter` is `*.dll`.
- `NotifyFilter` includes `NotifyFilters.FileName` and `NotifyFilters.LastWrite`.
- `EnableRaisingEvents` is set to `true`.
- Handlers are attached for `Created`, `Changed`, and `Deleted` events.
- `IncludeSubdirectories` is not set, so the watcher retains its framework default.

The monitor itself does not periodically enumerate or poll the plugin directory. After setup,
`ExecuteAsync` runs a loop containing `Task.Delay(5000, stoppingToken)`. This five-second delay
keeps the background service alive and observes cancellation; all file reactions come from
`FileSystemWatcher` events.

## Timing and file-change reactions

| Trigger or loop | Interval or delay | Reaction |
| --- | --- | --- |
| Service lifetime loop | 5 seconds | Waits with the stopping token, then repeats; it does not inspect files. |
| DLL created | 1 second | Uses a continuation to call `IPluginManagerService.LoadPluginAsync(e.FullPath)`, then logs the loaded plugin name and version. The delay is intended to allow the file write to finish. |
| DLL changed | 500 milliseconds | Uses a continuation to write a debug message. The source comments that matching the file to a plugin ID and reloading would be production behavior, but no reload occurs here. |
| DLL deleted | No added delay | Logs the file name only. No unload operation occurs. |

Exceptions raised while loading a created plugin or processing a changed notification are logged
inside their continuations. An `OperationCanceledException` from the main execution path is logged
as cancellation. Other execution errors are logged, and the monitor's watcher is disposed in the
`finally` block.

## Registration example

Because the source file is excluded from compilation, this registration only applies if the type
is included in a build. The constructor dependencies must also be registered:

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.BackgroundServices;
using PluginEngine.Configuration;
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;

services.Configure<PluginEngineOptions>(options =>
{
    options.EnableHotReload = true;
    options.PluginDirectory = "plugins";
});

services.AddSingleton<IPluginManagerService, PluginManagerService>();
services.AddSingleton<IHotReloadService, HotReloadService>();
services.AddHostedService<BackgroundPluginMonitor>();
```

The monitor passes no cancellation token to `StartHotReloadMonitoringAsync` or
`LoadPluginAsync`. Its own stopping token is used only by the five-second lifetime delay and by
the base hosted-service shutdown path.
