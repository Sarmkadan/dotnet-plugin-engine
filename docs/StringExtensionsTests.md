# StringExtensionsTests

The `StringExtensionsTests` class contains unit tests that validate the behavior of the `StringExtensions` utility methods. Each test method exercises a specific scenario—such as null input, valid or invalid formats, and boundary conditions—to ensure the extension methods produce correct results or throw appropriate exceptions. The tests are designed to be run with a standard test framework (e.g., xUnit, NUnit) and cover the core string manipulation functions used throughout the plugin engine.

## API

### `NormalizePluginPath_WithNullOrWhitespace_ReturnsEmpty`
Tests that `StringExtensions.NormalizePluginPath` returns `string.Empty` when the input is `null`, empty, or consists only of whitespace.

### `NormalizePluginPath_WithForwardSlashes_ConvertsToOsSeparator`
Verifies that forward slashes (`/`) in the input are replaced with the operating system’s directory separator character (`\` on Windows, `/` on Unix).

### `NormalizePluginPath_WithTrailingSeparator_RemovesTrailingSeparator`
Ensures that a trailing directory separator (either `\` or `/`) is removed from the normalized path.

### `IsValidPluginId_WithValidGuid_ReturnsTrue`
Confirms that a string representing a valid GUID (e.g., `"12345678-1234-1234-1234-123456789abc"`) returns `true`.

### `IsValidPluginId_WithInvalidString_ReturnsFalse`
Confirms that a non-GUID string (e.g., `"not-a-guid"`) returns `false`.

### `IsValidPluginId_WithEmptyString_ReturnsFalse`
Confirms that an empty string returns `false`.

### `IsValidVersion_WithSemanticVersion_ReturnsTrue`
Verifies that a valid semantic version string (e.g., `"1.2.3"`) returns `true`.

### `IsValidVersion_WithNonVersionString_ReturnsFalse`
Verifies that a string that cannot be parsed as a version (e.g., `"abc"`) returns `false`.

### `SanitizeForFilename_WithEmptyString_ReturnsEmpty`
Tests that an empty input returns an empty string.

### `SanitizeForFilename_WithValidName_ReturnsUnchanged`
Confirms that a filename without invalid characters (e.g., `"myFile.txt"`) is returned unchanged.

### `SanitizeForFilename_WithInvalidFileChars_RemovesInvalidChars`
Ensures that characters invalid for file names (e.g., `\`, `/`, `:`, `*`, `?`, `"`, `<`, `>`, `|`) are removed from the result.

### `GetAssemblyName_WithDllPath_ReturnsNameWithoutExtension`
Tests that a full DLL path (e.g., `"C:\plugins\myplugin.dll"`) returns only the file name without the extension (`"myplugin"`).

### `GetAssemblyName_WithNestedPath_ReturnsOnlyFilenameWithoutExtension`
Verifies that a deeply nested path (e.g., `"subdir\another\plugin.dll"`) returns only the file name without extension (`"plugin"`).

### `IsAssemblyPath_WithDllExtension_ReturnsTrue`
Confirms that a path ending with `.dll` (case-insensitive) returns `true`.

### `IsAssemblyPath_WithExeExtension_ReturnsTrue`
Confirms that a path ending with `.exe` returns `true`.

### `IsAssemblyPath_WithTxtExtension_ReturnsFalse`
Confirms that a path ending with `.txt` returns `false`.

### `IsAssemblyPath_WithUppercaseDllExtension_ReturnsTrue`
Verifies that a path ending with `.DLL` (uppercase) returns `true`.

### `TruncateWithEllipsis_WhenShorterThanMax_ReturnsOriginal`
Tests that a string shorter than the specified maximum length is returned unchanged.

### `TruncateWithEllipsis_WhenEqualToMax_ReturnsOriginal`
Tests that a string exactly equal to the maximum length is returned unchanged.

### `TruncateWithEllipsis_WhenLongerThanMax_TruncatesWithEllipsis`
Verifies that a string longer than the maximum length is truncated to `maxLength - 3` characters and appended with `"..."`.

## Usage

The following examples demonstrate how the extension methods are used in production code. The tests in `StringExtensionsTests` verify that these methods behave correctly under the conditions shown.

```csharp
// Example 1: Normalizing a plugin path and sanitizing a filename
string rawPath = "plugins//MyPlugin/";
string normalized = rawPath.NormalizePluginPath();
// normalized == "plugins\MyPlugin" on Windows, "plugins/MyPlugin" on Unix

string unsafeName = "file:name?.txt";
string safeName = unsafeName.SanitizeForFilename();
// safeName == "filename.txt"
```

```csharp
// Example 2: Validating a plugin ID and truncating a display string
string pluginId = "550e8400-e29b-41d4-a716-446655440000";
bool isValid = pluginId.IsValidPluginId();
// isValid == true

string longDescription = "This is a very long description that exceeds the maximum allowed length for display.";
string truncated = longDescription.TruncateWithEllipsis(30);
// truncated == "This is a very long desc..."
```

## Notes

- All extension methods are static and stateless; they are thread-safe and can be called concurrently from multiple threads without synchronization.
- `NormalizePluginPath` uses `Path.DirectorySeparatorChar` to convert separators; on Unix systems forward slashes are already the native separator, so the method only removes trailing separators.
- `SanitizeForFilename` removes all characters returned by `Path.GetInvalidFileNameChars()`. It does not trim whitespace or limit the length of the result.
- `GetAssemblyName` relies on `Path.GetFileNameWithoutExtension` and will throw an `ArgumentException` if the path contains invalid characters (e.g., null characters). The test methods assume valid paths are provided.
- `IsAssemblyPath` performs a case-insensitive comparison for the `.dll` and `.exe` extensions.
- `TruncateWithEllipsis` requires `maxLength` to be at least 4 (to accommodate the three‑character ellipsis). If `maxLength` is less than 4, the method may return a string shorter than expected or throw an exception depending on the implementation. The tests do not cover this edge case.
- The test methods themselves are not thread-safe in the sense that they may rely on shared test fixtures, but each test is independent and can be run in parallel with a compatible test runner.
