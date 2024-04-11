# Plugin Dependency Resolution Guide

This guide covers how the dependency resolution system works, how to declare plugin
dependencies, and how to diagnose resolution problems.

## Overview

The engine resolves plugin dependencies at load time using `IDependencyResolutionService`.
Dependency data lives on the `Plugin` entity's `Dependencies` collection, where each entry
is a `PluginDependency` that encodes version bounds and optionality. Resolution is recursive:
loading plugin A may trigger resolution of B and C, and B may itself require D.

```
PluginA  ──requires──►  PluginB  ──requires──►  PluginD
                   └──requires──►  PluginC
```

## Declaring Dependencies

Add a `PluginDependency` to your plugin's dependency list before or during loading:

```csharp
var dependency = new PluginDependency
{
    PluginId           = myPlugin.Id,
    DependencyPluginId = utilityPlugin.Id,    // must already be loaded
    MinimumVersion     = "2.0.0",
    MaximumVersion     = "3.0.0",             // leave empty for no upper bound
    IsOptional         = false,
    Type               = DependencyType.Runtime
};

myPlugin.AddDependency(dependency);
```

### Version Constraints

| Example | Meaning |
|---------|---------|
| `MinimumVersion = "2.0.0"` (no max) | `>= 2.0.0` |
| `MinimumVersion = "2.0.0"`, `MaximumVersion = "3.0.0"` | `>= 2.0.0 && <= 3.0.0` |

`IsSatisfiedBy(string version)` returns `true` when the provided version string falls within the
declared bounds. Invalid version strings always return `false`.

## Resolving Dependencies at Runtime

```csharp
var resolver = serviceProvider.GetRequiredService<IDependencyResolutionService>();

// Resolve the full transitive closure (cached after first call)
IEnumerable<Plugin> deps = await resolver.ResolveDependenciesAsync(plugin);

foreach (var dep in deps)
    Console.WriteLine($"  {dep.Name} {dep.Version}");

// Validate without full resolution (fast check)
bool allSatisfied = await resolver.ValidateDependenciesAsync(plugin);
```

## Detecting Circular Dependencies

```csharp
bool hasCycle = await resolver.HasCircularDependenciesAsync(plugin);
if (hasCycle)
    throw new DependencyResolutionException($"Circular dependency detected in {plugin.Name}");
```

Enable automatic circular-dependency detection during load:

```csharp
services.AddPluginEngine(options =>
{
    options.EnableCircularDependencyDetection = true;
});
```

## Visualising the Dependency Graph

```csharp
DependencyGraph graph = await resolver.GetDependencyGraphAsync(plugin.Id);

foreach (var node in graph.Nodes)
    Console.WriteLine($"[{node.Level}] {node.PluginName} {node.Version}");

foreach (var edge in graph.Edges)
    Console.WriteLine($"  {edge.FromPluginId} → {edge.ToPluginId}  {edge.VersionConstraint}{(edge.IsOptional ? " (optional)" : "")}");
```

## Optional Dependencies

Mark a dependency optional so the engine skips it when the target plugin is not loaded:

```csharp
var dep = new PluginDependency
{
    DependencyPluginId = analyticsPlugin.Id,
    MinimumVersion     = "1.0.0",
    IsOptional         = true          // load succeeds even if missing
};
```

`ValidateDependenciesAsync` returns `true` even when optional dependencies are absent.

## Getting Reverse Dependents

Find every loaded plugin that depends on a given plugin before unloading it:

```csharp
var dependents = await resolver.GetDependentsAsync(targetPlugin.Id);
if (dependents.Any())
{
    Console.WriteLine("Cannot unload: the following plugins depend on it:");
    foreach (var d in dependents)
        Console.WriteLine($"  {d.Name}");
}
```

## Caching

Resolution results are cached in-process for the lifetime of the service instance. Call
`ClearDependencyCacheAsync()` after hot-reloading a plugin to force fresh resolution:

```csharp
await resolver.ClearDependencyCacheAsync();
```

## Configuration Reference

```csharp
services.AddPluginEngine(options =>
{
    options.EnableDependencyCaching          = true;
    options.DependencyCacheTTLMs            = 300_000;  // 5 minutes
    options.EnableCircularDependencyDetection = true;
    options.MaxDependencyResolutionAttempts  = 10;
    options.StrictVersionChecking           = true;
});
```

| Option | Default | Description |
|--------|---------|-------------|
| `EnableDependencyCaching` | `true` | Cache resolution results in memory |
| `DependencyCacheTTLMs` | `300000` | Cache entry lifetime in milliseconds |
| `EnableCircularDependencyDetection` | `true` | Abort load on detected cycle |
| `MaxDependencyResolutionAttempts` | `10` | Retry limit per dependency |
| `StrictVersionChecking` | `true` | Reject non-SemVer version strings |

## Troubleshooting

### `DependencyResolutionException` on load

Enable debug logging and check which dependency ID cannot be resolved:

```csharp
options.EnableLogging      = true;
options.MinimumLogLevel    = LogLevel.Debug;
```

Verify the required plugin was loaded before the dependent plugin:

```csharp
bool isLoaded = await loader.IsPluginLoadedAsync(requiredPluginId);
```

### Version mismatch

Inspect the constraint and the installed version:

```csharp
var dep    = plugin.Dependencies.First(d => d.DependencyPluginId == targetId);
var target = await loader.GetLoadedPluginAsync(targetId);

Console.WriteLine($"Required : {dep.GetVersionConstraint()}");
Console.WriteLine($"Installed: {target?.Version}");
Console.WriteLine($"Satisfied: {dep.IsSatisfiedBy(target?.Version ?? "")}");
```

### Circular dependency detected

Use `GetDependencyGraphAsync` to print the full graph and identify the cycle visually. Then
restructure the plugins to break the cycle, or mark one edge as `IsOptional = true` if the
direction of the dependency can be inverted.

## See Also

- [Architecture Guide](./architecture.md)
- [API Reference](./api-reference.md)
- [Getting Started](./getting-started.md)
