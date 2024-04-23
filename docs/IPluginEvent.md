# IPluginEvent

The `IPluginEvent` interface defines the core contract for tracking and auditing events within the `dotnet-plugin-engine`. It provides a standardized structure for capturing metadata, timing, and error information related to plugin lifecycle transitions, dependency resolution, and runtime failures, enabling consistent telemetry and logging across the plugin ecosystem.

## API

| Member | Type | Description |
| :--- | :--- | :--- |
| `EventId` | `Guid` | A unique identifier for the specific event instance. |
| `OccurredAtUtc` | `DateTime` | The timestamp indicating when the event occurred, in UTC. |
| `PluginId` | `Guid` | The unique identifier of the plugin associated with the event. Required. |
| `EventType` | `string` | A string representing the category or type of the event. Must be implemented by derived types. |
| `PluginName` | `string` | The human-readable name of the plugin. Required. |
| `Version` | `string` | The version string of the plugin. Required. |
| `LoadTimeMs` | `long` | The time taken to load the plugin, in milliseconds, if applicable. |
| `Reason` | `string?` | An optional description explaining the cause of the event. |
| `PreviousVersion` | `string` | The version of the plugin prior to an update. Required. |
| `NewVersion` | `string` | The version of the plugin after an update. Required. |
| `ChangesSummary` | `string` | A summary of changes associated with the event, such as an update. Required. |
| `ErrorMessage` | `string` | The error message captured during a failure event. Required. |
| `ErrorDetails` | `string?` | Optional, detailed diagnostic information regarding the error. |
| `ErrorCode` | `int` | A numerical code identifying the specific type of error. |
| `ResolvedDependencies` | `List<Guid>` | A list of GUIDs representing the dependencies resolved for the plugin. |
| `ResolutionTimeMs` | `long` | The duration, in milliseconds, taken to resolve plugin dependencies. |

## Usage

### Example 1: Logging a Successful Plugin Load
```csharp
public class PluginLoadEvent : IPluginEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public required Guid PluginId { get; init; }
    public string EventType => "PluginLoaded";
    public required string PluginName { get; init; }
    public required string Version { get; init; }
    public long LoadTimeMs { get; init; }
    public required string PreviousVersion { get; init; }
    public required string NewVersion { get; init; }
    public required string ChangesSummary { get; init; }
    public required string ErrorMessage { get; init; }
    public string? ErrorDetails { get; init; }
    public int ErrorCode { get; init; }
    public List<Guid> ResolvedDependencies { get; init; } = new();
    public long ResolutionTimeMs { get; init; }
    public string? Reason { get; init; }
}

// Usage
var loadEvent = new PluginLoadEvent {
    PluginId = Guid.NewGuid(),
    PluginName = "LoggingPlugin",
    Version = "1.0.0",
    LoadTimeMs = 150,
    PreviousVersion = "0.0.0",
    NewVersion = "1.0.0",
    ChangesSummary = "Initial install",
    ErrorMessage = ""
};
```

### Example 2: Handling Dependency Resolution Failures
```csharp
public void HandleDependencyError(IPluginEvent pluginEvent)
{
    if (pluginEvent.ErrorCode != 0)
    {
        Console.WriteLine($"Error in {pluginEvent.PluginName}: {pluginEvent.ErrorMessage}");
        if (!string.IsNullOrEmpty(pluginEvent.ErrorDetails))
        {
            Console.WriteLine($"Details: {pluginEvent.ErrorDetails}");
        }
    }
}
```

## Notes

- **Thread Safety**: Implementations of this interface should ideally treat properties as immutable after initialization. If properties must be mutable, implement appropriate locking mechanisms to ensure thread-safe access to members like `ResolvedDependencies` or error-related properties.
- **Required Members**: The `required` keyword ensures that consumers must provide values for these properties during object initialization. Ensure all `required` members are properly set in constructors or via object initializers; failure to do so will result in compilation errors.
- **Nullable Types**: Properties marked as nullable (`string?`) should be explicitly checked for `null` before access to prevent `NullReferenceException` at runtime.
- **EventType**: While defined here for the interface contract, this property is intended to distinguish specific event subtypes. Ensure implementations provide meaningful, non-empty strings for this property.
