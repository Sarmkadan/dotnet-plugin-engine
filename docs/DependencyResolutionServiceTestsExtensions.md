# DependencyResolutionServiceTestsExtensions

Provides a set of static helper methods and extension methods designed to streamline the creation of test fixtures and assertions within unit tests for the dependency resolution service. These utilities simplify the construction of plugins, dependencies, and mock service configurations, while offering fluent-style validation methods to verify resolution outcomes, dependency graphs, and plugin validity.

## API

### CreateTestPlugin

```csharp
public static Plugin CreateTestPlugin(...)
```

Creates a fully configured `Plugin` instance suitable for testing dependency resolution scenarios. The exact signature parameters are determined by the overloads present in the source, but the method encapsulates the boilerplate of assigning identifiers, manifests, and dependency declarations.

- **Returns:** A new `Plugin` object ready for use in test arrangements.
- **Exceptions:** May throw `ArgumentNullException` if required arguments are omitted (depending on overload).

### CreateTestDependency

```csharp
public static PluginDependency CreateTestDependency(...)
```

Constructs a `PluginDependency` object representing a single dependency edge between plugins. Used to populate the dependency collections of test plugins.

- **Returns:** A new `PluginDependency` instance.
- **Exceptions:** Specific exceptions depend on the overload; typically none for valid inputs.

### SetupLoaderService

```csharp
public static void SetupLoaderService(...)
```

Configures a mock or stub loader service with a known set of plugins and their metadata. This method prepares the underlying service so that calls to resolve or enumerate plugins return predictable test data.

- **Exceptions:** May throw if the service mock cannot be configured due to invalid state.

### SetupLoaderServiceForPlugin

```csharp
public static void SetupLoaderServiceForPlugin(...)
```

A focused variant of `SetupLoaderService` that registers a single plugin and its dependency chain into the loader service. Useful for isolating tests to one plugin under test.

- **Exceptions:** May throw if the target plugin is null or the service rejects the registration.

### ShouldHaveDependencyCount

```csharp
public static void ShouldHaveDependencyCount(...)
```

Asserts that a given plugin or dependency resolution result contains an exact number of dependencies. Typically used in fluent assertion chains.

- **Exceptions:** Throws an assertion exception (e.g., `AssertionException` or equivalent) if the actual count differs from the expected count.

### ShouldHaveDependencyOn

```csharp
public static void ShouldHaveDependencyOn(...)
public static void ShouldHaveDependencyOn(...)
```

Two overloads that verify a plugin declares a dependency on a specific target plugin, either by identifier or by a `PluginDependency` reference. Both perform the same logical check but accept different argument types.

- **Exceptions:** Throws an assertion exception if the dependency is not found.

### ShouldContainPluginIds

```csharp
public static void ShouldContainPluginIds(...)
```

Validates that a collection of plugins or a resolution result includes all of the specified plugin identifiers. Ensures that expected plugins are present in the output set.

- **Exceptions:** Throws an assertion exception if any expected identifier is missing.

### ShouldBeValid

```csharp
public static void ShouldBeValid(...)
```

Asserts that a dependency resolution result or plugin configuration is in a valid state—no circular dependencies, missing references, or constraint violations.

- **Exceptions:** Throws an assertion exception if the subject is invalid.

### ShouldBeInvalid

```csharp
public static void ShouldBeInvalid(...)
```

The inverse of `ShouldBeValid`; asserts that a resolution result or plugin configuration is expected to be invalid, confirming that error detection works correctly.

- **Exceptions:** Throws an assertion exception if the subject unexpectedly passes validation.

### CreatePluginWithDependencies

```csharp
public static Plugin CreatePluginWithDependencies(this DependencyResolutionServiceTests tests, Guid pluginId, params ...)
```

An extension method on `DependencyResolutionServiceTests` that creates a plugin with a given identifier and a variable number of dependencies in a single call. The `params` parameter accepts dependency specifications, streamlining test arrangement.

- **Parameters:**
  - `tests`: The test fixture instance providing context (e.g., mocks, defaults).
  - `pluginId`: The `Guid` to assign to the new plugin.
  - `params`: One or more dependency descriptors (exact type determined by overload).
- **Returns:** A `Plugin` with the specified identifier and dependencies already attached.
- **Exceptions:** May throw `ArgumentNullException` if `tests` is null, or `ArgumentException` if `pluginId` is empty.

### ShouldResolveTo

```csharp
public static void ShouldResolveTo(...)
```

Asserts that resolving dependencies for a given plugin produces an exact, ordered set of plugins. Validates both the contents and the sequence of the resolution output.

- **Exceptions:** Throws an assertion exception if the resolved set does not match the expected collection in count, identity, or order.

## Usage

### Example 1: Validating a Simple Linear Dependency Chain

```csharp
[Test]
public void LinearDependencyChain_ResolvesInCorrectOrder()
{
    // Arrange
    var rootPlugin = DependencyResolutionServiceTestsExtensions.CreateTestPlugin(
        id: Guid.NewGuid(),
        name: "Root"
    );
    var middlePlugin = DependencyResolutionServiceTestsExtensions.CreateTestPlugin(
        id: Guid.NewGuid(),
        name: "Middle"
    );
    var leafPlugin = DependencyResolutionServiceTestsExtensions.CreateTestPlugin(
        id: Guid.NewGuid(),
        name: "Leaf"
    );

    DependencyResolutionServiceTestsExtensions.SetupLoaderService(
        loaderServiceMock,
        new[] { rootPlugin, middlePlugin, leafPlugin }
    );

    rootPlugin.Dependencies.Add(
        DependencyResolutionServiceTestsExtensions.CreateTestDependency(middlePlugin.Id)
    );
    middlePlugin.Dependencies.Add(
        DependencyResolutionServiceTestsExtensions.CreateTestDependency(leafPlugin.Id)
    );

    // Act
    var resolved = resolutionService.Resolve(rootPlugin);

    // Assert
    DependencyResolutionServiceTestsExtensions.ShouldResolveTo(resolved, new[]
    {
        leafPlugin.Id,
        middlePlugin.Id,
        rootPlugin.Id
    });
    DependencyResolutionServiceTestsExtensions.ShouldBeValid(resolved);
}
```

### Example 2: Using Extension Methods Within a Test Fixture

```csharp
[TestFixture]
public class DependencyResolutionServiceTests
{
    [Test]
    public void PluginWithMultipleDependencies_ContainsAll()
    {
        // Arrange
        var depA = Guid.NewGuid();
        var depB = Guid.NewGuid();

        var plugin = this.CreatePluginWithDependencies(
            Guid.NewGuid(),
            depA,
            depB
        );

        SetupLoaderServiceForPlugin(loaderServiceMock, plugin);

        // Assert
        plugin.ShouldHaveDependencyCount(2);
        plugin.ShouldHaveDependencyOn(depA);
        plugin.ShouldHaveDependencyOn(depB);
        plugin.ShouldBeValid();
    }
}
```

## Notes

- **Assertion failures:** All `Should*` methods are designed to throw assertion exceptions rather than returning boolean values. Tests should rely on the exception being caught by the test runner to signal failure; do not wrap these calls in conditional checks.
- **Extension method context:** `CreatePluginWithDependencies` extends the test fixture class itself, meaning it has access to any shared mocks, default plugin templates, or configuration held by the fixture instance. Ensure the fixture is fully initialized before calling it.
- **Overload resolution:** Several methods (`ShouldHaveDependencyOn`, `CreateTestPlugin`, `CreateTestDependency`) have multiple overloads. The compiler selects the appropriate overload based on argument types. Passing ambiguous types (e.g., a `Guid` where both a `Guid` and a `PluginDependency` overload exist) may require explicit casting.
- **Thread safety:** These methods are not designed for concurrent use. They operate on test-specific state and mock configurations without synchronization primitives. All calls should occur sequentially within a single test thread.
- **State leakage:** `SetupLoaderService` and `SetupLoaderServiceForPlugin` mutate shared mock objects. Tests that reuse the same mock instance across multiple test methods must reset or reinitialize the mock between tests to avoid cross-test contamination.
- **Validation scope:** `ShouldBeValid` and `ShouldBeInvalid` check structural integrity (e.g., no cycles, all dependencies resolvable) but do not verify semantic correctness such as version compatibility unless the underlying service implementation includes such checks.
