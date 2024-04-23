# PluginExceptionExtensions

The `PluginExceptionExtensions` class provides a set of static extension methods designed to enhance the handling, contextualization, and reporting of `PluginException` instances within the `dotnet-plugin-engine`. These extensions facilitate the standardization of error reporting, the attachment of relevant diagnostic information, and the consistent interpretation of error states, allowing for cleaner exception management across plugin implementations.

## API

### WithErrorCode
```csharp
public static PluginException WithErrorCode(this PluginException exception, string errorCode)
```
*   **Purpose**: Associates a specific error code string with an existing `PluginException` instance.
*   **Parameters**:
    *   `exception`: The target `PluginException`.
    *   `errorCode`: The error code to associate.
*   **Returns**: The `PluginException` instance with the error code set.

### WithContext
```csharp
public static PluginException WithContext(this PluginException exception, params object[] context)
```
*   **Purpose**: Appends contextual information to an existing `PluginException` instance, allowing for easier debugging.
*   **Parameters**:
    *   `exception`: The target `PluginException`.
    *   `context`: A variable number of objects providing additional context.
*   **Returns**: The `PluginException` instance updated with the provided context.

### ToDiagnosticReport
```csharp
public static string ToDiagnosticReport(this PluginException exception)
```
*   **Purpose**: Generates a detailed, machine-readable string report of the exception, including error codes and attached context, suitable for logs or debugging dashboards.
*   **Parameters**:
    *   `exception`: The target `PluginException`.
*   **Returns**: A formatted string containing the diagnostic report.

### ToUserFriendlyMessage
```csharp
public static string ToUserFriendlyMessage(this PluginException exception)
```
*   **Purpose**: Translates the exception into a localized or simplified message intended for end-user display, hiding implementation details.
*   **Parameters**:
    *   `exception`: The target `PluginException`.
*   **Returns**: A user-friendly string representation of the exception.

### IsErrorCode
```csharp
public static bool IsErrorCode(this PluginException exception, string errorCode)
```
*   **Purpose**: Checks if the `PluginException` instance is associated with a specific error code.
*   **Parameters**:
    *   `exception`: The target `PluginException`.
    *   `errorCode`: The error code to verify.
*   **Returns**: `true` if the exception matches the provided error code, otherwise `false`.

## Usage

### Attaching Context and Reporting Errors
```csharp
try
{
    // Perform plugin operation
}
catch (PluginException ex)
{
    throw ex.WithErrorCode("ERR_PLUGIN_INIT_001")
            .WithContext("Timestamp", DateTime.UtcNow, "Attempt", 3);
}
```

### Checking Error Codes and Generating Reports
```csharp
catch (PluginException ex)
{
    if (ex.IsErrorCode("ERR_PLUGIN_INIT_001"))
    {
        Log.Error(ex.ToDiagnosticReport());
        ShowUserMessage(ex.ToUserFriendlyMessage());
    }
}
```

## Notes

*   **Thread Safety**: These extension methods are thread-safe as they operate on existing `PluginException` instances, provided the underlying `PluginException` implementation is itself thread-safe or accessed in a thread-safe manner.
*   **Null Handling**: These extensions expect a valid `PluginException` instance; passing a `null` reference will result in a `NullReferenceException` when accessing the `this` parameter.
*   **Context Serialization**: When using `WithContext`, ensure that the objects passed are serializable if the diagnostic reports are destined for persistence or transmission across boundaries.
