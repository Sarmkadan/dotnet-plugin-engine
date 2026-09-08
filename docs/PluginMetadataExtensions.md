# PluginMetadataExtensions

Provides extension methods for `PluginMetadata`.

## Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| HasCustomProperty | `public static bool HasCustomProperty(this PluginMetadata metadata, string key)` | Determines whether the plugin metadata has a custom property with the specified key. |
| GetAllCustomProperties | `public static IReadOnlyDictionary<string, string> GetAllCustomProperties(this PluginMetadata metadata)` | Gets all custom properties of the plugin metadata. |
| ClearCustomProperties | `public static void ClearCustomProperties(this PluginMetadata metadata)` | Clears all custom properties of the plugin metadata. |

## Examples

### Checking for a custom property
```csharp
var metadata = new PluginMetadata();
// Assume metadata has been populated with custom properties
if (metadata.HasCustomProperty("version"))
{
    // Do something if the version custom property exists
}
```

### Retrieving all custom properties
```csharp
var metadata = new PluginMetadata();
// Assume metadata has been populated with custom properties
var customProperties = metadata.GetAllCustomProperties();
foreach (var kvp in customProperties)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

### Clearing all custom properties
```csharp
var metadata = new PluginMetadata();
// Assume metadata has been populated with custom properties
metadata.ClearCustomProperties();
// After this call, GetAllCustomProperties() will return an empty dictionary
```