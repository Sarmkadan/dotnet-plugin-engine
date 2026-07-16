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
