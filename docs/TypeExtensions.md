# TypeExtensions

The `TypeExtensions` class provides a collection of static extension methods designed to simplify runtime reflection operations within the `dotnet-plugin-engine` ecosystem. It offers standardized utilities for inspecting type metadata, verifying interface implementations, retrieving custom attributes, and evaluating serialization capabilities, thereby reducing boilerplate code associated with manual reflection logic.

## API

### `ImplementsInterface<T>`
Determines whether a specific type implements a given interface.
*   **Parameters**: `this Type type` (the type to inspect), implicit generic parameter `T` (the interface type).
*   **Returns**: `bool` indicating `true` if the type implements the interface `T`, otherwise `false`.
*   **Throws**: `ArgumentNullException` if `type` is null; `ArgumentException` if `T` is not an interface.

### `GetTypesImplementing<T>`
Retrieves a sequence of types from a specified assembly or collection that implement a specific interface.
*   **Parameters**: `this IEnumerable<Type> types` (the source collection of types), implicit generic parameter `T` (the interface type).
*   **Returns**: `IEnumerable<Type>` containing all types in the source collection that implement interface `T`.
*   **Throws**: `ArgumentNullException` if the source collection is null; `ArgumentException` if `T` is not an interface.

### `GetLoadableTypes`
Safely retrieves all types that can be loaded from an assembly, filtering out any types that cause reflection load errors.
*   **Parameters**: `this Assembly assembly` (the assembly to scan).
*   **Returns**: `IEnumerable<Type>` containing only the successfully loaded types.
*   **Throws**: `ArgumentNullException` if `assembly` is null. This method suppresses `ReflectionTypeLoadException` internally to return partial results rather than failing entirely.

### `HasAttribute<T>`
Checks whether a type is decorated with a specific custom attribute.
*   **Parameters**: `this Type type` (the type to inspect), implicit generic parameter `T` (the attribute type).
*   **Returns**: `bool` indicating `true` if the attribute `T` is present, otherwise `false`.
*   **Throws**: `ArgumentNullException` if `type` is null; `ArgumentException` if `T` does not inherit from `Attribute`.

### `GetAttribute<T>`
Retrieves the first instance of a specific custom attribute applied to a type.
*   **Parameters**: `this Type type` (the type to inspect), implicit generic parameter `T` (the attribute type).
*   **Returns**: `T?` representing the attribute instance if found, or `null` if not present.
*   **Throws**: `ArgumentNullException` if `type` is null; `ArgumentException` if `T` does not inherit from `Attribute`.

### `IsConcreteClass`
Evaluates whether a type is a concrete class suitable for instantiation.
*   **Parameters**: `this Type type` (the type to inspect).
*   **Returns**: `bool` indicating `true` if the type is a class, is not abstract, and is not an interface; otherwise `false`.
*   **Throws**: `ArgumentNullException` if `type` is null.

### `GetFullMetadata`
Generates a comprehensive string representation of a type's metadata, including its namespace, name, generic arguments, and implemented interfaces.
*   **Parameters**: `this Type type` (the type to inspect).
*   **Returns**: `string` containing the formatted metadata description.
*   **Throws**: `ArgumentNullException` if `type` is null.

### `IsJsonSerializable`
Determines if a type can be serialized using standard JSON serialization mechanisms by checking for a parameterless constructor and public getters on properties.
*   **Parameters**: `this Type type` (the type to inspect).
*   **Returns**: `bool` indicating `true` if the type meets serialization criteria, otherwise `false`.
*   **Throws**: `ArgumentNullException` if `type` is null.

### `GetPropertyValues`
Extracts the current values of all public instance properties from an object instance into a dictionary.
*   **Parameters**: `this object obj` (the object instance to inspect).
*   **Returns**: `Dictionary<string, object?>` where keys are property names and values are the corresponding property values (or `null` if the property value is null).
*   **Throws**: `ArgumentNullException` if `obj` is null; `TargetException` if a property getter throws an exception during invocation.

## Usage

### Example 1: Plugin Discovery and Validation
This example demonstrates scanning an assembly for loadable types that implement a specific plugin interface and verifying they are concrete classes ready for instantiation.

```csharp
using System.Reflection;
using DotNetPluginEngine.Extensions;

public class PluginLoader
{
    public void LoadPlugins(Assembly assembly)
    {
        // Safely get types, ignoring any that fail to load
        var loadableTypes = assembly.GetLoadableTypes();

        // Filter for types implementing IPlugin that are concrete classes
        var pluginTypes = loadableTypes
            .Where(t => t.IsConcreteClass() && t.ImplementsInterface<IPlugin>());

        foreach (var type in pluginTypes)
        {
            if (type.HasAttribute<PluginMetadataAttribute>())
            {
                var metadata = type.GetAttribute<PluginMetadataAttribute>();
                Console.WriteLine($"Loading plugin: {metadata?.Name} ({type.GetFullMetadata()})");
            }
        }
    }
}
```

### Example 2: Configuration Serialization Check
This example illustrates validating an object's serializability before processing and extracting its property values for a flat configuration map.

```csharp
using System.Collections.Generic;
using DotNetPluginEngine.Extensions;

public class ConfigProcessor
{
    public void ProcessConfig(object configObject)
    {
        if (!configObject.GetType().IsJsonSerializable())
        {
            throw new InvalidOperationException("Configuration object is not JSON serializable.");
        }

        // Extract property values for logging or transformation
        Dictionary<string, object?> properties = configObject.GetPropertyValues();

        foreach (var prop in properties)
        {
            Console.WriteLine($"Config Key: {prop.Key}, Value: {prop.Value}");
        }
    }
}
```

## Notes

*   **Thread Safety**: All methods in `TypeExtensions` are stateless and rely solely on input parameters and the .NET reflection API. They are thread-safe for concurrent execution provided the underlying `Type` or `Assembly` objects are not being dynamically modified by external loaders during execution.
*   **Reflection Performance**: Methods such as `GetLoadableTypes` and `GetPropertyValues` utilize reflection. While `GetLoadableTypes` handles `ReflectionTypeLoadException` gracefully to prevent application crashes, frequent invocation on large assemblies may incur performance overhead. Caching results is recommended for high-frequency scenarios.
*   **Generic Constraints**: Methods accepting a generic type parameter `T` (e.g., `ImplementsInterface<T>`, `HasAttribute<T>`) do not enforce compile-time constraints on `T`. Invalid types (e.g., passing a non-interface type to `ImplementsInterface`) will result in an `ArgumentException` at runtime.
*   **Null Handling**: All extension methods explicitly check for null inputs on the `this` parameter and throw `ArgumentNullException`. They do not return `false` or empty collections in place of null inputs to fail fast and prevent silent logic errors.
*   **Serialization Heuristics**: The `IsJsonSerializable` method performs a heuristic check based on constructor availability and property accessibility. It does not guarantee successful serialization if custom converters are required or if internal runtime states prevent serialization.
