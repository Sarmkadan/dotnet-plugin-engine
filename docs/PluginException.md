# PluginException

The `PluginException` class is a specialized exception type within the `dotnet-plugin-engine` designed to facilitate structured error reporting. It extends the base `Exception` class to include diagnostic metadata, enabling plugins to convey specific error codes, associated entity identifiers, and arbitrary contextual information to the host application's error handling infrastructure.

## API

### Properties

*   **`public string ErrorCode { get; set; }`**
    Gets or sets the machine-readable error code associated with this exception.
*   **`public Guid? EntityId { get; set; }`**
    Gets or sets the optional identifier of the entity that was being processed when the exception occurred.
*   **`public Dictionary<string, object> Context { get; set; }`**
    Gets or sets the dictionary containing arbitrary contextual information related to the exception.

### Constructors

*   **`public PluginException() : base()`**
    Initializes a new instance of the `PluginException` class.
*   **`public PluginException(string message) : base(message)`**
    Initializes a new instance of the `PluginException` class with a specified error message.
*   **`public PluginException(string message, string errorCode) : base(message)`**
    Initializes a new instance of the `PluginException` class with a specified error message and error code.
*   **`public PluginException(string message, Exception innerException) : base(message, innerException)`**
    Initializes a new instance of the `PluginException` class with a specified error message and a reference to the inner exception that is the cause of this exception.

### Methods

*   **`public override string ToString()`**
    Returns a string that represents the current `PluginException`, including the `ErrorCode`, `EntityId`, and the serialized contents of the `Context` dictionary if populated.
*   **`public PluginException WithContext(string key, object value)`**
    Fluent method that adds a key-value pair to the `Context` dictionary and returns the current `PluginException` instance to allow for method chaining.
*   **`public PluginException WithEntityId(Guid entityId)`**
    Fluent method that sets the `EntityId` property and returns the current `PluginException` instance to allow for method chaining.

## Usage

### Attaching Contextual Metadata

```csharp
try
{
    // Plugin logic
}
catch (Exception ex)
{
    throw new PluginException("Failed to process plugin task.", "TASK_EXECUTION_FAILURE")
        .WithContext("AttemptCount", 3)
        .WithContext("User", "admin");
}
```

### Associating an Entity Identifier

```csharp
public void UpdateEntity(Guid id)
{
    if (id == Guid.Empty)
    {
        throw new PluginException("Invalid entity identifier.")
            .WithEntityId(id);
    }
    // Processing logic...
}
```

## Notes

*   **Thread Safety:** The `Context` property is backed by a `Dictionary<string, object>`, which is not thread-safe for concurrent writes. Access to the `Context` dictionary should be synchronized if the exception instance is shared across threads.
*   **Serialization:** As an `Exception` type, `PluginException` should be marked with `[Serializable]` if cross-domain serialization is required, though best practices generally favor serializing the structured data (ErrorCode, Context) rather than the exception object itself.
*   **Fluent API:** The `WithContext` and `WithEntityId` methods modify the state of the exception instance and return `this`, enabling chained configuration immediately prior to throwing.
