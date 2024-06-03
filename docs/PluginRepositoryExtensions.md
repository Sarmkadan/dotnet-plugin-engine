# PluginRepositoryExtensions

Provides a set of extension methods for `IPluginRepository` that simplify common query operations such as retrieving a plugin by name, filtering plugins by status, and finding plugins that depend on a given plugin. These methods are asynchronous and support cancellation tokens.

## API

### GetByNameAsync
```csharp
public static async Task<Plugin?> GetByNameAsync(
    this IPluginRepository repository,
    string name,
    CancellationToken cancellationToken = default)
```
**Purpose:** Returns the plugin whose `Name` property matches the specified `name`, or `null` if no such plugin exists.  
**Parameters:**  
- `repository`: The plugin repository to query. Must not be `null`.  
- `name`: The exact name of the plugin to locate. Must not be `null` or whitespace.  
- `cancellationToken`: Optional token to observe for cancellation requests.  
**Return Value:** A `Task` that completes with the matching `Plugin` instance or `null`.  
**Exceptions:**  
- `ArgumentNullException` if `repository` or `name` is `null`.  
- `ArgumentException` if `name` consists only of whitespace.  
- `OperationCanceledException` if the token is triggered before completion.  
- Any exception thrown by the underlying repository implementation.

### GetByStatusAsync
```csharp
public static async Task<IEnumerable<Plugin>> GetByStatusAsync(
    this IPluginRepository repository,
    PluginStatus status,
    CancellationToken cancellationToken = default)
```
**Purpose:** Returns all plugins whose `Status` property equals the supplied `status`.  
**Parameters:**  
- `repository`: The plugin repository to query. Must not be `null`.  
- `status`: The `PluginStatus` value to filter by.  
- `cancellationToken`: Optional token to observe for cancellation requests.  
**Return Value:** A `Task` that completes with an enumerable of `Plugin` objects matching the status. The enumeration is lazy; evaluating it may throw exceptions from the repository.  
**Exceptions:**  
- `ArgumentNullException` if `repository` is `null`.  
- `OperationCanceledException` if the token is triggered before completion.  
- Any exception thrown by the underlying repository implementation.

### GetByStatusesAsync
```csharp
public static async Task<IEnumerable<Plugin>> GetByStatusesAsync(
    this IPluginRepository repository,
    IEnumerable<PluginStatus> statuses,
    CancellationToken cancellationToken = default)
```
**Purpose:** Returns all plugins whose `Status` property is contained in the supplied `statuses` collection.  
**Parameters:**  
- `repository`: The plugin repository to query. Must not be `null`.  
- `statuses`: A collection of `PluginStatus` values to match. Must not be `null` and must contain at least one element.  
- `cancellationToken`: Optional token to observe for cancellation requests.  
**Return Value:** A `Task` that completes with an enumerable of `Plugin` objects whose status matches any value in `statuses`.  
**Exceptions:**  
- `ArgumentNullException` if `repository` or `statuses` is `null`.  
- `ArgumentException` if `statuses` is empty.  
- `OperationCanceledException` if the token is triggered before completion.  
- Any exception thrown by the underlying repository implementation.

### GetDependentPluginsAsync
```csharp
public static async Task<IEnumerable<Plugin>> GetDependentPluginsAsync(
    this IPluginRepository repository,
    Plugin plugin,
    CancellationToken cancellationToken = default)
```
**Purpose:** Returns all plugins that declare a dependency on the specified `plugin`.  
**Parameters:**  
- `repository`: The plugin repository to query. Must not be `null`.  
- `plugin`: The plugin for which dependents are sought. Must not be `null` and must be present in the repository.  
- `cancellationToken`: Optional token to observe for cancellation requests.  
**Return Value:** A `Task` that completes with an enumerable of `Plugin` objects that depend on `plugin`. If no dependents exist, an empty enumeration is returned.  
**Exceptions:**  
- `ArgumentNullException` if `repository` or `plugin` is `null`.  
- `InvalidOperationException` if `plugin` is not found in the repository.  
- `OperationCanceledException` if the token is triggered before completion.  
- Any exception thrown by the underlying repository implementation.

## Usage

```csharp
using System.Threading.Tasks;
using DotNet.PluginEngine;

// Assume `repo` is an IPluginRepository implementation.
IPluginRepository repo = new PluginRepository();

// Retrieve a plugin by its exact name.
Plugin? logger = await repo.GetByNameAsync("LoggingPlugin");
if (logger != null)
{
    Console.WriteLine($"Found plugin: {logger.Name}");
}

// Get all plugins that are currently active.
IEnumerable<Plugin> activePlugins = await repo.GetByStatusAsync(PluginStatus.Active);
foreach (var p in activePlugins)
{
    Console.WriteLine($"Active: {p.Name}");
}
```

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.PluginEngine;

IPluginRepository repo = new PluginRepository();

// Find plugins that depend on a core library plugin on a base framework plugin.
Plugin basePlugin = await repo.GetByNameAsync("FrameworkCore");
if (basePlugin != null)
{
    IEnumerable<Plugin> dependents = await repo.GetDependentPluginsAsync(basePlugin);
    foreach (var dependent in dependents)
    {
        Console.WriteLine($"{dependent.Name} depends on {basePlugin.Name}");
    }
}

// Retrieve plugins with either a 'Staging' or 'Testing' status.
var statuses = new[] { PluginStatus.Staging, PluginStatus.Testing };
IEnumerable<Plugin> stagedOrTested = await repo.GetByStatusesAsync(repo, statuses);
```

## Notes

- The extension methods themselves are stateless and thread‑safe; however, the safety of concurrent calls depends on the underlying `IPluginRepository` implementation. Consumers should ensure that the repository instance used is thread‑safe if they intend to invoke these methods from multiple threads simultaneously.  
- Passing `null` for required arguments (`repository`, `name`, `statuses`, or `plugin`) will result in an `ArgumentNullException` before any asynchronous work begins.  
- Empty or whitespace‑only strings for `name` are considered invalid and will raise an `ArgumentException`.  
- The `GetByStatusesAsync` method treats a null or empty `statuses` argument as invalid; it does not return all plugins when the collection is empty.  
- `GetDependentPluginsAsync` assumes that the supplied `plugin` instance is known to the repository; if the plugin cannot be located, an `InvalidOperationException` is thrown.  
- All methods respect the supplied `CancellationToken`. If cancellation is requested before the operation completes, an `OperationCanceledException` is propagated from the returned task.  
- The returned `IEnumerable<Plugin>` sequences are lazy; enumerating them may trigger additional calls to the repository and therefore may throw exceptions that were not observed during the initial await. Consumers should handle exceptions during enumeration as appropriate.
