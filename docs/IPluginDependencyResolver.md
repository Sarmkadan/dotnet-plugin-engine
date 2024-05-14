# IPluginDependencyResolver

The `IPluginDependencyResolver` interface defines the contract for objects responsible for analyzing, tracking, and reporting the dependency graph state within the `dotnet-plugin-engine`. It serves as a data carrier that encapsulates the resolution context for a specific plugin, including its identity, version constraints, dependency requirements, detected conflicts, and the sequential steps taken during the resolution process. Implementations of this interface provide the engine with a snapshot of the dependency landscape at a specific point in time, enabling diagnostic tools and the core engine to make informed decisions about plugin loading order and compatibility.

## API

The following members constitute the public surface area of the `IPluginDependencyResolver` interface.

### `DependencyPluginId`
*   **Type:** `Guid`
*   **Purpose:** Gets the unique identifier of the plugin that is being depended upon in the current resolution context.
*   **Parameters:** None.
*   **Return Value:** The `Guid` representing the dependency plugin.
*   **Throws:** Never.

### `DependencyName`
*   **Type:** `string`
*   **Purpose:** Gets the human-readable name of the plugin that is being depended upon.
*   **Parameters:** None.
*   **Return Value:** The name of the dependency plugin.
*   **Throws:** Never.

### `ConflictingRequirements`
*   **Type:** `List<ConflictingRequirement>`
*   **Purpose:** Gets a collection of requirements that could not be satisfied due to version mismatches or circular dependencies.
*   **Parameters:** None.
*   **Return Value:** A list of `ConflictingRequirement` objects detailing specific unmet constraints.
*   **Throws:** Never. May return an empty list if no conflicts exist.

### `Description`
*   **Type:** `string`
*   **Purpose:** Gets a textual description of the current resolution state or the specific dependency relationship being analyzed.
*   **Parameters:** None.
*   **Return Value:** A descriptive string.
*   **Throws:** Never.

### `RequiringPluginId`
*   **Type:** `Guid`
*   **Purpose:** Gets the unique identifier of the plugin that requires the dependency defined in this context.
*   **Parameters:** None.
*   **Return Value:** The `Guid` of the requiring plugin.
*   **Throws:** Never.

### `RequiringPluginName`
*   **Type:** `string`
*   **Purpose:** Gets the human-readable name of the plugin that requires the dependency.
*   **Parameters:** None.
*   **Return Value:** The name of the requiring plugin.
*   **Throws:** Never.

### `VersionConstraint`
*   **Type:** `string`
*   **Purpose:** Gets the version constraint string (e.g., ">=1.0.0", "2.0.*") imposed by the requiring plugin on the dependency.
*   **Parameters:** None.
*   **Return Value:** The version constraint expression.
*   **Throws:** Never.

### `RootPluginId`
*   **Type:** `Guid`
*   **Purpose:** Gets the unique identifier of the root plugin from which the current dependency resolution chain originated.
*   **Parameters:** None.
*   **Return Value:** The `Guid` of the root plugin.
*   **Throws:** Never.

### `Steps`
*   **Type:** `List<ResolutionStep>`
*   **Purpose:** Gets the ordered sequence of logical steps executed by the resolver to reach the current state.
*   **Parameters:** None.
*   **Return Value:** A list of `ResolutionStep` objects representing the resolution history.
*   **Throws:** Never.

### `Conflicts`
*   **Type:** `List<DependencyConflict>`
*   **Purpose:** Gets a collection of high-level conflict objects summarizing incompatibilities detected between multiple plugins.
*   **Parameters:** None.
*   **Return Value:** A list of `DependencyConflict` objects.
*   **Throws:** Never.

### `GeneratedAtUtc`
*   **Type:** `DateTime`
*   **Purpose:** Gets the Coordinated Universal Time (UTC) timestamp indicating when this resolution snapshot was generated.
*   **Parameters:** None.
*   **Return Value:** A `DateTime` value in UTC.
*   **Throws:** Never.

### `Order`
*   **Type:** `int`
*   **Purpose:** Gets the calculated load order priority for the plugin within the resolved graph. Lower values typically indicate earlier loading.
*   **Parameters:** None.
*   **Return Value:** An integer representing the load order.
*   **Throws:** Never.

### `PluginId`
*   **Type:** `Guid`
*   **Purpose:** Gets the unique identifier of the primary plugin associated with this resolver instance.
*   **Parameters:** None.
*   **Return Value:** The `Guid` of the plugin.
*   **Throws:** Never.

### `PluginName`
*   **Type:** `string`
*   **Purpose:** Gets the human-readable name of the primary plugin associated with this resolver instance.
*   **Parameters:** None.
*   **Return Value:** The name of the plugin.
*   **Throws:** Never.

### `Version`
*   **Type:** `string`
*   **Purpose:** Gets the specific version of the primary plugin being resolved.
*   **Parameters:** None.
*   **Return Value:** The version string.
*   **Throws:** Never.

### `Action`
*   **Type:** `ResolutionAction`
*   **Purpose:** Gets the enumerated action determined by the resolver (e.g., Load, Skip, Fail) for the current plugin.
*   **Parameters:** None.
*   **Return Value:** A `ResolutionAction` enum value.
*   **Throws:** Never.

### `IsOptional`
*   **Type:** `bool`
*   **Purpose:** Gets a value indicating whether the dependency or the plugin itself is marked as optional, allowing the system to proceed even if resolution fails.
*   **Parameters:** None.
*   **Return Value:** `true` if optional; otherwise, `false`.
*   **Throws:** Never.

## Usage

### Example 1: Inspecting Resolution Conflicts
This example demonstrates how to iterate through a resolved dependency graph to identify and log any conflicts that prevent a plugin from loading correctly.

```csharp
public void LogResolutionIssues(IPluginDependencyResolver resolver)
{
    if (resolver.Conflicts.Count > 0 || resolver.ConflictingRequirements.Count > 0)
    {
        Console.WriteLine($"[ERROR] Resolution failed for plugin: {resolver.PluginName} (ID: {resolver.PluginId})");
        Console.WriteLine($"Generated at: {resolver.GeneratedAtUtc}");
        Console.WriteLine($"Proposed Action: {resolver.Action}");

        foreach (var conflict in resolver.Conflicts)
        {
            Console.WriteLine($"  - Conflict: {conflict.Message}");
        }

        foreach (var req in resolver.ConflictingRequirements)
        {
            Console.WriteLine($"  - Unmet Requirement: {req.Constraint} for {req.TargetName}");
        }
    }
    else
    {
        Console.WriteLine($"[OK] Plugin {resolver.PluginName} resolved successfully with order {resolver.Order}.");
    }
}
```

### Example 2: Validating Optional Dependencies
This example shows how to check the status of an optional dependency. If the dependency is missing but marked as optional, the system proceeds; otherwise, it halts the load process.

```csharp
public bool ValidateDependencyLoad(IPluginDependencyResolver resolver)
{
    // Check if the specific dependency relationship has issues
    if (resolver.ConflictingRequirements.Any())
    {
        if (resolver.IsOptional)
        {
            // Log warning but allow continuation
            Console.WriteLine($"Warning: Optional dependency '{resolver.DependencyName}' could not be resolved. Continuing without it.");
            return true; 
        }
        else
        {
            // Critical failure
            Console.WriteLine($"Critical: Required dependency '{resolver.DependencyName}' failed resolution.");
            return false;
        }
    }

    // Verify the version constraint is satisfied by checking the steps
    var successStep = resolver.Steps.FirstOrDefault(s => s.Status == ResolutionStatus.Success);
    if (successStep != null)
    {
        Console.WriteLine($"Dependency '{resolver.DependencyName}' version {resolver.Version} satisfies constraint {resolver.VersionConstraint}.");
        return true;
    }

    return false;
}
```

## Notes

*   **Immutability and Thread Safety:** The members of `IPluginDependencyResolver` appear to be data properties representing a snapshot in time (`GeneratedAtUtc`). While the interface itself does not enforce immutability, implementations should treat these collections (`ConflictingRequirements`, `Steps`, `Conflicts`) as read-only after the resolution process completes. If multiple threads access the same resolver instance, external synchronization is required if the underlying lists are mutable.
*   **Empty Collections:** Consumers should not assume that list properties (`Steps`, `Conflicts`, `ConflictingRequirements`) are non-null. While standard practice suggests returning empty lists rather than null, defensive coding should verify nullability before enumeration if the specific implementation contract is unknown.
*   **Temporal Consistency:** The `GeneratedAtUtc` property should be used to validate the freshness of the resolution data. In long-running processes where plugins may be updated dynamically, a resolver instance created at a previous time may not reflect the current state of the plugin directory.
*   **Root Context:** The `RootPluginId` is critical for debugging transitive dependency issues. When a conflict arises deep in the graph, tracing back to the `RootPluginId` identifies the entry point that triggered the problematic resolution chain.
*   **Action Semantics:** The `Action` property dictates the engine's next move. If `Action` is set to `Fail` and `IsOptional` is `false`, the engine must abort the loading sequence for the `PluginId`. If `IsOptional` is `true`, the engine may downgrade the `Action` to `Skip` internally despite the reported failure.
