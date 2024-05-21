# IPluginFormatter

The `IPluginFormatter` interface defines the contract for plugin formatters in the `dotnet-plugin-engine` project. It provides metadata about a plugin formatter, its capabilities, and methods to retrieve supported formats and create formatters. This interface is used to standardize plugin formatting operations and ensure compatibility with the plugin system.

## API

### `PluginId`
- **Type**: `Guid`
- **Description**: A unique identifier for the plugin formatter. This value is required and must be set when implementing the interface.
- **Purpose**: Uniquely identifies the formatter within the plugin system.

### `PluginName`
- **Type**: `string`
- **Description**: A human-readable name for the plugin formatter. This value is required and must be set when implementing the interface.
- **Purpose**: Provides a descriptive name for display or logging purposes.

### `Status`
- **Type**: `string`
- **Description**: A string representing the current status of the plugin formatter (e.g., "Active", "Deprecated", "Error"). This value is required and must be set when implementing the interface.
- **Purpose**: Indicates the operational state of the formatter for diagnostics or user feedback.

### `DependencyCount`
- **Type**: `int`
- **Description**: The number of dependencies required by the plugin formatter. This value is required and must be set when implementing the interface.
- **Purpose**: Helps track and manage plugin dependencies.

### `CapabilityCount`
- **Type**: `int`
- **Description**: The number of capabilities or features supported by the plugin formatter. This value is required and must be set when implementing the interface.
- **Purpose**: Provides insight into the formatter's functionality.

### `LoadTimeMs`
- **Type**: `long`
- **Description**: The time taken to load the plugin formatter, in milliseconds.
- **Purpose**: Used for performance monitoring and optimization.

### `LastAccessedUtc`
- **Type**: `DateTime`
- **Description**: The UTC timestamp of the last time the plugin formatter was accessed.
- **Purpose**: Tracks usage patterns for lifecycle management.

### `IsHealthy`
- **Type**: `bool`
- **Description**: Indicates whether the plugin formatter is in a healthy state (e.g., no critical errors).
- **Purpose**: Used for health checks and failover logic.

### `Issues`
- **Type**: `List<string>`
- **Description**: A list of issues or warnings associated with the plugin formatter.
- **Purpose**: Provides diagnostic information for troubleshooting.

### `FormatterFactory`
- **Type**: `public` (type not specified in the provided members)
- **Description**: A factory or delegate responsible for creating instances of the plugin formatter.
- **Purpose**: Enables dynamic instantiation of formatters.

### `GetFormatter()`
- **Return Type**: `IPluginFormatter?`
- **Description**: Creates and returns an instance of the plugin formatter.
- **Returns**: An instance of the formatter, or `null` if creation fails.
- **Throws**: May throw exceptions if the formatter cannot be instantiated (e.g., missing dependencies).
- **Purpose**: Provides a way to instantiate the formatter for use.

### `GetSupportedFormats()`
- **Return Type**: `IEnumerable<string>`
- **Description**: Retrieves the list of supported format identifiers (e.g., "json", "xml").
- **Returns**: An enumerable collection of supported format strings.
- **Throws**: May throw exceptions if the supported formats cannot be determined.
- **Purpose**: Enables consumers to check compatibility with specific formats.

## Usage

### Example 1: Basic Usage
