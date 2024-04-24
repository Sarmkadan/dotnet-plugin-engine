# IIntegrationClient

The `IIntegrationClient` interface defines the contract for integration clients within the `dotnet-plugin-engine` framework, providing necessary identification, event-tracking, and metadata capabilities for plugin-based architectures.

## API

### PluginId
`public required Guid PluginId { get; set; }`
The unique identifier associated with the parent plugin.

### PluginName
`public required string PluginName { get; set; }`
The human-readable name of the parent plugin.

### EventType
`public required string EventType { get; set; }`
The specific type of event that the integration client is processing or reporting.

### OccurredAtUtc
`public required DateTime OccurredAtUtc { get; set; }`
The precise timestamp, in Coordinated Universal Time (UTC), indicating when the event occurred.

### Metadata
`public Dictionary<string, object> Metadata { get; set; }`
A collection of custom key-value pairs used to attach additional, arbitrary information to the integration client. Initializing as an empty dictionary is recommended if no metadata is required.

### Id
`public required Guid Id { get; set; }`
The unique identifier for this specific integration client instance.

### Name
`public required string Name { get; set; }`
The descriptive name of this integration client.

### Version
`public required string Version { get; set; }`
The semantic version string of this integration client instance.

### Description
`public string? Description { get; set; }`
An optional, descriptive summary of the integration client's functionality.

### Author
`public string? Author { get; set; }`
The optional name or identifier of the author or maintainer of this integration client.

### DownloadUrl
`public string? DownloadUrl { get; set; }`
An optional URL pointing to the download location or source repository of the integration client.

## Usage

### Example 1: Instantiating a Client
```csharp
var client = new MyIntegrationClient
{
    PluginId = Guid.NewGuid(),
    PluginName = "AnalyticsPlugin",
    EventType = "DataExport",
    OccurredAtUtc = DateTime.UtcNow,
    Id = Guid.NewGuid(),
    Name = "MainExporter",
    Version = "1.0.0",
    Metadata = new Dictionary<string, object> { { "Environment", "Production" } }
};
```

### Example 2: Accessing Metadata in a Handler
```csharp
public void HandleClient(IIntegrationClient client)
{
    if (client.Metadata.TryGetValue("Environment", out var env))
    {
        Console.WriteLine($"Processing event {client.EventType} for environment: {env}");
    }
}
```

## Notes

- **Thread Safety**: Implementations of this interface are not inherently thread-safe. If multiple threads access or modify the `Metadata` dictionary or the properties of an `IIntegrationClient` instance, appropriate external synchronization mechanisms should be employed.
- **Required Members**: Properties marked with the `required` keyword must be initialized during object construction (e.g., using object initializers). Failure to provide values for these members will result in a compiler error.
- **Nullable Types**: Properties marked with `?` (e.g., `Description`, `Author`, `DownloadUrl`) allow null values. Consumers of these properties should perform appropriate null checks before usage.
