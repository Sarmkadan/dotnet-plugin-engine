# IPluginRepository

`IPluginRepository` defines asynchronous storage operations for plugin records, their dependencies, and their capabilities. It is declared in `PluginEngine.Data.Repositories` and works with the entity types in `PluginEngine.Domain.Entities`.

Every method accepts an optional `CancellationToken`. The interface declares 15 methods in the current source: 10 plugin operations, 3 dependency operations, and 2 capability operations.

## Plugin operations

| Method | Signature | Result |
| --- | --- | --- |
| Add | `Task<Plugin> AddAsync(Plugin plugin, CancellationToken cancellationToken = default)` | Returns the added plugin. |
| Update | `Task<bool> UpdateAsync(Plugin plugin, CancellationToken cancellationToken = default)` | Reports whether an existing plugin was updated. |
| Delete | `Task<bool> DeleteAsync(Guid pluginId, CancellationToken cancellationToken = default)` | Reports whether a plugin with the given ID was deleted. |
| Get by ID | `Task<Plugin?> GetByIdAsync(Guid pluginId, CancellationToken cancellationToken = default)` | Returns the matching plugin, or `null` when none is found. |
| Get by name | `Task<Plugin?> GetByNameAsync(string name, CancellationToken cancellationToken = default)` | Returns the matching plugin, or `null` when none is found. |
| Get all | `Task<IEnumerable<Plugin>> GetAllAsync(CancellationToken cancellationToken = default)` | Returns all plugins. |
| Get by status | `Task<IEnumerable<Plugin>> GetByStatusAsync(PluginStatus status, CancellationToken cancellationToken = default)` | Returns plugins whose status matches the supplied value. |
| Exists | `Task<bool> ExistsAsync(Guid pluginId, CancellationToken cancellationToken = default)` | Reports whether the ID exists. |
| Count | `Task<int> CountAsync(CancellationToken cancellationToken = default)` | Returns the number of plugins. |
| Search | `Task<IEnumerable<Plugin>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)` | Returns plugins matching the supplied search term. |

The interface itself does not prescribe validation, uniqueness, ordering, matching rules, or exception behavior beyond its types and nullable return values. Those details belong to an implementation.

## Dependency operations

| Method | Signature | Result |
| --- | --- | --- |
| Add dependency | `Task<bool> AddDependencyAsync(Guid pluginId, PluginDependency dependency, CancellationToken cancellationToken = default)` | Reports whether the dependency was added. |
| Remove dependency | `Task<bool> RemoveDependencyAsync(Guid pluginId, Guid dependencyId, CancellationToken cancellationToken = default)` | Reports whether the dependency record was removed. |
| Get dependencies | `Task<IEnumerable<PluginDependency>> GetDependenciesAsync(Guid pluginId, CancellationToken cancellationToken = default)` | Returns the dependency records associated with a plugin ID. |

`dependencyId` identifies a `PluginDependency` record by its `Id`; it is not the value of `PluginDependency.DependencyPluginId`.

## Capability operations

| Method | Signature | Result |
| --- | --- | --- |
| Add capability | `Task<bool> AddCapabilityAsync(Guid pluginId, PluginCapability capability, CancellationToken cancellationToken = default)` | Reports whether the capability was added. |
| Get capabilities | `Task<IEnumerable<PluginCapability>> GetCapabilitiesAsync(Guid pluginId, CancellationToken cancellationToken = default)` | Returns the capability records associated with a plugin ID. |

The interface has no capability removal method.

## Example

The following example uses only operations declared by `IPluginRepository`:

```csharp
using PluginEngine.Data.Repositories;
using PluginEngine.Domain.Entities;

static async Task RegisterPluginAsync(
    IPluginRepository repository,
    CancellationToken cancellationToken)
{
    var plugin = await repository.AddAsync(
        new Plugin
        {
            Name = "Reporting",
            Description = "Creates reports",
            Version = "1.0.0",
            Author = "Example",
            AssemblyPath = "plugins/Reporting.dll"
        },
        cancellationToken);

    await repository.AddCapabilityAsync(
        plugin.Id,
        new PluginCapability
        {
            PluginId = plugin.Id,
            Name = "ReportGeneration",
            Version = "1.0.0",
            InterfaceTypeName = "Example.IReportGenerator"
        },
        cancellationToken);

    var capabilities = await repository.GetCapabilitiesAsync(
        plugin.Id,
        cancellationToken);

    foreach (var capability in capabilities)
        Console.WriteLine(capability.GetDisplayName());
}
```

## In-memory `PluginRepository`

`PluginRepository` is the in-memory implementation included with the project. It stores plugins, dependencies, and capabilities in three dictionaries and protects access with a single lock. Its methods return already-completed tasks rather than performing external I/O.

Behavior visible in this implementation includes:

- `AddAsync` rejects a `null` plugin, assigns a new ID when `Plugin.Id` is empty, stores the same plugin instance, and initializes empty dependency and capability lists. Assigning an existing ID replaces that plugin and resets both associated lists.
- `UpdateAsync` rejects a `null` plugin, returns `false` for an unknown ID, sets `ModifiedAt` to `DateTime.UtcNow`, and replaces the stored instance for a known ID.
- `DeleteAsync` returns `false` for an unknown ID. A successful deletion also removes that ID's dependency and capability lists.
- `GetByNameAsync` returns `null` for a null, empty, or whitespace name; otherwise it selects the first case-insensitive exact name match.
- `GetByStatusAsync` uses equality on `Plugin.Status`. `SearchAsync` performs a case-insensitive substring search over `Name`, `Description`, and `Author`; a null, empty, or whitespace term delegates to `GetAllAsync`.
- `AddDependencyAsync` and `AddCapabilityAsync` reject null records, create a list for an unknown plugin ID, append without checking for a corresponding plugin or duplicate record, and return `true`.
- `RemoveDependencyAsync` removes the first record whose `PluginDependency.Id` matches and returns `false` when either the list or record is absent.
- Dependency and capability reads return empty sequences for unknown IDs and return copied lists for known IDs. Plugin reads return the stored plugin references; `GetAllAsync` returns an enumerable over the dictionary values.
- Although all methods accept a `CancellationToken`, this implementation does not inspect it.

Because the implementation is in-memory, its contents last only for the lifetime of that repository instance and are not persisted to external storage.
