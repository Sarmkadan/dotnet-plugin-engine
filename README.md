// entire file content ...

// ...

## PluginAssembly

`PluginAssembly` represents an assembly that belongs to a plugin. It stores details such as the assembly's unique identifier, plugin ID, assembly name, version, file path, and load status.

### Usage Example

```csharp
using PluginEngine.Domain.Entities;

// Create a new PluginAssembly instance
var assembly = new PluginAssembly
{
    PluginId = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
    AssemblyName = "MyAssembly",
    AssemblyVersion = "1.0.0",
    FilePath = "/path/to/MyAssembly.dll",
    FileSizeBytes = 1024 * 1024 * 10, // 10 MB
    FileHash = "some-file-hash",
    PublicKeyToken = "some-public-key-token",
    IsMainAssembly = true,
    LoadContextId = "my-load-context",
    LastModifiedAt = DateTime.UtcNow,
    LoadedAt = DateTime.UtcNow,
    Status = AssemblyLoadStatus.Loaded
};

// Validate the assembly
bool isValid = assembly.IsValid();
Console.WriteLine($"Is assembly valid? {isValid}");

// Get the assembly qualified name
string qualifiedName = assembly.GetQualifiedName();
Console.WriteLine($"Assembly qualified name: {qualifiedName}");

// Update the assembly file information
assembly.UpdateFileInfo("/path/to/new-assembly.dll", 1024 * 1024 * 20, "new-file-hash");

// Mark the assembly as loaded
assembly.MarkAsLoaded("my-load-context");

// Mark the assembly as failed load
assembly.MarkAsFailedLoad("some-error-message");
```

// ... rest of content ...
