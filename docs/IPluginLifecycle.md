# `IPluginLifecycle`

`IPluginLifecycle` lets a plugin assembly participate in the load and unload flow implemented by `PluginLoaderService`.

During loading, `PluginLoaderService` examines every type returned by `Assembly.GetTypes()`. It creates an instance of each concrete, non-abstract type assignable to `IPluginLifecycle` by calling `Activator.CreateInstance(type)`. Consequently, a lifecycle implementation must be constructible this way (normally with a public parameterless constructor). The created instances are retained with the loaded plugin until unloading.

Callbacks are awaited sequentially in the order in which the discovered lifecycle instances appear in the loader's list. The `CancellationToken` supplied to `LoadPluginAsync`, `UnloadPluginAsync`, or indirectly `ReloadPluginAsync` is passed unchanged to each callback.

## Signatures and invocation points

| Method | Signature | When `PluginLoaderService` invokes it |
| --- | --- | --- |
| Before load | `Task OnBeforeLoadAsync(CancellationToken cancellationToken = default)` | After the assembly and metadata have been loaded and all lifecycle implementations have been instantiated, while the new `Plugin` still has `PluginStatus.Loading`. |
| After load | `Task OnAfterLoadAsync(CancellationToken cancellationToken = default)` | After the plugin status is changed to `PluginStatus.Loaded` and the plugin, load context, and lifecycle instances have been added to the loaded-plugin dictionary. |
| Before unload | `Task OnBeforeUnloadAsync(CancellationToken cancellationToken = default)` | After a loaded plugin is found and its status is changed to `PluginStatus.Unloading`, but before it is removed from the loaded-plugin dictionary, lifecycle references are cleared, hot-reload callbacks are removed, and the assembly load context is unloaded. |
| After unload | `Task OnAfterUnloadAsync(CancellationToken cancellationToken = default)` | The loader contains an invocation loop after unloading the assembly load context. However, it clears the lifecycle list before that loop, so the current implementation invokes this method on no lifecycle instances. |

## Load lifecycle

`LoadPluginAsync` performs the lifecycle portion of loading in this order:

1. Discover and instantiate all concrete `IPluginLifecycle` implementations in the plugin assembly.
2. Await `OnBeforeLoadAsync` on each instance.
3. Set the plugin status to `Loaded` and store it in the loaded-plugin dictionary with its load context and lifecycle instances.
4. Await `OnAfterLoadAsync` on each instance.

If a lifecycle callback throws during loading, the remaining load operation does not continue. A non-`PluginException` is caught by `LoadPluginAsync` and wrapped in a `PluginLoadException`.

## Unload lifecycle

`UnloadPluginAsync` first returns `false` without invoking callbacks when the plugin ID is not present. For a loaded plugin, it sets the status to `Unloading` and then:

1. Awaits `OnBeforeUnloadAsync` on each retained lifecycle instance.
2. Removes the plugin from the loaded-plugin dictionary.
3. Clears the lifecycle list, removes hot-reload callbacks, unloads the assembly load context, and requests garbage collection.
4. Iterates the now-empty lifecycle list for `OnAfterUnloadAsync`; therefore no `OnAfterUnloadAsync` callback currently runs.
5. Sets the plugin status to `Unloaded` and returns `true`.

Any exception inside the unload `try` block is caught; the plugin status is set to `Failed` and `UnloadPluginAsync` returns `false`.

`ReloadPluginAsync` uses the same flow: it calls `UnloadPluginAsync` for the existing plugin and then calls `LoadPluginAsync` for its assembly path.

## Minimal implementation

```csharp
using System.Threading;
using System.Threading.Tasks;
using PluginEngine.Execution;

public sealed class PluginLifecycle : IPluginLifecycle
{
    public Task OnBeforeLoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task OnAfterLoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnBeforeUnloadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnAfterUnloadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
```

The example has an implicit public parameterless constructor, allowing `PluginLoaderService` to instantiate it with `Activator.CreateInstance`.
