# PluginMetadata
The `PluginMetadata` type in the `dotnet-plugin-engine` project is designed to hold information about a plugin, including its identity, versioning, authorship, and other relevant details. This metadata is crucial for managing plugins within the engine, ensuring compatibility, and providing information to users. It serves as a central location for accessing and manipulating plugin-specific data.

## API
The `PluginMetadata` type exposes several public members:
- `Id`: A unique identifier for the metadata instance, represented as a `Guid`.
- `PluginId`: The identifier of the plugin, represented as a `Guid`.
- `PluginName`: The name of the plugin, represented as a `string`.
- `PluginVersion`: The version of the plugin, represented as a `string`.
- `AssemblyName`: The name of the assembly containing the plugin, represented as a `string`.
- `AssemblyVersion`: The version of the assembly containing the plugin, represented as a `string`.
- `TargetFramework`: The target framework of the plugin, represented as a `string`.
- `Author`: The author of the plugin, represented as a `string`.
- `Company`: The company associated with the plugin, represented as a `string`.
- `Description`: A brief description of the plugin, represented as a `string`.
- `License`: The license under which the plugin is distributed, represented as a `string`.
- `RepositoryUrl`: The URL of the plugin's repository, represented as a `string`.
- `MinimumClrVersion`: The minimum required CLR version for the plugin, represented as a `string`.
- `EngineVersionConstraint`: The version constraint for the engine, represented as a `string`.
- `IsSigned`: A boolean indicating whether the plugin is signed.
- `PublicKeyToken`: The public key token of the plugin, represented as a `string`.
- `CreatedAt`: The date and time when the metadata was created, represented as a `DateTime`.
- `SetCustomProperty`: A method to set a custom property. It does not return a value.
- `GetCustomProperty`: A method to retrieve a custom property, returning its value as a `string?`.
- `RemoveCustomProperty`: A method to remove a custom property, returning a `bool` indicating success.

## Usage
Here are two examples of using the `PluginMetadata` type in C#:
```csharp
// Example 1: Creating and accessing plugin metadata
var metadata = new PluginMetadata
{
    Id = Guid.NewGuid(),
    PluginId = Guid.NewGuid(),
    PluginName = "MyPlugin",
    PluginVersion = "1.0.0",
    Author = "John Doe",
    Description = "A sample plugin."
};

Console.WriteLine($"Plugin Name: {metadata.PluginName}, Version: {metadata.PluginVersion}");

// Example 2: Setting and retrieving custom properties
metadata.SetCustomProperty("CustomKey", "CustomValue");
var customValue = metadata.GetCustomProperty("CustomKey");

Console.WriteLine($"Custom Value: {customValue}");
```

## Notes
When working with `PluginMetadata`, consider the following:
- The `Id` and `PluginId` properties are unique identifiers and should be generated carefully to avoid collisions.
- The `MinimumClrVersion` and `EngineVersionConstraint` properties are crucial for ensuring compatibility and should be set based on the plugin's requirements.
- Custom properties set via `SetCustomProperty` can be retrieved using `GetCustomProperty` and removed using `RemoveCustomProperty`.
- The `IsSigned` and `PublicKeyToken` properties provide information about the plugin's signing status and public key token, respectively.
- The `CreatedAt` property indicates when the metadata instance was created and can be useful for tracking and logging purposes.
- The `PluginMetadata` type does not inherently provide thread safety guarantees. When accessing or modifying its properties in a multithreaded environment, appropriate synchronization mechanisms should be employed to prevent data corruption or other concurrency issues.
