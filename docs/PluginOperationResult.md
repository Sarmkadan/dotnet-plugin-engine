# PluginOperationResult

A result object used to report the outcome of plugin operations, including success/failure status, timing information, and optional payloads. It supports both simple operations (without payload) and generic operations (with typed payload).

## API

### Properties

- **`bool Success`**
  Indicates whether the operation completed successfully.
  *Read-only.*

- **`string Message`**
  Human-readable message describing the outcome of the operation.
  *Read-only.*

- **`int? ErrorCode`**
  Optional error code associated with a failed operation.
  *Read-only.*

- **`string? ErrorDetails`**
  Optional detailed error information for debugging or logging.
  *Read-only.*

- **`long DurationMs`**
  Duration of the operation in milliseconds.
  *Read-only.*

- **`DateTime TimestampUtc`**
  UTC timestamp when the result was created.
  *Read-only.*

- **`T? Data`**
  Generic payload attached to a successful operation.
  *Read-only.*

### Static Factory Methods (Non-Generic)

- **`PluginOperationResult CreateSuccess(string message = "")`**
  Creates a successful result with an optional message.
  *Parameters:*
  - `message` – Optional description of the success.
  *Returns:* A `PluginOperationResult` with `Success = true`, `Message = message`, and `DurationMs = 0`.

- **`PluginOperationResult CreateFailure(string message, int? errorCode = null, string? errorDetails = null)`**
  Creates a failed result with a message, optional error code, and optional details.
  *Parameters:*
  - `message` – Description of the failure.
  - `errorCode` – Optional error code.
  - `errorDetails` – Optional detailed error information.
  *Returns:* A `PluginOperationResult` with `Success = false`, populated fields, and `DurationMs = 0`.

- **`PluginOperationResult FromException(Exception ex, int? errorCode = null)`**
  Creates a failed result from an exception.
  *Parameters:*
  - `ex` – The exception to convert.
  - `errorCode` – Optional error code.
  *Returns:* A `PluginOperationResult` with `Success = false`, `Message` derived from `ex.Message`, `ErrorCode = errorCode`, `ErrorDetails` derived from `ex.ToString()`, and `DurationMs = 0`.

### Static Factory Methods (Generic)

- **`PluginOperationResult<T> CreateSuccess(T? data = default, string message = "")`**
  Creates a successful generic result with optional payload and message.
  *Parameters:*
  - `data` – Optional payload.
  - `message` – Optional description.
  *Returns:* A `PluginOperationResult<T>` with `Success = true`, `Data = data`, `Message = message`, and `DurationMs = 0`.

- **`PluginOperationResult<T> CreateFailure(string message, int? errorCode = null, string? errorDetails = null)`**
  Creates a failed generic result.
  *Parameters:*
  - `message` – Description of the failure.
  - `errorCode` – Optional error code.
  - `errorDetails` – Optional detailed error information.
  *Returns:* A `PluginOperationResult<T>` with `Success = false`, populated fields, and `DurationMs = 0`.

- **`PluginOperationResult<T> FromException(Exception ex, int? errorCode = null)`**
  Creates a failed generic result from an exception.
  *Parameters:*
  - `ex` – The exception to convert.
  - `errorCode` – Optional error code.
  *Returns:* A `PluginOperationResult<T>` with `Success = false`, `Message` derived from `ex.Message`, `ErrorCode = errorCode`, `ErrorDetails` derived from `ex.ToString()`, and `DurationMs = 0`.

### Aggregation Properties (Non-Generic)

- **`int SuccessCount`**
  Number of successful results in an aggregated collection.
  *Read-only.*

- **`int FailureCount`**
  Number of failed results in an aggregated collection.
  *Read-only.*

- **`List<(Guid PluginId, string PluginName, PluginOperationResult Result)> Results`**
  Collection of plugin results with their identifiers and names.
  *Read-only.*

- **`long TotalDurationMs`**
  Sum of durations across all results in an aggregated collection.
  *Read-only.*

### Aggregation Methods (Non-Generic)

- **`void AddResult(Guid pluginId, string pluginName, PluginOperationResult result)`**
  Adds a plugin result to the aggregation.
  *Parameters:*
  - `pluginId` – Unique identifier of the plugin.
  - `pluginName` – Display name of the plugin.
  - `result` – Result to add.
  *Throws:* `ArgumentNullException` if `result` is `null`.

- **`string GetSummary()`**
  Generates a human-readable summary of the aggregated results.
  *Returns:* A string summarizing success/failure counts and total duration.

## Usage
