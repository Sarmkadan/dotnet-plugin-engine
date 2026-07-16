# dotnet-plugin-engine

[... existing content ...]

## StringExtensions

The `StringExtensions` class provides a set of extension methods for string operations commonly used in plugin processing. It offers utilities for path normalization, plugin identifier and version validation, filename sanitization, and more. These extensions simplify tasks such as handling plugin paths, validating plugin identifiers and versions, and formatting strings for better readability.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Utils.Extensions;

public class StringExtensionsDemo
{
    public void DemonstrateStringExtensions()
    {
        // Normalize a plugin path
        string normalizedPath = "/path/to/plugin/'.NormalizePluginPath();
        Console.WriteLine($"Normalized path: {normalizedPath}");

        // Validate a plugin identifier
        string pluginId = "123e4567-e89b-12d3-a456-426655440000";
        bool isValidId = pluginId.IsValidPluginId();
        Console.WriteLine($"Is valid plugin ID: {isValidId}");

        // Validate a semantic version
        string version = "1.2.3";
        bool isValidVersion = version.IsValidVersion();
        Console.WriteLine($"Is valid version: {isValidVersion}");

        // Sanitize a filename
        string filename = "My Plugin.dll";
        string sanitizedFilename = filename.SanitizeForFilename();
        Console.WriteLine($"Sanitized filename: {sanitizedFilename}");

        // Get the assembly name from a file path
        string filePath = "/path/to/MyPlugin.dll";
        string assemblyName = filePath.GetAssemblyName();
        Console.WriteLine($"Assembly name: {assemblyName}");

        // Check if a path is an assembly path
        bool isAssemblyPath = filePath.IsAssemblyPath();
        Console.WriteLine($"Is assembly path: {isAssemblyPath}");

        // Truncate a string with an ellipsis
        string longString = "This is a very long string that needs to be truncated.";
        string truncatedString = longString.TruncateWithEllipsis(20);
        Console.WriteLine($"Truncated string: {truncatedString}");

        // Convert a TimeSpan to a readable format
        TimeSpan timeSpan = TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30)).Add(TimeSpan.FromSeconds(45));
        string readableTimeSpan = timeSpan.ToReadableTimeSpan();
        Console.WriteLine($"Readable time span: {readableTimeSpan}");

        // Format bytes
        long bytes = 1024 * 1024 * 2;
        string formattedBytes = bytes.FormatBytes();
        Console.WriteLine($"Formatted bytes: {formattedBytes}");
    }
}
```

## MemoryPluginCache

[... existing content ...]
