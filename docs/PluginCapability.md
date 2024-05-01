# PluginCapability

Represents a declared capability of a plugin within the dotnet-plugin-engine framework. A `PluginCapability` describes a specific interface that a plugin implements, along with metadata such as version, tags, and validity constraints. It serves as the contract point between the host application and the plugin, enabling discovery, filtering, and validation of plugin functionality.

## API

### `Guid Id`
Gets the unique identifier of this capability instance.

### `Guid PluginId`
Gets the unique identifier of the plugin that owns this capability.

### `string Name`
Gets the short name of the capability. Typically used for display and lookup purposes.

### `string Version`
Gets the version string associated with this capability. The format is not enforced by the type itself; consumers should parse or compare it according to their own versioning policy.

### `string InterfaceTypeName`
Gets the assembly-qualified or full name of the interface type that this capability claims to implement.

### `string ImplementationTypeName`
Gets the assembly-qualified or full name of the concrete type that implements the interface for this capability.

### `string Description`
Gets a human-readable description of the capability. May be empty or null if no description was provided.

### `string Tags`
Gets the raw, serialized tags string. For structured access, use `GetTags()`.

### `bool IsMandatory`
Gets whether this capability is required for the plugin to be considered functional. The host may refuse to load a plugin if a mandatory capability is invalid or missing.

### `DateTime CreatedAt`
Gets the UTC timestamp when this capability record was first created.

### `DateTime ModifiedAt`
Gets the UTC timestamp when this capability record was last modified.

### `IEnumerable<string> GetTags()`
Returns the tags associated with this capability as a sequence of individual strings. The method splits the underlying `Tags` string using the storage-defined delimiter. Returns an empty enumeration if no tags are set.

### `void SetTags(IEnumerable<string> tags)`
Replaces the current tags with the provided collection. The method joins the supplied strings into the `Tags` property using the storage-defined delimiter. Passing `null` results in an empty tag set.

**Parameters:**
- `tags`: The collection of tag strings to store. Individual tags should not contain the delimiter character.

### `bool HasTag(string tag)`
Determines whether the specified tag is present in this capability's tag set. The comparison is case-sensitive unless documented otherwise by the specific engine configuration.

**Parameters:**
- `tag`: The tag string to search for.

**Returns:** `true` if the tag exists; otherwise `false`.

### `bool IsValid`
Gets a value indicating whether this capability is currently considered valid. The validity check typically verifies that the interface and implementation types can be resolved and that the implementation correctly implements the interface. The exact validation logic is determined by the engine configuration.

### `string GetDisplayName()`
Returns a formatted display name for this capability, typically combining the `Name` and `Version` in a human-readable form. The exact format is implementation-defined but is intended for UI and logging purposes.

**Returns:** A string suitable for display to users.

## Usage

### Example 1: Registering and Validating a Capability

```csharp
// Assume engine and plugin context are already initialized
var capability = new PluginCapability
{
    Id = Guid.NewGuid(),
    PluginId = plugin.Id,
    Name = "DataExport",
    Version = "2.1.0",
    InterfaceTypeName = "MyApp.Contracts.IDataExporter, MyApp.Contracts",
    ImplementationTypeName = "MyPlugin.CsvExporter, MyPlugin",
    Description = "Exports data to CSV format",
    IsMandatory = true,
    CreatedAt = DateTime.UtcNow,
    ModifiedAt = DateTime.UtcNow
};

capability.SetTags(new[] { "export", "csv", "file-io" });

if (capability.IsValid)
{
    Console.WriteLine($"Capability '{capability.GetDisplayName()}' is ready.");
}
else
{
    Console.WriteLine($"Capability '{capability.Name}' failed validation.");
}
```

### Example 2: Filtering Plugins by Tags

```csharp
IEnumerable<PluginCapability> capabilities = pluginManager.GetAllCapabilities();

// Find all non-mandatory capabilities that support "logging"
var optionalLoggers = capabilities
    .Where(c => !c.IsMandatory && c.HasTag("logging"))
    .ToList();

foreach (var cap in optionalLoggers)
{
    Console.WriteLine($"Optional logger found: {cap.GetDisplayName()} [{cap.InterfaceTypeName}]");
    foreach (var tag in cap.GetTags())
    {
        Console.WriteLine($"  Tag: {tag}");
    }
}
```

## Notes

- **Tag delimiter sensitivity:** The `Tags` property stores tags using an internal delimiter. When calling `SetTags`, ensure individual tag strings do not contain that delimiter, as doing so will corrupt the tag set and cause `GetTags` and `HasTag` to behave unpredictably.
- **Case sensitivity:** `HasTag` performs case-sensitive comparison by default. Verify the engine configuration if case-insensitive matching is required.
- **Validity caching:** The `IsValid` property may reflect a cached state. If the underlying type resolution environment changes (e.g., assemblies are loaded or unloaded), the value may become stale. Consult the engine documentation for refresh mechanisms.
- **Thread safety:** This type is not inherently thread-safe. Concurrent reads and writes to `Tags` via `SetTags` and `GetTags` from multiple threads may lead to race conditions. External synchronization is required if instances are shared across threads.
- **Empty and null handling:** `GetTags` never returns `null`; it returns an empty enumeration when no tags are present. `SetTags` with a `null` argument stores an empty tag set. `HasTag` returns `false` for `null` or empty input.
- **Timestamps:** `CreatedAt` and `ModifiedAt` are set externally and are not automatically updated by the type itself. Consumers are responsible for setting `ModifiedAt` when mutating the capability.
