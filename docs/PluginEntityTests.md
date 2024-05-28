# PluginEntityTests
The `PluginEntityTests` class is designed to test the functionality of plugin entities, which are used to manage plugins and their dependencies in the dotnet-plugin-engine project. This class provides a comprehensive set of tests to ensure that plugin entities behave as expected, including tests for adding and removing dependencies, validating plugin entities, and checking version constraints.

## API
The `PluginEntityTests` class contains the following public members:
* `AddDependency_WithNullDependency_ThrowsArgumentNullException`: Tests that adding a null dependency throws an `ArgumentNullException`.
* `AddDependency_WithValidDependency_AppendsToDependencyList`: Tests that adding a valid dependency appends it to the dependency list.
* `AddCapability_WithNullCapability_ThrowsArgumentNullException`: Tests that adding a null capability throws an `ArgumentNullException`.
* `AddCapability_WithDuplicateName_DoesNotAddSecondEntry`: Tests that adding a capability with a duplicate name does not add a second entry.
* `RemoveDependency_WhenDependencyExists_RemovesAndReturnsTrue`: Tests that removing an existing dependency returns `true`.
* `RemoveDependency_WithNonExistentId_ReturnsFalse`: Tests that removing a non-existent dependency returns `false`.
* `IsValid_WithAllRequiredFields_ReturnsTrue`: Tests that a plugin entity with all required fields is valid.
* `IsValid_WithEmptyName_ReturnsFalse`: Tests that a plugin entity with an empty name is not valid.
* `IsValid_WithEmptyGuid_ReturnsFalse`: Tests that a plugin entity with an empty GUID is not valid.
* `GetValidationError_WithMissingAssemblyPath_ReturnsDescriptiveMessage`: Tests that getting the validation error for a missing assembly path returns a descriptive message.
* `PluginDependency_IsSatisfiedBy_VersionAtMinimum_ReturnsTrue`: Tests that a plugin dependency is satisfied by a version at the minimum.
* `PluginDependency_IsSatisfiedBy_VersionAboveMinimum_ReturnsTrue`: Tests that a plugin dependency is satisfied by a version above the minimum.
* `PluginDependency_IsSatisfiedBy_VersionBelowMinimum_ReturnsFalse`: Tests that a plugin dependency is not satisfied by a version below the minimum.
* `PluginDependency_IsSatisfiedBy_VersionExceedsMaximum_ReturnsFalse`: Tests that a plugin dependency is not satisfied by a version that exceeds the maximum.
* `PluginDependency_IsSatisfiedBy_WithInvalidVersionString_ReturnsFalse`: Tests that a plugin dependency is not satisfied by an invalid version string.
* `PluginDependency_GetVersionConstraint_WithoutMaximum_ReturnsGreaterThanOrEqualExpression`: Tests that getting the version constraint without a maximum returns a greater-than-or-equal expression.
* `PluginDependency_GetVersionConstraint_WithMaximum_IncludesBothBounds`: Tests that getting the version constraint with a maximum includes both bounds.
* `PluginCapability_HasTag_WithMatchingTagIgnoringCase_ReturnsTrue`: Tests that a plugin capability has a tag with a matching tag, ignoring case.
* `PluginCapability_HasTag_WithNonExistentTag_ReturnsFalse`: Tests that a plugin capability does not have a non-existent tag.

## Usage
Here are two examples of using the `PluginEntityTests` class:
```csharp
// Example 1: Testing plugin entity validation
PluginEntity pluginEntity = new PluginEntity { Name = "MyPlugin", Guid = Guid.NewGuid() };
PluginEntityTests tests = new PluginEntityTests();
Assert.IsTrue(tests.IsValid_WithAllRequiredFields_ReturnsTrue(pluginEntity));

// Example 2: Testing plugin dependency satisfaction
PluginDependency dependency = new PluginDependency { MinimumVersion = "1.0.0" };
PluginEntityTests tests = new PluginEntityTests();
Assert.IsTrue(tests.PluginDependency_IsSatisfiedBy_VersionAtMinimum_ReturnsTrue(dependency, "1.0.0"));
```

## Notes
The `PluginEntityTests` class is designed to be thread-safe, as it does not maintain any internal state. However, it is still important to note that the tests are designed to be run in isolation, and running multiple tests concurrently may lead to unexpected behavior. Additionally, the tests are sensitive to the specific implementation of the `PluginEntity` and `PluginDependency` classes, and changes to these classes may break the tests. It is also worth noting that the tests do not cover all possible edge cases, and additional testing may be necessary to ensure the correctness of the plugin entity functionality.
