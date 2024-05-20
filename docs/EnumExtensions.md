# EnumExtensions

The `EnumExtensions` class provides a comprehensive set of static utility methods designed to enhance the usability of enumeration types within the `dotnet-plugin-engine` framework. It facilitates the retrieval of metadata attributes such as descriptions and display names, enables robust parsing and value inspection, and offers specialized helpers for determining enum state characteristics like health status, transience, and terminal states. Additionally, it supports UI-related conversions, including CSS class generation and hexadecimal color representation, ensuring consistent handling of enum values across the application layer.

## API

### `GetDescription`
Retrieves the description associated with a specific enum value, typically sourced from a `DescriptionAttribute`.
- **Parameters**: `Enum value` – The enum instance to inspect.
- **Returns**: `string` – The description text if an attribute is present; otherwise, the string representation of the enum value.
- **Throws**: `ArgumentNullException` if the input value is null.

### `GetDisplayName`
Obtains the display name for a specific enum value, usually derived from a `DisplayAttribute` or similar metadata.
- **Parameters**: `Enum value` – The enum instance to inspect.
- **Returns**: `string` – The formatted display name if available; otherwise, the standard enum name.
- **Throws**: `ArgumentNullException` if the input value is null.

### `ToUserFriendlyString`
Converts an enum value into a human-readable string format, often applying spacing or casing transformations to the underlying name.
- **Parameters**: `Enum value` – The enum instance to convert.
- **Returns**: `string` – A formatted string suitable for user interfaces.
- **Throws**: `ArgumentNullException` if the input value is null.

### `ToCssClass`
Generates a CSS class name string corresponding to the enum value, typically converting the name to kebab-case or lowercase.
- **Parameters**: `Enum value` – The enum instance to convert.
- **Returns**: `string` – A valid CSS class identifier.
- **Throws**: `ArgumentNullException` if the input value is null.

### `IsHealthy`
Determines whether the specific enum value represents a "healthy" or operational state based on predefined framework conventions.
- **Parameters**: `Enum value` – The enum instance to evaluate.
- **Returns**: `bool` – `true` if the state is considered healthy; otherwise, `false`.
- **Throws**: `ArgumentNullException` if the input value is null.

### `IsTransient`
Checks if the enum value indicates a transient, temporary, or intermediate state.
- **Parameters**: `Enum value` – The enum instance to evaluate.
- **Returns**: `bool` – `true` if the state is transient; otherwise, `false`.
- **Throws**: `ArgumentNullException` if the input value is null.

### `GetAllValues<T>`
Retrieves a collection of all defined values for a specified enum type.
- **Parameters**: None (generic type `T` defines the target enum).
- **Returns**: `IEnumerable<T>` – A sequence containing all valid values of type `T`.
- **Throws**: `ArgumentException` if `T` is not an enum type.

### `TryParse<T>`
Attempts to parse a string representation into a specific enum value.
- **Parameters**: `string value` – The string to parse; `bool ignoreCase` (optional) – Specifies whether to ignore case during parsing.
- **Returns**: `T?` – The parsed enum value if successful; `null` if the string does not match any defined name or number.
- **Throws**: `ArgumentException` if `T` is not an enum type.

### `GetIntValue`
Extracts the underlying integer value of an enum instance.
- **Parameters**: `Enum value` – The enum instance to convert.
- **Returns**: `int` – The numeric value associated with the enum member.
- **Throws**: `ArgumentNullException` if the input value is null.

### `GetValueDescriptions<T>`
Retrieves a list of tuples pairing each enum value with its corresponding description string.
- **Parameters**: None (generic type `T` defines the target enum).
- **Returns**: `List<(T Value, string Description)>` – A list containing all enum values and their resolved descriptions.
- **Throws**: `ArgumentException` if `T` is not an enum type.

### `ToColorHex`
Converts an enum value into a hexadecimal color code string, typically based on associated color attributes or predefined mappings.
- **Parameters**: `Enum value` – The enum instance to convert.
- **Returns**: `string` – A hex color string (e.g., "#FF5733").
- **Throws**: `ArgumentNullException` if the input value is null; may throw if no color mapping exists depending on implementation strictness.

### `IsTerminal`
Determines whether the enum value represents a terminal state, indicating that no further state transitions are expected.
- **Parameters**: `Enum value` – The enum instance to evaluate.
- **Returns**: `bool` – `true` if the state is terminal; otherwise, `false`.
- **Throws**: `ArgumentNullException` if the input value is null.

## Usage

### Example 1: Retrieving Metadata for UI Display
This example demonstrates how to retrieve user-friendly strings and descriptions for populating a dropdown menu or status badge.

```csharp
using DotNetPluginEngine.Extensions;

public enum PluginStatus
{
    [Description("Initializing components")]
    Initializing,
    
    [Description("Running normally")]
    Healthy,
    
    [Description("Temporary network issue")]
    TransientError,
    
    [Description("Critical failure")]
    Fatal
}

public void RenderStatusBadge(PluginStatus status)
{
    // Get the detailed description for tooltips
    string tooltip = EnumExtensions.GetDescription(status);
    
    // Get a CSS class for styling (e.g., "plugin-status-healthy")
    string cssClass = EnumExtensions.ToCssClass(status);
    
    // Check state logic
    if (EnumExtensions.IsHealthy(status))
    {
        Console.WriteLine($"Status: {EnumExtensions.ToUserFriendlyString(status)} ({tooltip})");
    }
}
```

### Example 2: Parsing and State Inspection
This example illustrates parsing a string input safely and inspecting state characteristics like transience and termination.

```csharp
using DotNetPluginEngine.Extensions;
using System;

public enum LifecycleState
{
    Pending = 0,
    Running = 1,
    Paused = 2,
    Completed = 3,
    Failed = 4
}

public void ProcessStateInput(string input)
{
    // Attempt to parse the string safely
    var state = EnumExtensions.TryParse<LifecycleState>(input, ignoreCase: true);

    if (state.HasValue)
    {
        int rawValue = EnumExtensions.GetIntValue(state.Value);
        bool isDone = EnumExtensions.IsTerminal(state.Value);
        bool isTemporary = EnumExtensions.IsTransient(state.Value);

        Console.WriteLine($"Parsed: {state.Value} (Int: {rawValue})");
        
        if (isDone)
        {
            Console.WriteLine("Process has reached a terminal state.");
        }
        else if (isTemporary)
        {
            Console.WriteLine("State is transient; retry logic may apply.");
        }
    }
    else
    {
        Console.WriteLine("Invalid state provided.");
    }
}
```

## Notes

- **Null Safety**: All instance-based extension methods (those accepting an `Enum` parameter) will throw an `ArgumentNullException` if the input enum value is null. Callers should ensure valid instances before invocation.
- **Generic Constraints**: Methods utilizing the generic type `T` (e.g., `GetAllValues<T>`, `TryParse<T>`) require `T` to be a valid enum type. Passing a non-enum type will result in an `ArgumentException` at runtime.
- **Attribute Dependency**: Methods such as `GetDescription`, `GetDisplayName`, and `ToColorHex` rely on the presence of specific attributes (`DescriptionAttribute`, `DisplayAttribute`, etc.) decorated on the enum fields. If these attributes are missing, the methods typically fall back to default string representations or predefined logic rather than throwing exceptions, though behavior may vary by specific implementation details of the color mapping.
- **Thread Safety**: As this class consists entirely of static methods that operate on immutable input data or reflect upon type metadata without maintaining internal mutable state, all members are thread-safe and can be called concurrently from multiple threads without synchronization.
- **Parsing Behavior**: `TryParse<T>` returns `null` (for nullable enums) upon failure rather than throwing an exception, making it suitable for processing untrusted user input or configuration strings.
