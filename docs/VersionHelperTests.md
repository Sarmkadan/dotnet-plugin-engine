# VersionHelperTests

Unit test class for `VersionHelper`, providing test coverage for version parsing, comparison, constraint validation, and semantic version utilities.

## API

### `public VersionHelperTests`
Constructor for the test fixture. Initializes the test context and dependencies required for version-related tests.

### `public void ParseVersion_WithVPrefix_StripsPrefixAndParsesCorrectly`
Tests that a version string prefixed with `v` (e.g., `v1.2.3`) is correctly parsed by removing the prefix and extracting the semantic version components.

### `public void ParseVersion_WithEmptyOrWhitespace_ReturnsNull`
Verifies that passing an empty or whitespace-only string to `ParseVersion` results in a `null` return value, indicating invalid input.

### `public void ParseVersion_WithInvalidString_LogsWarningAndReturnsNull`
Ensures that invalid version strings trigger a warning-level log entry and result in a `null` return value from `ParseVersion`.

### `public void ParseVersion_WithPrereleaseTag_ParsesCoreVersionNumbers`
Confirms that version strings including prerelease tags (e.g., `1.2.3-alpha`) are parsed correctly, preserving the core semantic version numbers.

### `public void CompareVersions_WhenFirstVersionIsGreater_ReturnsPositive`
Validates that `CompareVersions` returns a positive integer when the first version is greater than the second, indicating correct ordering.

### `public void CompareVersions_WhenVersionsAreEqual_ReturnsZero`
Checks that `CompareVersions` returns zero when both versions are identical, confirming equality comparison behavior.

### `public void SatisfiesConstraint_GreaterThanOrEqual_ReturnsTrueWhenVersionMeetsMinimum`
Tests that `SatisfiesConstraint` returns `true` when the input version meets or exceeds the specified minimum version under a `>=` constraint.

### `public void SatisfiesConstraint_GreaterThanOrEqual_ReturnsFalseWhenBelowMinimum`
Ensures that `SatisfiesConstraint` returns `false` when the input version is below the specified minimum version under a `>=` constraint.

### `public void SatisfiesConstraint_CaretOperator_RejectsDifferentMajorVersion`
Verifies that the caret (`^`) operator correctly rejects version strings with a different major version number, enforcing compatibility rules.

### `public void SatisfiesConstraint_CaretOperator_AcceptsSameMajorHigherMinor`
Confirms that the caret (`^`) operator accepts versions with the same major version but a higher minor version, allowing compatible updates.

### `public void SatisfiesConstraint_TildeOperator_RejectsDifferentMinorVersion`
Ensures that the tilde (`~`) operator rejects version strings with a different minor version number, enforcing patch-level compatibility.

### `public void GetLatestVersion_FromMixedVersionList_ReturnsHighestVersion`
Tests that `GetLatestVersion` correctly identifies and returns the highest version from a mixed list of valid semantic versions.

### `public void GetVersionInfo_WithAlphaPrereleaseTag_SetsIsPrereleaseTrue`
Validates that `GetVersionInfo` correctly identifies prerelease versions (e.g., `1.0.0-alpha`) and sets the `IsPrerelease` flag to `true`.

### `public void IsValidSemanticVersion_WithProperVersionString_ReturnsTrue`
Checks that `IsValidSemanticVersion` returns `true` for properly formatted semantic version strings (e.g., `1.2.3`).

### `public void IsValidSemanticVersion_WithNonVersionString_ReturnsFalse`
Ensures that `IsValidSemanticVersion` returns `false` for strings that do not conform to semantic versioning standards.

## Usage
