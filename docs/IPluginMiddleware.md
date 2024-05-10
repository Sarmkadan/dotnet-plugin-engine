# IPluginMiddleware

`IPluginMiddleware` defines the contract for middleware components that can intercept and participate in plugin operation execution within the `dotnet-plugin-engine` framework. It enables cross-cutting concerns such as logging, timing, error handling, and metadata enrichment to be applied consistently across plugin operations without modifying the core operation logic.

## API

### `public required string OperationType`
The type of operation being executed by the plugin. This value is used to categorize and route middleware registrations, allowing specific middleware to target particular operation kinds. The value is set during middleware registration and must not be null or empty.

### `public required Plugin Plugin`
The plugin instance that owns the operation being intercepted. This provides access to the plugin’s metadata, configuration, and lifecycle hooks. The value is set during middleware registration and must not be null.

### `public Dictionary<string, object> Metadata`
A dictionary of key-value pairs that can be used to attach additional context to the operation. Middleware can read or modify this dictionary to share state between middleware instances. The dictionary is initialized as empty and is safe to mutate during execution.

### `public long StartTimeMs`
The Unix timestamp (in milliseconds) when the plugin operation started. This value is set automatically when the operation begins and is used to calculate execution duration. It is always a non-negative value representing the system clock time.

### `public long? EndTimeMs`
The Unix timestamp (in milliseconds) when the plugin operation completed, or `null` if the operation has not yet finished. This value is set automatically upon completion and is used to compute the operation’s duration. It may be `null` during active execution.

### `public Exception? Exception`
The exception thrown by the operation, if any. This value is set automatically when an exception propagates out of the operation pipeline. It remains `null` if the operation completed successfully. Middleware should check this value to determine whether the operation failed.

### `public bool IsSuccessful`
Indicates whether the plugin operation completed without throwing an exception. This value is derived from the presence or absence of an exception in `Exception`. It is `true` if `Exception` is `null`, and `false` otherwise. This property is read-only and reflects the final state of the operation.

### `public delegate Task PluginOperationDelegate`
A delegate type representing the signature of a plugin operation. It accepts no parameters and returns a `Task`, allowing asynchronous execution of the operation. This delegate is used by the middleware pipeline to chain operations and middleware.

### `public PluginMiddlewarePipeline Use(PluginOperationDelegate next)`
Registers a subsequent middleware in the pipeline. The `next` delegate represents the next middleware or the core operation to execute. This method returns a `PluginMiddlewarePipeline` that can be further configured. Calling this method multiple times chains middleware in the order of registration.

### `public PluginOperationDelegate Build()`
Constructs and returns a `PluginOperationDelegate` that represents the fully configured middleware pipeline. This delegate, when invoked, executes all registered middleware in order followed by the core operation. The returned delegate must be invoked to start the pipeline execution.

## Usage

### Example 1: Basic Timing Middleware
