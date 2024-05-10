# IDependencyResolutionService

Represents the state and result of a dependency resolution operation within the plugin engine. It holds the root plugin identifier, the complete graph of dependency nodes and edges discovered during resolution, and metadata describing each plugin and the constraints between them.

## API

### RootPluginId
`public Guid RootPluginId`

Gets the unique identifier of the plugin that initiated the dependency resolution. All nodes and edges in the graph are evaluated relative to this root.

### Nodes
`public List<DependencyNode> Nodes`

Gets the collection of all dependency nodes discovered during resolution. Each node represents a distinct plugin version that was considered or selected to satisfy a dependency chain originating from the root plugin.

### Edges
`public List<DependencyEdge> Edges`

Gets the collection of directed edges connecting dependency nodes. Each edge defines a relationship from one plugin to another, including the version constraint that governs the dependency and whether it is optional.

### PluginId
`public Guid PluginId`

Gets the unique identifier of a specific plugin within a dependency node. This value corresponds to the plugin’s stable identity across versions.

### PluginName
`public string PluginName`

Gets the human-readable name of the plugin as declared in its manifest. This is the display name used for logging, diagnostics, and user-facing output.

### Version
`public string Version`

Gets the version string of the plugin represented by this dependency node. The format follows the plugin engine’s versioning scheme and is used when evaluating version constraints.

### Level
`public int Level`

Gets the depth of this dependency node in the resolution graph. A value of zero typically indicates the root plugin, with each successive level representing one degree of transitive dependency separation.

### FromPluginId
`public Guid FromPluginId`

Gets the identifier of the plugin that declares the dependency. This is the source node of a directed edge in the dependency graph.

### ToPluginId
`public Guid ToPluginId`

Gets the identifier of the plugin that is required or optionally referenced. This is the target node of a directed edge in the dependency graph.

### VersionConstraint
`public string VersionConstraint`

Gets the version range or exact version specification that the dependency must satisfy. The format supports minimum, maximum, and wildcard constraints as defined by the plugin engine’s resolution rules.

### IsOptional
`public bool IsOptional`

Gets whether the dependency edge is optional. When `true`, resolution may succeed even if no compatible version of the target plugin is found. When `false`, failure to satisfy the constraint causes the overall resolution to fail.

## Usage

### Example 1: Inspecting a Resolved Dependency Graph

```csharp
IDependencyResolutionService resolution = engine.ResolveDependencies(rootPluginId);

Console.WriteLine($"Root plugin: {resolution.RootPluginId}");
Console.WriteLine($"Total nodes: {resolution.Nodes.Count}");
Console.WriteLine($"Total edges: {resolution.Edges.Count}");

foreach (var node in resolution.Nodes.OrderBy(n => n.Level))
{
    Console.WriteLine($"  [{node.Level}] {node.PluginName} v{node.Version} ({node.PluginId})");
}

foreach (var edge in resolution.Edges)
{
    string optionalTag = edge.IsOptional ? " (optional)" : "";
    Console.WriteLine($"  {edge.FromPluginId} -> {edge.ToPluginId} : {edge.VersionConstraint}{optionalTag}");
}
```

### Example 2: Detecting Missing Required Dependencies

```csharp
IDependencyResolutionService resolution = engine.ResolveDependencies(rootPluginId);

var resolvedPluginIds = new HashSet<Guid>(resolution.Nodes.Select(n => n.PluginId));
var failures = new List<DependencyEdge>();

foreach (var edge in resolution.Edges)
{
    if (!edge.IsOptional && !resolvedPluginIds.Contains(edge.ToPluginId))
    {
        failures.Add(edge);
    }
}

if (failures.Any())
{
    foreach (var failure in failures)
    {
        Console.WriteLine($"Unresolved required dependency: {failure.ToPluginId} " +
                          $"(constraint: {failure.VersionConstraint})");
    }
    throw new DependencyResolutionException("Required dependencies could not be satisfied.");
}
```

## Notes

The `Nodes` and `Edges` collections are populated during resolution and should be treated as read-only snapshots of the resolution state. Modifying these lists after resolution completes will not affect the plugin engine’s internal state and may lead to inconsistent representations.

The `Level` property on `DependencyNode` reflects the shortest-path depth from the root. In the presence of diamond dependencies, a plugin may appear at multiple depths; the resolution service records the minimum level at which it was first encountered.

When `IsOptional` is `true` on a `DependencyEdge` and no compatible version exists, the target plugin may be absent from the `Nodes` list entirely. Code that traverses edges should verify node existence before dereferencing.

The `VersionConstraint` string is stored in its raw form as declared by the dependent plugin. Consumers that need to evaluate compatibility programmatically should use the plugin engine’s version-matching utilities rather than parsing the constraint manually.

This type is not guaranteed to be thread-safe. Instances are typically created and consumed within the scope of a single resolution operation. Concurrent access to the same instance from multiple threads requires external synchronization.
