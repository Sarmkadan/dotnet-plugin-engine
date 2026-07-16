// entire file content ...

// ...

## PluginCapability

The `PluginCapability` class represents a capability or feature provided by a plugin. It encapsulates metadata about the capability, such as its name, version, and interface type.

### Usage Example

```csharp
using PluginEngine.Domain.Entities;

// Create a new plugin capability
var capability = new PluginCapability
{
    PluginId = Guid.NewGuid(),
    Name = "structured-logging",
    Version = "1.0.0",
    InterfaceTypeName = "ILogger",
    ImplementationTypeName = "LoggerImplementation",
    Description = "Provides structured logging capabilities",
    Tags = "logging, structured-logging",
    IsMandatory = true
};

// Get tags as an enumerable
var tags = capability.GetTags();
Console.WriteLine($"Tags: {string.Join(", ", tags)}");

// Check if the capability has a specific tag
var hasLoggingTag = capability.HasTag("logging");
Console.WriteLine($"Has logging tag: {hasLoggingTag}");

// Validate the capability
var isValid = capability.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Get a formatted display name
var displayName = capability.GetDisplayName();
Console.WriteLine($"Display name: {displayName}");
```

## PluginDependency

`PluginDependency` models a relationship where one plugin depends on another. It stores the IDs of both plugins, version constraints, optionality, and additional metadata, and provides helper methods to validate and evaluate those constraints.

### Usage Example

```csharp
using PluginEngine.Domain.Entities;
using System;

// Define a dependency between two plugins
var dependency = new PluginDependency
{
    PluginId = Guid.NewGuid(),                     // The plugin that has the dependency
    DependencyPluginId = Guid.Parse("d2c5e8a1-3b4f-4a6e-9f2b-1c2d3e4f5a6b"),
    MinimumVersion = "1.2.0",
    MaximumVersion = "2.0.0",
    IsOptional = false,
    Type = DependencyType.Runtime,
    Description = "Requires a logging plugin compatible with version 1.2–2.0"
};

// Display the version constraint string
Console.WriteLine($"Version constraint: {dependency.GetVersionConstraint()}");

// Check if a specific version satisfies the constraint
bool satisfies = dependency.IsSatisfiedBy("1.5.0");
Console.WriteLine($"Is version 1.5.0 satisfied? {satisfies}");

// Validate the dependency record itself
Console.WriteLine($"Dependency record valid? {dependency.IsValid()}");

// Show creation timestamp
Console.WriteLine($"Created at: {dependency.CreatedAt:u}");
```
