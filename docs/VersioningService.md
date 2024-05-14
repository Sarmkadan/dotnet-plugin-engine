# VersioningService

Provides version validation, comparison, parsing, and increment operations for semantic versioning in .NET plugin systems.

## API

### `bool ValidateVersion(string version)`

Determines whether the given string represents a valid semantic version.

- **Parameters**: `version` – The version string to validate (e.g., `"1.2.3"`).
- **Returns**: `true` if the version is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `bool IsSatisfiedBy(string versionConstraint, string version)`

Checks whether the specified version satisfies the given version constraint (e.g., `"^1.0.0"`).

- **Parameters**:
  - `versionConstraint` – A semantic version constraint (e.g., `"~1.2.0"`, `"^2.0.0"`).
  - `version` – The version to test against the constraint.
- **Returns**: `true` if the version satisfies the constraint; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `int CompareVersions(string versionA, string versionB)`

Compares two semantic versions and returns an integer indicating their relative order.

- **Parameters**:
  - `versionA` – The first version to compare.
  - `versionB` – The second version to compare.
- **Returns**:
  - A positive integer if `versionA` is greater than `versionB`.
  - Zero if they are equal.
  - A negative integer if `versionA` is less than `versionB`.
- **Throws**: Does not throw exceptions.

### `SemanticVersion ParseVersion(string version)`

Parses a semantic version string into a structured `SemanticVersion` object.

- **Parameters**: `version` – The semantic version string to parse (e.g., `"2.5.1-alpha.1"`).
- **Returns**: A `SemanticVersion` object representing the parsed version.
- **Throws**:
  - `ArgumentException` if the input string is not a valid semantic version.
  - `ArgumentNullException` if `version` is `null`.

### `Task<IEnumerable<Domain.Entities.VersionInfo>> GetVersionHistoryAsync(string pluginName)`

Retrieves the version history for a specified plugin.

- **Parameters**: `pluginName` – The name of the plugin whose version history is requested.
- **Returns**: A task that resolves to an enumerable of `VersionInfo` objects representing the version history.
- **Throws**:
  - `ArgumentException` if `pluginName` is null or empty.
  - `InvalidOperationException` if the version history cannot be retrieved (e.g., plugin not found).

### `string IncrementVersion(string version, string incrementType)`

Increments a semantic version based on the specified increment type (e.g., `"major"`, `"minor"`, `"patch"`).

- **Parameters**:
  - `version` – The version string to increment.
  - `incrementType` – The type of increment to apply (`"major"`, `"minor"`, `"patch"`, `"pre"`, `"build"`).
- **Returns**: The incremented version string.
- **Throws**:
  - `ArgumentException` if `version` is not a valid semantic version.
  - `ArgumentException` if `incrementType` is invalid.
  - `ArgumentNullException` if either parameter is `null`.

### `bool AreCompatible(string versionA, string versionB)`

Determines whether two versions are compatible based on semantic versioning rules.

- **Parameters**:
  - `versionA` – The first version to compare.
  - `versionB` – The second version to compare.
- **Returns**: `true` if the versions are compatible; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `Task<Domain.Entities.VersionInfo?> GetLatestVersionAsync(string pluginName)`

Retrieves the latest version information for a specified plugin.

- **Parameters**: `pluginName` – The name of the plugin whose latest version is requested.
- **Returns**: A task that resolves to the latest `VersionInfo` or `null` if not found.
- **Throws**:
  - `ArgumentException` if `pluginName` is null or empty.
  - `InvalidOperationException` if the latest version cannot be retrieved.

## Usage

### Example 1: Validating and Comparing Versions
