# IPluginLoaderService

`IPluginLoaderService` is the asynchronous API for loading, unloading, reloading, and querying plugins. The built-in `PluginLoaderService` implementation also discovers lifecycle implementations in plugin assemblies and tracks each loaded plugin together with its load context and lifecycle instances.

The interface is in `PluginEngine.Services.Abstractions`; returned plugin records use `PluginEngine.Domain.Entities.Plugin`.

## Signatures

| Method | Signature | Result |
| --- | --- | --- |
| Load one plugin | `Task<Plugin> LoadPluginAsync(string assemblyPath, CancellationToken cancellationToken = default)` | A newly loaded plugin with a new ID. |
| Unload one plugin | `Task<bool> UnloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default)` | `true` when the tracked plugin is unloaded; `false` when it is not found or unloading fails. |
| Get one plugin | `Task<Plugin?> GetLoadedPluginAsync(Guid pluginId, CancellationToken cancellationToken = default)` | The tracked plugin, or `null`. |
| Get all plugins | `Task<IEnumerable<Plugin>> GetAllLoadedPluginsAsync(CancellationToken cancellationToken = default)` | A snapshot list of the currently tracked plugins. |
| Test whether loaded | `Task<bool> IsPluginLoadedAsync(Guid pluginId, CancellationToken cancellationToken = default)` | Whether the ID is present in the loader's tracking dictionary. |
| Load a directory | `Task<IEnumerable<Plugin>> LoadPluginsFromDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)` | The plugins successfully loaded from top-level `*.dll` files. |
| Reload one plugin | `Task<Plugin> ReloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default)` | A newly loaded plugin instance with a new ID. |

## Loading a plugin

`LoadPluginAsync` rejects a blank path with `ArgumentException` and a missing file with `PluginLoadException`. It converts the path to a full path, creates a collectible load context, and loads the assembly from that path. The returned `Plugin` receives a new ID, the assembly name and version, the supplied assembly path, the load-context name, and an initial `Loading` status.

The implementation looks for `plugin.json` beside the assembly, then for a file named after the assembly with the engine metadata extension. Valid JSON is deserialized into `Plugin.Metadata`; invalid JSON is logged and ignored when a logger is available. If metadata declares an engine version constraint and an `IVersioningService` is available from the optional service provider, an unsatisfied constraint unloads the new context and throws `PluginIncompatibleException`.

Every concrete, non-abstract assembly type assignable to `IPluginLifecycle` is instantiated with `Activator.CreateInstance`. The loader calls `OnBeforeLoadAsync`, changes the plugin status to `Loaded`, records the plugin, context, and lifecycle instances, and then calls `OnAfterLoadAsync`. Non-`PluginException` failures inside the load operation are wrapped in `PluginLoadException`.

## Unloading a plugin

`UnloadPluginAsync` returns `false` if the ID is not tracked. For a tracked plugin it sets the status to `Unloading`, calls `OnBeforeUnloadAsync` for each lifecycle instance, removes the dictionary entry, clears the lifecycle list, asks an available `IHotReloadService` to remove callbacks for the context, and calls `AssemblyLoadContext.Unload()`. It then explicitly runs garbage collection and waits for pending finalizers.

The code attempts to iterate the lifecycle list for `OnAfterUnloadAsync` after that list has been cleared, so the built-in implementation does not invoke any after-unload callbacks. It then marks the detached `Plugin` object as `Unloaded` and returns `true`. Any exception in this process is caught, the plugin is marked `Failed`, and the method returns `false`.

Calling `Unload()` makes the context eligible for collection; actual reclamation still depends on no other strong references to assemblies or objects from that context remaining.

## Querying loaded plugins

`GetLoadedPluginAsync`, `GetAllLoadedPluginsAsync`, and `IsPluginLoadedAsync` read the in-memory dictionary under a lock. `GetAllLoadedPluginsAsync` materializes a new `List<Plugin>`, so later changes to the dictionary do not change that returned collection. The `Plugin` objects themselves are not cloned.

## Loading a directory

`LoadPluginsFromDirectoryAsync` throws `DirectoryNotFoundException` when the directory does not exist. It enumerates top-level files matching `*.dll` and loads them sequentially. A failure for one file is written to `Console` and does not stop later files; only successfully loaded plugins are returned.

## Reloading a plugin

`ReloadPluginAsync` first queries the tracked plugin. A missing ID produces `PluginException` with code `PLUGIN_NOT_FOUND`. Otherwise, it saves the assembly path, awaits unloading, and loads that path again. It does not inspect the Boolean result of unloading. Because loading creates a new `Plugin`, a successful reload has a new plugin ID and a new load-context name.

## Cancellation behavior

All interface methods accept an optional token, but the built-in implementation observes it at different points:

- `LoadPluginAsync` passes the token to `Task.Run` and to the before- and after-load lifecycle callbacks. Cancellation arising inside its `try` block is a non-`PluginException` and is therefore wrapped in `PluginLoadException`.
- The three query methods pass the token to `Task.Run`; cancellation can therefore complete them with cancellation before their delegate runs.
- `UnloadPluginAsync` does not pass the token to either of its `Task.Run` calls. It passes the token to lifecycle callbacks, but catches every exception, including cancellation exceptions, and returns `false` after marking the plugin `Failed`.
- `LoadPluginsFromDirectoryAsync` has no separate cancellation check. It forwards the token to each load, then catches every exception from that load, writes a failure message, and continues to the next DLL.
- `ReloadPluginAsync` forwards the token to its query, unload, and load operations. Cancellation during the initial query can propagate as cancellation; cancellation during unload can be converted to `false`, and the method still proceeds to load; cancellation during load follows the wrapping behavior described above.

## AssemblyLoadContext isolation

`PluginLoaderService` creates one private `PluginAssemblyLoadContext` per load. Each context is named `PluginContext_<guid>` and constructed with `isCollectible: true`, enabling unload and reload. An `AssemblyDependencyResolver` rooted at the plugin assembly path resolves that plugin's managed dependencies and native libraries. Resolved managed assemblies are loaded with `LoadFromAssemblyPath`, and resolved native libraries with `LoadUnmanagedDllFromPath`.

When the resolver cannot resolve a managed dependency, `Load` returns `null`; when it cannot resolve a native library, `LoadUnmanagedDll` returns `IntPtr.Zero`. Those return values allow the runtime's normal fallback behavior. The implementation therefore gives every plugin a distinct collectible dependency-resolution context, while assemblies supplied by fallback may still come from outside that context.

The tracking dictionary is guarded by a single lock and stores `(Plugin, AssemblyLoadContext, List<IPluginLifecycle>)` for each plugin ID. Removing that entry and clearing lifecycle references are part of the implementation's unload strategy.

## Example

The service is registered by the library as a singleton and can be resolved through dependency injection:

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Services.Abstractions;

IPluginLoaderService loader =
    serviceProvider.GetRequiredService<IPluginLoaderService>();

using var cancellation = new CancellationTokenSource();

var plugin = await loader.LoadPluginAsync(
    "/opt/my-app/plugins/ExamplePlugin.dll",
    cancellation.Token);

if (await loader.IsPluginLoadedAsync(plugin.Id, cancellation.Token))
{
    Console.WriteLine($"{plugin.Name} {plugin.Version} is in {plugin.LoadContextId}");
}

bool unloaded = await loader.UnloadPluginAsync(plugin.Id, cancellation.Token);
Console.WriteLine(unloaded ? "Unloaded" : "Not unloaded");
```
