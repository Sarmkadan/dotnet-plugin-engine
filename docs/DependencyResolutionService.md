# DependencyResolutionService

The `DependencyResolutionService` provides asynchronous utilities for analyzing, validating, and resolving plugin dependencies within the dotnet-plugin-engine framework. It maintains an internal representation of plugin relationships and offers operations to query dependency graphs, detect circular references, and retrieve resolved plugin sets.

## API

### DependencyResolutionService()
**Purpose:** Initializes a new instance of the service with an empty internal state.  
**Parameters:** None.  
**Return Value:** N/A (constructor).  
**Exceptions:** None under normal conditions.

### Task<IEnumerable<Plugin>> ResolveDependenciesAsync()
**Purpose:** Asynchronously resolves the full set of dependencies for all plugins known to the service.  
**Parameters:** None.  
**Return Value:** A task that yields an enumerable of `Plugin` objects representing the resolved dependency set.  
**Exceptions:**  
- `InvalidOperationException` if the dependency graph cannot be resolved due to missing dependencies.  
- `OperationCanceledException` if a cancellation token (if supplied) is triggered.

### Task<bool> ValidateDependenciesAsync()
**Purpose:** Asynchronously checks whether all declared dependencies can be satisfied without conflicts.  
**Parameters:** None.  
**Return Value:** A task that yields `true` if the dependency set is valid; otherwise `false`.  
**Exceptions:**  
- `InvalidOperationException` if validation encounters an inconsistent internal state.  
- `OperationCanceledException` if a cancellation token (if supplied) is triggered.

### Task<bool> HasCircularDependenciesAsync()
**Purpose:** Asynchronously determines whether the current dependency graph contains any circular references.  
**Parameters:** None.  
**Return Value:** A task that yields `true` if a circular dependency exists; otherwise `false`.  
**Exceptions:**  
- `InvalidOperationException` if the graph cannot be traversed.  
- `OperationCanceledException` if a cancellation token (if supplied) is triggered.

### Task<DependencyGraph> GetDependencyGraphAsync()
**Purpose:** Asynchronously retrieves a snapshot of the internal dependency graph.  
**Parameters:** None.  
**Return Value:** A task that yields a `DependencyGraph` object describing nodes (plugins) and edges (dependencies).  
**Exceptions:**  
- `InvalidOperationException` if the graph cannot be constructed.  
- `OperationCanceledException` if a cancellation token (if supplied) is triggered.

### Task<Plugin?> ResolveSingleDependencyAsync()
**Purpose:** Asynchronously attempts to resolve a single, unspecified dependency (typically the next pending dependency in the resolution queue).  
**Parameters:** None.  
**Return Value:** A task that yields the resolved `Plugin` instance, or `null` if no dependency can be resolved.  
**Exceptions:**  
- `InvalidOperationException` if resolution logic encounters an invalid state.  
- `OperationCanceledException` if a cancellation token (if supplied) is triggered.

### Task<IEnumerable<Plugin>> GetDependentsAsync()
**Purpose:** Asynchronously returns the set of plugins that depend on any other plugin in the current graph.  
**Parameters:** None.  
**Return Value:** A task that yields an enumerable of `Plugin` objects representing dependents.  
**Exceptions:**  
- `InvalidOperationException` if the dependent set cannot be computed.  
- `OperationCanceledException` if a cancellation token (if supplied) is triggered.

### Task ClearDependencyCacheAsync()
**Purpose:** Asynchronously clears any cached dependency resolution results, forcing subsequent calls to recompute from scratch.  
**Parameters:** None.  
**Return Value:** A task that completes when the cache has been cleared.  
**Exceptions:**  
- `IOException` if the underlying cache storage fails to reset.  
- `OperationCanceledException` if a cancellation token (if supplied) is triggered.

## Usage

```csharp
using System.Threading.Tasks;
using DotNetPluginEngine;

// Assuming a host has already registered plugins with the service.
var resolver = new DependencyResolutionService();

// Resolve all dependencies and iterate over the result.
IEnumerable<Plugin> resolved = await resolver.ResolveDependenciesAsync();
foreach (var plugin in resolved)
{
    Console.WriteLine($"Resolved: {plugin.Id}");
}
```

```csharp
using System.Threading.Tasks;
using DotNetPluginEngine;

var resolver = new DependencyResolutionService();

// Detect circular dependencies before attempting resolution.
bool hasCircle = await resolver.HasCircularDependenciesAsync();
if (hasCircle)
{
    Console.WriteLine("Circular dependencies detected; aborting resolution.");
}
else
{
    bool valid = await resolver.ValidateDependenciesAsync();
    if (valid)
    {
        var graph = await resolver.GetDependencyGraphAsync();
        // Use graph for visualization or further processing.
    }
}
```

## Notes

- All methods operate on the service's internal state; they do not accept external plugin collections as arguments. Callers must ensure plugins are registered with the service prior to invoking resolution or validation routines.  
- The service is safe for concurrent read‑only operations (e.g., multiple calls to `GetDependencyGraphAsync` or `ValidateDependenciesAsync`). However, invoking `ClearDependencyCacheAsync` while other asynchronous operations are in progress may cause those operations to fail with `InvalidOperationException` as they rely on cached data that is being cleared.  
- Methods that return `Task<bool>` indicate success or failure of a check; they do not throw for the boolean outcome itself—exceptions are reserved for unexpected internal errors.  
- If a cancellation token is supplied via an overload not shown in the public signature, observing `OperationCanceledException` is the appropriate way to handle cooperative cancellation.  
- The `ResolveSingleDependencyAsync` method returns `null` when no further dependencies can be resolved; callers should treat this as a normal termination condition rather than an error.  
- The `DependencyGraph` type returned by `GetDependencyGraphAsync` is immutable for the duration of the returned instance; subsequent modifications to the service's internal state will not affect already‑returned graph objects.
