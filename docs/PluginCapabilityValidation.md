# PluginCapabilityValidation

Provides validation helpers for `PluginCapability` instances.

## Public Members

| Member | Description |
|--------|-------------|
| `Validate(this PluginCapability value)` | Validates a `PluginCapability` instance and returns a list of human-readable validation problems. |
| `IsValid(this PluginCapability value)` | Determines whether a `PluginCapability` instance is valid. |
| `EnsureValid(this PluginCapability value)` | Ensures that a `PluginCapability` instance is valid, throwing an `ArgumentException` if it is not. |

## Validation Rules

The `Validate` method checks the following fields:

| Field | Validation Rule | Error Message |
|-------|----------------|---------------|
| `Id` | Must not be `Guid.Empty` | `Id cannot be empty (Guid.Empty).` |
| `PluginId` | Must not be `Guid.Empty` | `PluginId cannot be empty (Guid.Empty).` |
| `Name` | Must not be null or whitespace | `Name cannot be null or whitespace.` |
| `Version` | Must not be null or whitespace; must be a valid version string (parsable by `Version.TryParse`) | `Version cannot be null or whitespace.`<br/>`Version must be a valid version string (e.g., 1.0.0).` |
| `InterfaceTypeName` | Must not be null or whitespace | `InterfaceTypeName cannot be null or whitespace.` |
| `ImplementationTypeName` | Must not be null or whitespace | `ImplementationTypeName cannot be null or whitespace.` |
| `Description` | Must not be null or whitespace | `Description cannot be null or whitespace.` |
| `Tags` | If not null or whitespace, must contain at least one non-empty tag after splitting by `','` and trimming | `Tags must contain at least one non-empty tag.` |
| `CreatedAt` | Must not be `default(DateTime)`; must be on or after `2000-01-01` | `CreatedAt cannot be default DateTime.`<br/>`CreatedAt must be a reasonable date.` |
| `ModifiedAt` | Must not be `default(DateTime)`; must be on or after `2000-01-01`; must be on or after `CreatedAt` | `ModifiedAt cannot be default DateTime.`<br/>`ModifiedAt must be a reasonable date.`<br/>`ModifiedAt cannot be earlier than CreatedAt.` |

## Returned Problems Shape

The `Validate` method returns an `IReadOnlyList<string>` where each string is a validation error message. If the instance is valid, the list is empty.

## Example

```csharp
using PluginEngine.Domain.Entities;

// Create an invalid capability (empty Id and Name)
var capability = new PluginCapability
{
    Id = Guid.Empty,
    PluginId = Guid.NewGuid(),
    Name = "", // invalid
    Version = "1.0.0",
    InterfaceTypeName = "IMyInterface",
    ImplementationTypeName = "MyImplementation",
    Description = "A test capability",
    Tags = "tag1, tag2",
    CreatedAt = DateTime.Now,
    ModifiedAt = DateTime.Now
};

var problems = capability.Validate();
// problems contains:
//   "Id cannot be empty (Guid.Empty)."
//   "Name cannot be null or whitespace."

if (!capability.IsValid())
{
    capability.EnsureValid(); // throws ArgumentException with the combined problems
}
```