# PluginValidationResult

Represents the result of a plugin validation operation, containing validation status, associated plugin identifiers, and any validation errors encountered.

## API

### `public required Guid PluginId { get; set; }`

The unique identifier of the plugin being validated.

### `public required string PluginName { get; set; }`

The name of the plugin being validated.

### `public required bool IsValid { get; set; }`

Indicates whether the plugin passed all validation checks (`true`) or failed at least one check (`false`).

### `public required List<string> Errors { get; set; }`

A list of error messages describing validation failures. Empty when `IsValid` is `true`.

### `public string GetErrorSummary()`

Returns a formatted string containing all error messages, each on a new line prefixed with two spaces.

- **Returns**
  - A string where each error is separated by a newline and two spaces (`\n  `). Returns an empty string if there are no errors.

---

## Validation Rules

The `PluginValidator.Validate` method applies the following validation rules to a `Plugin` object:

### Name Validation
- **Cannot be empty**: Returns error "Plugin name cannot be empty" if `Name` is null, empty, or whitespace.
- **Maximum length**: Cannot exceed `MaxNameLength` (100 characters). Error: `"Plugin name exceeds maximum length of {MaxNameLength} characters: {actual length}"`.
- **Reserved prefixes**: Cannot start with "System." or "Microsoft.". Error: `"Plugin name cannot start with reserved prefixes (System., Microsoft.)"`.
- **First character**: Must start with a letter or digit. Error: `"Plugin name must start with a letter or digit"`.
- **Allowed characters**: Can only contain letters, digits, hyphens (`-`), underscores (`_`), and periods (`.`). Error: `"Plugin name contains invalid characters"`.

### Version Validation
- **Cannot be empty**: Returns error "Plugin version cannot be empty" if `Version` is null, empty, or whitespace.
- **Semantic version format**: Must be a valid semantic version (validated by `VersionHelper.IsValidSemanticVersion`). Error: `"Invalid semantic version format: {version}"`.
- **Non-zero version**: Cannot be exactly "0.0.0". Error: `"Plugin version cannot be 0.0.0"`.

### Metadata Validation (if `Metadata` is not null)
- **Description cannot be empty**: Returns error "Plugin description cannot be empty" if `Metadata.Description` is null, empty, or whitespace.
- **Author cannot be empty**: Returns error "Plugin author cannot be empty" if `Metadata.Author` is null, empty, or whitespace.
- **Author maximum length**: Cannot exceed `MaxAuthorLength` (100 characters). Error: `"Plugin author name exceeds maximum length"`.

### Dependency Validation
- **Maximum dependency count**: Cannot exceed `MaxDependencyCount` (50 dependencies). Error: `"Plugin has too many dependencies: {count} (max {MaxDependencyCount})"`.
- **Per-dependency validation** (for each dependency in `Dependencies`):
  - **Minimum version cannot be empty**: Error: `"Dependency {DependencyPluginId} has invalid minimum version constraint"`.
  - **Minimum version format**: Must be a valid semantic version. Error: `"Dependency {DependencyPluginId} has invalid minimum version: {version}"`.
  - **Maximum version format** (if specified): Must be a valid semantic version. Error: `"Dependency {DependencyPluginId} has invalid maximum version: {version}"`.
  - **Version range validity**: If both minimum and maximum versions are specified, maximum cannot be less than minimum. Error: `"Dependency {DependencyPluginId} maximum version {max} cannot be less than minimum version {min}"`.
  - **No duplicate dependencies**: Each `DependencyPluginId` must be unique within the dependencies list. Error: `"Duplicate dependency: {DependencyPluginId}"`.

## Constants
- `MaxNameLength`: 100
- `MaxAuthorLength`: 100
- `MaxDependencyCount`: 50