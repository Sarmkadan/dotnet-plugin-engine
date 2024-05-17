# PluginValidator

The `PluginValidator` class encapsulates the validation logic for a single plugin within the `dotnet-plugin-engine` framework. It holds the plugin’s identity (`PluginId`, `PluginName`) and the outcome of validation (`IsValid`, `Errors`). The class provides methods to run full validation (`Validate`), to check a specific dependency relationship (`ValidateDependencyRelationship`), and to retrieve a consolidated error summary (`GetErrorSummary`). Instances are typically created with an object initializer that supplies the required identity properties, after which validation methods are called.

## API

### `public PluginValidator()`

Initializes a new instance of the `PluginValidator` class. All required properties (`PluginId`, `PluginName`, `IsValid`, `Errors`) must be set before calling any validation methods.

### `public PluginValidationResult Validate()`

Performs full validation of the plugin using the current values of `PluginId`, `PluginName`, and any other internal state. The method updates the `IsValid` and `Errors` properties based on the validation outcome.

- **Returns**: A `PluginValidationResult` object that encapsulates the validation outcome (success or failure, along with any error details).
- **Throws**: `InvalidOperationException` if `PluginId` or `PluginName` have not been set (i.e., are the default value or `null`).

### `public bool ValidateDependencyRelationship()`

Validates the dependency relationship of the plugin against the current plugin registry or dependency graph. This method is independent of the `Validate` method and can be called separately.

- **Returns**: `true` if the dependency relationship is valid; otherwise, `false`. The `Errors` list is updated with any dependency-related errors.
- **Throws**: `InvalidOperationException` if the plugin registry has not been initialized or if `PluginId` is not set.

### `public required Guid PluginId { get; set; }`

Gets or sets the unique identifier of the plugin. This property is required and must be provided during object initialization.

### `public required string PluginName { get; set; }`

Gets or sets the human-readable name of the plugin. This property is required and must be provided during object initialization.

### `public required bool IsValid { get; set; }`

Gets or sets a value indicating whether the plugin passed the most recent validation. This property is required and is typically set by the `Validate` method. It can also be set manually if needed.

### `public required List<string> Errors { get; set; }`

Gets or sets the list of validation error messages. This property is required and is populated by the `Validate` and `ValidateDependencyRelationship` methods. An empty list indicates no errors.

### `public string GetErrorSummary()`

Returns a single string that concatenates all error messages from the `Errors` list, separated by newline characters.

- **Returns**: A string containing all errors, or an empty string if `Errors` is `null` or empty.

## Usage

### Example 1: Basic plugin validation

```csharp
using System;
using System.Collections.Generic;
using PluginEngine;

var validator = new PluginValidator
{
    PluginId = Guid.NewGuid(),
    PluginName = "MyPlugin",
    IsValid = false,          // will be overwritten by Validate
    Errors = new List<string>()
};

PluginValidationResult result = validator.Validate();

if (result.IsSuccess)
{
    Console.WriteLine($"Plugin '{validator.PluginName}' is valid.");
}
else
{
    Console.WriteLine($"Validation failed: {validator.GetErrorSummary()}");
}
```

### Example 2: Dependency relationship check

```csharp
using System;
using System.Collections.Generic;
using PluginEngine;

var validator = new PluginValidator
{
    PluginId = Guid.Parse("a1b2c3d4-..."),
    PluginName = "DependentPlugin",
    IsValid = true,
    Errors = new List<string>()
};

bool dependencyOk = validator.ValidateDependencyRelationship();

if (!dependencyOk)
{
    Console.WriteLine("Dependency check failed:");
    foreach (string error in validator.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
}
else
{
    Console.WriteLine("All dependencies are satisfied.");
}
```

## Notes

- **Required properties**: `PluginId`, `PluginName`, `IsValid`, and `Errors` must be assigned before calling any validation methods. If they are not set, the class will throw an `InvalidOperationException` at runtime. Use object initializers or constructor chaining to ensure they are provided.
- **Empty `Errors` list**: The `Errors` property should be initialized with an empty `List<string>` to avoid null reference exceptions when calling `GetErrorSummary` or when validation methods attempt to add errors.
- **Validation state**: The `IsValid` and `Errors` properties are overwritten by `Validate` and `ValidateDependencyRelationship`. Manual changes to these properties between validation calls may be lost.
- **Thread safety**: This class is not thread-safe. Concurrent calls to `Validate`, `ValidateDependencyRelationship`, or property setters from multiple threads may produce inconsistent state. External synchronization is required if the same instance is used across threads.
- **`PluginValidationResult`**: The return type of `Validate` is not documented here; it is assumed to contain a `bool IsSuccess` property and possibly additional details. Refer to the `PluginValidationResult` documentation for its full API.
