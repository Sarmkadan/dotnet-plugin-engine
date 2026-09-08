# DependencyResolutionServiceValidation

Provides validation helpers for `DependencyResolutionService` and related plugin dependency types.

## Public Members

| Member | Signature | Purpose |
|--------|-----------|---------|
| Validate | `public static IReadOnlyList<string> Validate(this DependencyResolutionService value)` | Validates a `DependencyResolutionService` instance. |
| IsValid | `public static bool IsValid(this DependencyResolutionService value)` | Determines whether a `DependencyResolutionService` instance is valid. |
| EnsureValid | `public static void EnsureValid(this DependencyResolutionService value)` | Ensures that a `DependencyResolutionService` instance is valid. |
| Validate (Plugin) | `public static IReadOnlyList<string> Validate(this Plugin plugin)` | Validates a `Plugin` instance for dependency resolution purposes. |
| Validate (PluginDependency) | `public static IReadOnlyList<string> Validate(this PluginDependency dependency)` | Validates a `PluginDependency` instance. |
| Validate (PluginMetadata) | `public static IReadOnlyList<string> Validate(this PluginMetadata metadata)` | Validates a `PluginMetadata` instance. |
| Validate (DependencyNode) | `public static IReadOnlyList<string> Validate(this DependencyNode node)` | Validates a `DependencyNode` instance. |
| Validate (DependencyEdge) | `public static IReadOnlyList<string> Validate(this DependencyEdge edge)` | Validates a `DependencyEdge` instance. |
| Validate (DependencyGraph) | `public static IReadOnlyList<string> Validate(this DependencyGraph graph)` | Validates a `DependencyGraph` instance. |

## Method Details

### Validate(DependencyResolutionService)

Validates a `DependencyResolutionService` instance.

```csharp
public static IReadOnlyList<string> Validate(this DependencyResolutionService value)
```

**Parameters**
- `value`: The service instance to validate.

**Returns**
- An enumerable of validation error messages, or empty if valid.

**Exceptions**
- `ArgumentNullException`: Thrown when `value` is null.

**Example Usage**
```csharp
var service = new DependencyResolutionService(/* dependencies */);
IReadOnlyList<string> errors = service.Validate();
if (errors.Count > 0)
{
    // Handle validation errors
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```

### IsValid(DependencyResolutionService)

Determines whether a `DependencyResolutionService` instance is valid.

```csharp
public static bool IsValid(this DependencyResolutionService value)
```

**Parameters**
- `value`: The service instance to check.

**Returns**
- `true` if the service is valid; otherwise, `false`.

**Exceptions**
- `ArgumentNullException`: Thrown when `value` is null.

**Example Usage**
```csharp
var service = new DependencyResolutionService(/* dependencies */);
if (service.IsValid())
{
    // Proceed with service usage
}
else
{
    // Handle invalid service
}
```

### EnsureValid(DependencyResolutionService)

Ensures that a `DependencyResolutionService` instance is valid.

```csharp
public static void EnsureValid(this DependencyResolutionService value)
```

**Parameters**
- `value`: The service instance to validate.

**Exceptions**
- `ArgumentNullException`: Thrown when `value` is null.
- `ArgumentException`: Thrown when the service is invalid, containing validation error messages.

**Example Usage**
```csharp
var service = new DependencyResolutionService(/* dependencies */);
try
{
    service.EnsureValid();
    // Service is valid, proceed
}
catch (ArgumentException ex)
{
    // Handle validation errors
    Console.WriteLine(ex.Message);
}
```

### Validate(Plugin)

Validates a `Plugin` instance for dependency resolution purposes.

```csharp
public static IReadOnlyList<string> Validate(this Plugin plugin)
```

**Parameters**
- `plugin`: The plugin to validate.

**Returns**
- An enumerable of validation error messages, or empty if valid.

**Exceptions**
- `ArgumentNullException`: Thrown when `plugin` is null.

**Example Usage**
```csharp
var plugin = new Plugin
{
    Id = Guid.NewGuid(),
    Name = "SamplePlugin",
    Version = "1.0.0",
    AssemblyPath = "/path/to/plugin.dll",
    Author = "Sample Author",
    CreatedAt = DateTime.UtcNow,
    ModifiedAt = DateTime.UtcNow
};
IReadOnlyList<string> errors = plugin.Validate();
if (errors.Count == 0)
{
    // Plugin is valid
}
```

### Validate(PluginDependency)

Validates a `PluginDependency` instance.

```csharp
public static IReadOnlyList<string> Validate(this PluginDependency dependency)
```

**Parameters**
- `dependency`: The dependency to validate.

**Returns**
- An enumerable of validation error messages, or empty if valid.

**Exceptions**
- `ArgumentNullException`: Thrown when `dependency` is null.

**Example Usage**
```csharp
var dependency = new PluginDependency
{
    PluginId = Guid.NewGuid(),
    DependencyPluginId = Guid.NewGuid(),
    MinimumVersion = "1.0.0",
    Type = DependencyType.Required,
    CreatedAt = DateTime.UtcNow
};
IReadOnlyList<string> errors = dependency.Validate();
if (errors.Count > 0)
{
    // Handle validation errors
}
```

### Validate(PluginMetadata)

Validates a `PluginMetadata` instance.

```csharp
public static IReadOnlyList<string> Validate(this PluginMetadata metadata)
```

**Parameters**
- `metadata`: The metadata to validate.

**Returns**
- An enumerable of validation error messages, or empty if valid.

**Exceptions**
- `ArgumentNullException`: Thrown when `metadata` is null.

**Example Usage**
```csharp
var metadata = new PluginMetadata
{
    PluginId = Guid.NewGuid(),
    PluginName = "SamplePlugin",
    PluginVersion = "1.0.0",
    AssemblyName = "SamplePlugin",
    TargetFramework = ".NETStandard2.0",
    AssemblyVersion = "1.0.0.0",
    Author = "Sample Author",
    CreatedAt = DateTime.UtcNow
};
IReadOnlyList<string> errors = metadata.Validate();
// errors will be empty if all fields are valid
```

### Validate(DependencyNode)

Validates a `DependencyNode` instance.

```csharp
public static IReadOnlyList<string> Validate(this DependencyNode node)
```

**Parameters**
- `node`: The node to validate.

**Returns**
- An enumerable of validation error messages, or empty if valid.

**Exceptions**
- `ArgumentNullException`: Thrown when `node` is null.

**Example Usage**
```csharp
var node = new DependencyNode
{
    PluginId = Guid.NewGuid(),
    PluginName = "SamplePlugin",
    Version = "1.0.0",
    Level = 0
};
IReadOnlyList<string> errors = node.Validate();
// errors will be empty if all fields are valid
```

### Validate(DependencyEdge)

Validates a `DependencyEdge` instance.

```csharp
public static IReadOnlyList<string> Validate(this DependencyEdge edge)
```

**Parameters**
- `edge`: The edge to validate.

**Returns**
- An enumerable of validation error messages, or empty if valid.

**Exceptions**
- `ArgumentNullException`: Thrown when `edge` is null.

**Example Usage**
```csharp
var edge = new DependencyEdge
{
    FromPluginId = Guid.NewGuid(),
    ToPluginId = Guid.NewGuid(),
    VersionConstraint = ">= 1.0.0"
};
IReadOnlyList<string> errors = edge.Validate();
// errors will be empty if the constraint is valid
```

### Validate(DependencyGraph)

Validates a `DependencyGraph` instance.

```csharp
public static IReadOnlyList<string> Validate(this DependencyGraph graph)
```

**Parameters**
- `graph`: The graph to validate.

**Returns**
- An enumerable of validation error messages, or empty if valid.

**Exceptions**
- `ArgumentNullException`: Thrown when `graph` is null.

**Example Usage**
```csharp
var graph = new DependencyGraph
{
    RootPluginId = Guid.NewGuid(),
    Nodes = new List<DependencyNode> { /* nodes */ },
    Edges = new List<DependencyEdge> { /* edges */ }
};
IReadOnlyList<string> errors = graph.Validate();
// errors will be empty if the graph structure is valid
```