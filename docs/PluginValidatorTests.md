# PluginValidatorTests

Unit test class for validating plugin definitions using the `PluginValidator` class. Contains test cases that verify correct behavior for various plugin property validations including name, version, metadata, and dependencies.

## API

### `PluginValidatorTests`

Public test class containing validation test cases for plugin definitions.

### `void Validate_WithValidPlugin_ReturnsValidResult()`

Tests that a plugin with valid properties returns a valid validation result.

### `void Validate_WithEmptyName_ReturnsInvalidWithError()`

Tests that a plugin with an empty name returns an invalid validation result with an appropriate error message.

### `void Validate_WithWhitespaceName_ReturnsInvalidWithError()`

Tests that a plugin with a whitespace-only name returns an invalid validation result with an appropriate error message.

### `void Validate_WithNameStartingWithSystemPrefix_ReturnsInvalidWithError()`

Tests that a plugin with a name starting with the "System." prefix returns an invalid validation result with an appropriate error message.

### `void Validate_WithNameStartingWithMicrosoftPrefix_ReturnsInvalidWithError()`

Tests that a plugin with a name starting with the "Microsoft." prefix returns an invalid validation result with an appropriate error message.

### `void Validate_WithNameExceedingMaxLength_ReturnsInvalidWithError()`

Tests that a plugin with a name exceeding the maximum allowed length returns an invalid validation result with an appropriate error message.

### `void Validate_WithNameStartingWithSpecialCharacter_ReturnsInvalidWithError()`

Tests that a plugin with a name starting with a special character returns an invalid validation result with an appropriate error message.

### `void Validate_WithNameContainingInvalidCharacters_ReturnsInvalidWithError()`

Tests that a plugin with a name containing invalid characters returns an invalid validation result with an appropriate error message.

### `void Validate_WithValidNames_ReturnsValidResult()`

Tests that multiple valid plugin names all return valid validation results.

### `void Validate_WithEmptyVersion_ReturnsInvalidWithError()`

Tests that a plugin with an empty version returns an invalid validation result with an appropriate error message.

### `void Validate_WithInvalidVersionFormat_ReturnsInvalidWithError()`

Tests that a plugin with an invalid version format returns an invalid validation result with an appropriate error message.

### `void Validate_WithValidVersions_ReturnsValidResult()`

Tests that multiple valid plugin versions all return valid validation results.

### `void Validate_WithValidMetadata_ReturnsValidResult()`

Tests that a plugin with valid metadata returns a valid validation result.

### `void Validate_WithEmptyMetadataAuthor_ReturnsInvalidWithError()`

Tests that a plugin with an empty metadata author returns an invalid validation result with an appropriate error message.

### `void Validate_WithValidDependencies_ReturnsValidResult()`

Tests that a plugin with valid dependencies returns a valid validation result.

### `void Validate_WithInvalidDependencyMinimumVersion_ReturnsInvalidWithError()`

Tests that a plugin with an invalid dependency minimum version returns an invalid validation result with an appropriate error message.

### `void Validate_WithDependencyMaximumVersionLowerThanMinimum_ReturnsInvalidWithError()`

Tests that a plugin with a dependency maximum version lower than the minimum version returns an invalid validation result with an appropriate error message.

### `void Validate_MultipleErrors_ReturnsAllErrorMessages()`

Tests that a plugin with multiple validation errors returns all error messages in the result.

### `void Validate_IncludesPluginIdAndNameInResult()`

Tests that the validation result includes the plugin ID and name in the output.
