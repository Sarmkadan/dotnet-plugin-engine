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

## VersionInfo

`VersionInfo` represents versioning information for plugins and assemblies. It stores details such as the version string, release date, compatibility information, pre-release status, and download metrics. The class provides methods for version manipulation, compatibility checking, and formatted display strings.

### Usage Example

```csharp
using PluginEngine.Domain.Entities;
using System;

// Create a new VersionInfo instance for a plugin
var versionInfo = new VersionInfo
{
    EntityId = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
    Version = "2.1.0",
    ReleaseDate = DateTime.UtcNow,
    ReleaseNotes = "Added support for .NET 8 and new plugin API features",
    IsPrerelease = false,
    PrereleaseIdentifier = "beta",
    BuildMetadata = "20260716",
    Compatibility = ".NET 6.0, .NET 7.0, .NET 8.0",
    IsActive = true,
    DeprecationNotice = "",
    DownloadCount = 1562
};

// Validate the version info
bool isValid = versionInfo.IsValid();
Console.WriteLine($"Is version valid? {isValid}");

// Get the semantic version string
string semanticVersion = versionInfo.GetSemanticVersion();
Console.WriteLine($"Semantic version: {semanticVersion}");

// Get the display string
string displayString = versionInfo.GetDisplayString();
Console.WriteLine($"Display string: {displayString}");

// Check compatibility with another version
bool isCompatible = versionInfo.IsCompatibleWith("2.0.0");
Console.WriteLine($"Compatible with 2.0.0? {isCompatible}");

// Increment version numbers
versionInfo.IncrementPatch();
Console.WriteLine($"After patch increment: {versionInfo.Version}");

versionInfo.IncrementMinor();
Console.WriteLine($"After minor increment: {versionInfo.Version}");

versionInfo.IncrementMajor();
Console.WriteLine($"After major increment: {versionInfo.Version}");

// Update download count
versionInfo.DownloadCount++;
```

// ... rest of content ...
