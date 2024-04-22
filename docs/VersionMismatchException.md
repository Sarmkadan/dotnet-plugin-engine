# VersionMismatchException

The `VersionMismatchException` is thrown by the `dotnet-plugin-engine` when a loaded component's version does not meet the specified version requirements. This exception captures diagnostic information about the expected and actual versions, along with the component type and name, to facilitate troubleshooting and ensure system integrity during component initialization.

## API

### Properties

- `public string ExpectedVersion { get; set; }`
  The version string that the engine expected the component to have.

- `public string ActualVersion { get; set; }`
  The actual version string reported by the loaded component.

- `public string ComponentType { get; set; }`
  The type or category of the component that failed the version check.

- `public string ComponentName { get; set; }`
  The name of the component that failed the version check.

### Constructors

- `public VersionMismatchException() : base()`
  Initializes a new instance of the `VersionMismatchException` class.

- `public VersionMismatchException(string message) : base(message)`
  Initializes a new instance of the `VersionMismatchException` class with a specified error message.

- `public VersionMismatchException()`
  Initializes a new instance of the `VersionMismatchException` class.

- `public VersionMismatchException()`
  Initializes a new instance of the `VersionMismatchException` class.

### Methods

- `public override string ToString()`
  Returns a string representation of the exception, including the expected and actual versions, component type, and component name.

## Usage

```csharp
// Example 1: Throwing the exception
public void LoadPlugin(string name, string type, string actualVersion)
{
    string expectedVersion = "2.0.0";
    if (actualVersion != expectedVersion)
    {
        throw new VersionMismatchException($"Version mismatch for plugin {name}")
        {
            ComponentName = name,
            ComponentType = type,
            ExpectedVersion = expectedVersion,
            ActualVersion = actualVersion
        };
    }
}
```

```csharp
// Example 2: Catching and logging the exception
try
{
    // Attempt to load a plugin
}
catch (VersionMismatchException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Component: {ex.ComponentName} ({ex.ComponentType})");
    Console.WriteLine($"Expected: {ex.ExpectedVersion}, Actual: {ex.ActualVersion}");
}
```

## Notes

- **Thread Safety**: This exception is thread-safe, as it serves as a snapshot of the error state. The properties are typically populated during construction or initialization and remain immutable once the exception is thrown.
- **Edge Cases**: If properties are not initialized, they will default to `null` or empty strings. Consumers of this exception should perform null checks on `ExpectedVersion` and `ActualVersion` when logging them to avoid potential exceptions in custom formatters.
