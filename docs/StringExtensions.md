# StringExtensions

Utility class providing common string manipulation and validation methods for plugin identifiers, paths, versions, and human-readable formatting.

## API

### `public static string NormalizePluginPath(string path)`

Normalizes a plugin path to use consistent directory separators and removes redundant separators or trailing slashes.

- **Parameters**
  - `path`: The input path to normalize.
- **Returns**
  - The normalized path with forward slashes and no trailing slashes.
- **Throws**
  - `ArgumentNullException`: If `path` is `null`.

---

### `public static bool IsValidPluginId(string id)`

Determines whether the given string is a valid plugin identifier.

- **Parameters**
  - `id`: The plugin identifier to validate.
- **Returns**
  - `true` if the identifier is non-null, non-empty, and contains only alphanumeric characters, hyphens, underscores, and periods; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `id` is `null`.

---

### `public static bool IsValidVersion(string version)`

Determines whether the given string is a valid semantic version string.

- **Parameters**
  - `version`: The version string to validate.
- **Returns**
  - `true` if the version matches a semantic version pattern (e.g., `1.0.0`, `2.3.4-beta`); otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `version` is `null`.

---
### `public static string SanitizeForFilename(string input)`

Sanitizes a string to be safe for use as a filename by removing or replacing invalid characters.

- **Parameters**
  - `input`: The string to sanitize.
- **Returns**
  - A sanitized string with invalid filename characters replaced by underscores.
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.

---
### `public static string GetAssemblyName(string path)`

Extracts the assembly name from a file path without the extension.

- **Parameters**
  - `path`: The file path to process.
- **Returns**
  - The assembly name derived from the filename without the `.dll` or `.exe` extension.
- **Throws**
  - `ArgumentNullException`: If `path` is `null`.
  - `ArgumentException`: If `path` does not end with `.dll` or `.exe`.

---
### `public static bool IsAssemblyPath(string path)`

Determines whether the given path points to a valid assembly file (`.dll` or `.exe`).

- **Parameters**
  - `path`: The file path to check.
- **Returns**
  - `true` if the path ends with `.dll` or `.exe`; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `path` is `null`.

---
### `public static string TruncateWithEllipsis(string input, int maxLength)`

Truncates a string to a maximum length and appends an ellipsis if truncated.

- **Parameters**
  - `input`: The string to truncate.
  - `maxLength`: The maximum allowed length of the result.
- **Returns**
  - The truncated string with an ellipsis (`…`) appended if the original exceeds `maxLength`; otherwise, the original string.
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.
  - `ArgumentOutOfRangeException`: If `maxLength` is less than 3.

---
### `public static string ToReadableTimeSpan(TimeSpan span)`

Converts a `TimeSpan` into a human-readable string (e.g., "2 days, 3 hours").

- **Parameters**
  - `span`: The time span to format.
- **Returns**
  - A localized, human-readable representation of the time span.
- **Throws**
  - None.

---
### `public static string FormatBytes(long bytes)`

Formats a byte count into a human-readable string with appropriate unit (e.g., "1.2 MB").

- **Parameters**
  - `bytes`: The number of bytes to format.
- **Returns**
  - A string representing the byte count with a unit suffix (e.g., "1.2 MB", "500 B").
- **Throws**
  - None.

## Usage
