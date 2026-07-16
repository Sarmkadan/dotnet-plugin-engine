// entire file content ...

// ...

## AssemblyLoadContextInfo

`AssemblyLoadContextInfo` represents information about an AssemblyLoadContext used for plugin isolation. It stores details such as the load context identifier, plugin ID, context name, creation and last activity timestamps, and memory usage statistics.

### Usage Example

```csharp
using PluginEngine.Domain.Entities;

// Create a new AssemblyLoadContextInfo instance
var loadContextInfo = new AssemblyLoadContextInfo
{
    ContextId = "my-load-context",
    PluginId = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
    Name = "My Load Context",
    CreatedAt = DateTime.UtcNow,
    LastActivityAt = DateTime.UtcNow,
    MemoryUsageBytes = 1024 * 1024 * 10, // 10 MB
    LoadedTypeCount = 100,
    IsActive = true
};

// Add an assembly to the loaded collection
loadContextInfo.AddLoadedAssembly("MyAssembly.dll");

// Check if an assembly is loaded
bool isLoaded = loadContextInfo.IsAssemblyLoaded("MyAssembly.dll");
Console.WriteLine($"Is assembly loaded? {isLoaded}");

// Get the total assembly count
int assemblyCount = loadContextInfo.GetAssemblyCount();
Console.WriteLine($"Assembly count: {assemblyCount}");

// Update the activity timestamp
loadContextInfo.UpdateActivity();

// Get the status summary
string statusSummary = loadContextInfo.GetStatusSummary();
Console.WriteLine($"Status summary: {statusSummary}");
```

// ... rest of content ...
