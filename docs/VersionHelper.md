# VersionHelper

A utility class for parsing, comparing, and validating semantic version strings in .NET applications. It provides functionality to handle version constraints, check prerelease status, and determine version compatibility.

## API

### `public VersionHelper`

The primary entry point for version parsing and comparison operations. Initializes a new instance with the original version string.

### `public Version? ParseVersion(string versionString)`

Parses a version string into a `Version` object.

- **Parameters**
  - `versionString`: The semantic version string to parse (e.g., `"1.2.3"`, `"2.0.0-alpha"`).
- **Return Value**
  - A `Version` object if parsing succeeds; `null` if the input is invalid.
- **Exceptions**
  - Throws `ArgumentNullException` if `versionString` is `null`.
  - Throws `FormatException` if the version string is malformed.

### `public int CompareVersions(Version a, Version b)`

Compares two `Version` instances and returns their relative order.

- **Parameters**
  - `a`: The first version to compare.
  - `b`: The second version to compare.
- **Return Value**
  - A signed integer indicating the relative order:
    - Less than zero if `a` is older than `b`.
    - Zero if `a` and `b` are equivalent.
    - Greater than zero if `a` is newer than `b`.
- **Exceptions**
  - Throws `ArgumentNullException` if either `a` or `b` is `null`.

### `public bool SatisfiesConstraint(Version version, string constraint)`

Determines whether a given version satisfies a version constraint expression.

- **Parameters**
  - `version`: The version to check.
  - `constraint`: The constraint expression (e.g., `">=1.0.0 <2.0.0"`).
- **Return Value**
  - `true` if the version satisfies the constraint; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `version` or `constraint` is `null`.
  - Throws `FormatException` if the constraint expression is invalid.

### `public string? GetLatestVersion(IEnumerable<Version> versions)`

Retrieves the latest version from a collection of versions.

- **Parameters**
  - `versions`: An enumerable collection of `Version` objects.
- **Return Value**
  - The latest `Version` as a string if the collection is non-empty; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `versions` is `null`.

### `public ParsedVersionInfo GetVersionInfo(string versionString)`

Extracts detailed semantic version information from a version string.

- **Parameters**
  - `versionString`: The semantic version string to analyze.
- **Return Value**
  - A `ParsedVersionInfo` object containing parsed components (e.g., major, minor, patch, prerelease label).
- **Exceptions**
  - Throws `ArgumentNullException` if `versionString` is `null`.
  - Throws `FormatException` if the version string is invalid.

### `public bool IsValidSemanticVersion(string versionString)`

Validates whether a string conforms to semantic versioning rules.

- **Parameters**
  - `versionString`: The version string to validate.
- **Return Value**
  - `true` if the string is a valid semantic version; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `versionString` is `null`.

### `public required string Original { get; init; }`

The original version string as provided during initialization.

### `public int Major { get; }`

The major component of the semantic version.

### `public int Minor { get; }`

The minor component of the semantic version.

### `public int Patch { get; }`

The patch component of the semantic version.

### `public bool IsPrerelease { get; }`

Indicates whether the version is a prerelease (e.g., `"1.0.0-alpha"`).

### `public bool IsStable { get; }`

Indicates whether the version is stable (i.e., not a prerelease).

## Usage

### Example 1: Parsing and Comparing Versions
