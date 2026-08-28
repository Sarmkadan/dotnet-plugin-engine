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

