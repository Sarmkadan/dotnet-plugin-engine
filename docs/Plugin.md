# Plugin
The `Plugin` type in the `dotnet-plugin-engine` project represents a plugin that can be loaded and executed within the engine. It provides metadata about the plugin, such as its identifier, name, and version, as well as methods for managing dependencies and capabilities. This type is central to the plugin engine's functionality, allowing plugins to be discovered, loaded, and utilized at runtime.

## API
### Properties
* `Id`: A unique identifier for the plugin, represented as a `Guid`.
* `Name`: The name of the plugin, represented as a `string`.
* `Description`: A brief description of the plugin, represented as a `string`.
* `Version`: The version of the plugin, represented as a `string`.
* `Author`: The author of the plugin, represented as a `string`.
* `AssemblyPath`: The path to the plugin's assembly, represented as a `string`.
* `Status`: The current status of the plugin, represented as a `PluginStatus` enum value.
* `CreatedAt`: The date and time when the plugin was created, represented as a `DateTime`.
* `ModifiedAt`: The date and time when the plugin was last modified, represented as a `DateTime`.
* `SupportsHotReload`: A boolean indicating whether the plugin supports hot reloading.
* `LoadContextId`: The identifier of the load context for the plugin, represented as a `string`.
* `Metadata`: Optional metadata for the plugin, represented as a `PluginMetadata` object.

### Methods
* `AddDependency`: Adds a dependency to the plugin. This method takes no parameters and does not return a value.
* `RemoveDependency`: Removes a dependency from the plugin. This method takes no parameters and returns a `bool` indicating whether the removal was successful.
* `AddCapability`: Adds a capability to the plugin. This method takes no parameters and does not return a value.
* `IsValid`: Checks whether the plugin is valid. This method takes no parameters and returns a `bool` indicating whether the plugin is valid.
* `GetValidationError`: Retrieves the validation error for the plugin, if any. This method takes no parameters and returns a `string` containing the validation error message.

## Usage
The following examples demonstrate how to use the `Plugin` type:
```csharp
// Create a new plugin instance
var plugin = new Plugin
{
    Id = Guid.NewGuid(),
    Name = "My Plugin",
    Description = "A sample plugin",
    Version = "1.0.0",
    Author = "John Doe",
    AssemblyPath = "/path/to/plugin/assembly.dll",
    Status = PluginStatus.Enabled,
    CreatedAt = DateTime.Now,
    ModifiedAt = DateTime.Now,
    SupportsHotReload = true,
    LoadContextId = "my-load-context",
    Metadata = new PluginMetadata { /* metadata properties */ }
};

// Add a dependency to the plugin
plugin.AddDependency();

// Check if the plugin is valid
if (plugin.IsValid)
{
    Console.WriteLine("Plugin is valid");
}
else
{
    var error = plugin.GetValidationError();
    Console.WriteLine($"Plugin is invalid: {error}");
}
```

## Notes
When working with the `Plugin` type, consider the following edge cases and thread-safety remarks:
* The `AddDependency` and `AddCapability` methods do not return values, so their effects must be observed through other means, such as checking the plugin's dependencies or capabilities after calling these methods.
* The `RemoveDependency` method returns a `bool` indicating whether the removal was successful. If the removal fails, the method will not throw an exception, but will instead return `false`.
* The `IsValid` method checks the plugin's validity based on its current state, including its metadata and dependencies. If the plugin is invalid, the `GetValidationError` method can be used to retrieve the validation error message.
* The `Plugin` type is not thread-safe by default. If multiple threads need to access or modify a `Plugin` instance concurrently, appropriate synchronization mechanisms must be employed to ensure thread safety.
