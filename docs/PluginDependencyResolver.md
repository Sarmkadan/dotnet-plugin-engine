# PluginDependencyResolver

Resolves dependency relationships among plugins by computing valid installation orders, detecting conflicts, and constructing resolution plans. It operates on a set of plugins and their declared dependencies, producing results that respect version constraints and compatibility rules.

## API

### `PluginDependencyResolver`

Initializes a new instance of the resolver. The constructor accepts no arguments and prepares the internal state for subsequent dependency analysis operations.

### `GetInstallOrderAsync`

```csharp
public Task<PluginOperationResult<List<Plugin>>> GetInstallOrderAsync(
    IReadOnlyCollection<Plugin> available,
    IReadOnlyCollection<Plugin> requested)
```

Determines a valid linear installation order for the `requested` plugins, drawn from the `available` pool, such that every plugin appears after all of its dependencies. The returned list is topologically sorted; if multiple valid orders exist, the implementation selects one deterministically.

**Parameters:**
- `available` — the full set of plugins that may be used to satisfy dependencies.
- `requested` — the subset of plugins the caller intends to install.

**Return value:**  
A `PluginOperationResult<List<Plugin>>` whose `Success` property indicates whether an order was found. On success, `Data` contains the ordered list. On failure, `ErrorMessage` describes the reason (e.g., missing dependency, cycle).

**Throws:**  
`ArgumentNullException` when either argument is `null`.

### `FindConflictsAsync`

```csharp
public Task<PluginOperationResult<List<DependencyConflict>>> FindConflictsAsync(
    IReadOnlyCollection<Plugin> installed,
    IReadOnlyCollection<Plugin> proposed)
```

Examines the `proposed` plugins against the already `installed` set and returns every detected conflict. A conflict arises when two plugins require incompatible versions of the same dependency, or when a proposed plugin’s requirements cannot be satisfied by the installed set.

**Parameters:**
- `installed` — plugins already present in the environment.
- `proposed` — plugins being considered for addition.

**Return value:**  
A `PluginOperationResult<List<DependencyConflict>>`. On success, `Data` is the list of conflicts (empty if none exist). On failure, `ErrorMessage` contains a processing error description.

**Throws:**  
`ArgumentNullException` when either argument is `null`.

### `BuildResolutionPlanAsync`

```csharp
public async Task<PluginOperationResult<DependencyResolutionPlan>> BuildResolutionPlanAsync(
    IReadOnlyCollection<Plugin> installed,
    IReadOnlyCollection<Plugin> requested,
    ResolutionStrategy strategy = ResolutionStrategy.MinimalChanges)
```

Constructs a complete resolution plan that transitions from the `installed` state to a state where all `requested` plugins are satisfied. The plan specifies which plugins to install, upgrade, downgrade, or remove, respecting the chosen `strategy`.

**Parameters:**
- `installed` — current set of installed plugins.
- `requested` — desired set of plugins.
- `strategy` — the resolution strategy to apply (default: `MinimalChanges`).

**Return value:**  
A `PluginOperationResult<DependencyResolutionPlan>` whose `Data` property holds the plan when successful. The plan enumerates discrete operations required to reach the target state.

**Throws:**  
`ArgumentNullException` when `installed` or `requested` is `null`.

## Usage

### Example 1: Compute a fresh installation order

```csharp
var resolver = new PluginDependencyResolver();

var available = new List<Plugin>
{
    new("Core", "1.0.0"),
    new("Logging", "2.0.0", dependencies: new[] { "Core >= 1.0.0" }),
    new("Security", "1.5.0", dependencies: new[] { "Core >= 1.0.0", "Logging >= 2.0.0" })
};

var requested = new List<Plugin> { available[2] }; // Security

var result = await resolver.GetInstallOrderAsync(available, requested);

if (result.Success)
{
    foreach (var plugin in result.Data)
    {
        Console.WriteLine($"Install: {plugin.Name} {plugin.Version}");
    }
    // Output:
    // Install: Core 1.0.0
    // Install: Logging 2.0.0
    // Install: Security 1.5.0
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

### Example 2: Detect conflicts before adding a plugin

```csharp
var resolver = new PluginDependencyResolver();

var installed = new List<Plugin>
{
    new("Core", "1.0.0"),
    new("Logging", "2.0.0", dependencies: new[] { "Core >= 1.0.0" })
};

var proposed = new List<Plugin>
{
    new("Analytics", "3.0.0", dependencies: new[] { "Logging == 1.5.0" })
};

var conflictsResult = await resolver.FindConflictsAsync(installed, proposed);

if (conflictsResult.Success)
{
    foreach (var conflict in conflictsResult.Data)
    {
        Console.WriteLine($"Conflict: {conflict.Description}");
        // Output: Conflict: Analytics 3.0.0 requires Logging == 1.5.0,
        //          but Logging 2.0.0 is installed.
    }
}
```

## Notes

- All public methods are asynchronous and return `PluginOperationResult<T>`, a discriminated union that must be inspected for `Success` before accessing `Data`.
- `GetInstallOrderAsync` treats the dependency graph as directed; cycles cause immediate failure with an error message.
- `FindConflictsAsync` performs version-range intersection checks. A conflict is recorded even when a compatible version exists in `available` but is not present in `installed` — the method evaluates only the two provided sets.
- `BuildResolutionPlanAsync` may produce plans that include downgrades if the `ResolutionStrategy` permits it. The default `MinimalChanges` strategy avoids unnecessary alterations.
- The resolver does not maintain mutable internal state across calls; each method invocation operates independently on its arguments. Instances are safe to reuse and safe for concurrent access from multiple threads without external synchronization.
- All methods throw `ArgumentNullException` synchronously for null arguments; other errors are surfaced through the returned `PluginOperationResult` rather than via exceptions.
