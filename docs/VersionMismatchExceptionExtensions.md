# VersionMismatchExceptionExtensions

Provides utility methods for working with `VersionMismatchException` instances, including formatting error messages, checking critical mismatches, and enriching exceptions with contextual information.

## API

### `public static string GetFormattedErrorMessage(VersionMismatchException exception)`

Formats a detailed error message from a `VersionMismatchException` that includes version mismatch details, context, and recommendations.

- **Parameters**
  - `exception`: The `VersionMismatchException` instance to format.

- **Return value**
  Returns a formatted error message string. Returns `null` if `exception` is `null`.

- **Exceptions**
  - Throws `ArgumentNullException` if `exception` is `null`.

---

### `public static bool IsCriticalVersionMismatch(VersionMismatchException exception)`

Determines whether the version mismatch represented by the exception is considered critical, typically indicating a breaking change or incompatible version requirement.

- **Parameters**
  - `exception`: The `VersionMismatchException` instance to evaluate.

- **Return value**
  Returns `true` if the mismatch is critical; otherwise, returns `false`. Returns `false` if `exception` is `null`.

---

### `public static VersionMismatchException WithContext(this VersionMismatchException exception, string context)`

Adds contextual information to a `VersionMismatchException` and returns a new exception instance with the updated context.

- **Parameters**
  - `exception`: The original `VersionMismatchException` instance.
  - `context`: A string providing additional context about the mismatch (e.g., plugin name, environment).

- **Return value**
  Returns a new `VersionMismatchException` with the context appended to the existing context. Returns `null` if `exception` is `null`.

- **Exceptions**
  - Throws `ArgumentNullException` if `context` is `null`.

---

### `public static string GetSimplifiedMessage(VersionMismatchException exception)`

Extracts a concise error message from a `VersionMismatchException`, suitable for user-facing output without detailed technical information.

- **Parameters**
  - `exception`: The `VersionMismatchException` instance to process.

- **Return value**
  Returns a simplified error message string. Returns `null` if `exception` is `null`.

## Usage
