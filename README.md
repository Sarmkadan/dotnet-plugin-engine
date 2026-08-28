# dotnet-plugin-engine

The `IPluginEventPublisher` interface provides a set of extension methods for event publishing, subscription management, and diagnostics.

## PluginIncompatibleExceptionExtensionsTests

The `PluginIncompatibleExceptionExtensionsTests` class contains unit tests for the extension methods that operate on `PluginIncompatibleException`. These tests verify that the extension methods correctly determine version incompatibility, format incompatibility reasons, and handle null or empty values.

## PluginExceptionTests

The `PluginExceptionTests` class contains unit tests for the `PluginException` class, verifying its constructors, property getters, fluent modification methods, and string representation. It ensures that error codes, entity IDs, and contextual metadata are correctly stored and that appropriate exceptions are thrown for invalid inputs.

## PluginLoadExceptionExtensionsTests

The `PluginLoadExceptionExtensionsTests` class contains unit tests for the extension methods that operate on `PluginLoadException`. These tests verify that the extension methods correctly check the load stage, generate a formatted failure summary, and create a new exception with an updated load stage while preserving other properties.

Example usage:
```csharp
var ex = new PluginLoadException("Failed to load", "MyPlugin", "path/to/plugin.dll", PluginLoadStage.AssemblyResolution);
bool isAtResolution = ex.IsLoadStage(PluginLoadStage.AssemblyResolution); // returns true
string summary = ex.GetLoadFailureSummary(); // returns "Failed to load plugin 'MyPlugin' from 'path/to/plugin.dll' at stage AssemblyResolution."
var updatedEx = ex.WithLoadStage(PluginLoadStage.TypeLoading); // returns a new exception with LoadStage set to TypeLoading
```

## DependencyResolutionExceptionValidationTests

The `DependencyResolutionExceptionValidationTests` class verifies the validation logic for `DependencyResolutionException` objects. It ensures that the exception contains valid version constraints, reasons, and a list of unresolved dependencies, and that the validation methods correctly report problems or confirm validity.

Example usage:
```csharp
using PluginEngine.Exceptions;
using PluginEngine.Validation;

var ex = new DependencyResolutionException(
    "Missing dependency",
    "MyPlugin",
    new[] { "DependencyA", "DependencyB" });

var problems = DependencyResolutionExceptionValidator.Validate(ex);
if (problems.Any())
{
    foreach (var p in problems)
    {
        Console.WriteLine(p);
    }
}
else
{
    Console.WriteLine("Exception is valid.");
}
```

## VersionMismatchExceptionTests

The `VersionMismatchExceptionTests` class contains unit tests for the `VersionMismatchException` class, verifying its constructors and the `ToString` method under various conditions.

Example usage:
```csharp
using PluginEngine.Exceptions;

// Create an exception with version details
var ex = new VersionMismatchException(
    "Version mismatch detected",
    "1.0.0",
    "2.0.0",
    "Plugin",
    "MyPlugin");

// The ToString method includes the version and component information
string errorMessage = ex.ToString();
```
