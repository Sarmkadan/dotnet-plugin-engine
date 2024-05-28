# DependencyResolutionServiceTests

`DependencyResolutionServiceTests` is the unit test suite for the `DependencyResolutionService` class within the `dotnet-plugin-engine` project. It validates the correctness of dependency resolution, validation, circular dependency detection, and cache management logic, ensuring that the service behaves as expected under normal operation, edge cases, and invalid input conditions.

## API

### public DependencyResolutionServiceTests

Default constructor for the test class. The test framework instantiates this class to discover and execute the test methods defined within it. No parameters or special initialization logic are exposed.

### public void Constructor_WithNullLoaderService_ThrowsArgumentNullException

Verifies that the `DependencyResolutionService` constructor throws an `ArgumentNullException` when a null loader service is provided. This test ensures the service enforces its precondition on the loader dependency.

### public async Task ResolveDependenciesAsync_WithNullPlugin_ThrowsArgumentNullException

Ensures that calling `ResolveDependenciesAsync` with a null plugin argument throws an `ArgumentNullException`. This guards against null-reference misuse in the public API.

### public async Task ResolveDependenciesAsync_WithPluginHavingNoDependencies_ReturnsEmptyList

Validates that when a plugin declares no dependencies, the resolution method returns an empty collection rather than null or a collection with spurious entries.

### public async Task ResolveDependenciesAsync_WithSingleDependency_ResolvesDependency

Confirms that a plugin with exactly one dependency causes the service to resolve and return a collection containing that single dependency.

### public async Task ResolveDependenciesAsync_WithMultipleDependencies_ResolvesAll

Verifies that a plugin with several declared dependencies results in a collection containing all of them, with none omitted or duplicated.

### public async Task ResolveDependenciesAsync_WithCachedResult_ReturnsCache

Demonstrates that subsequent calls to resolve dependencies for the same plugin return the previously cached result without re-invoking the underlying loader, thereby validating the caching layer.

### public async Task ValidateDependenciesAsync_WithNullPlugin_ThrowsArgumentNullException

Ensures that `ValidateDependenciesAsync` throws an `ArgumentNullException` when invoked with a null plugin, enforcing input validation.

### public async Task ValidateDependenciesAsync_WithAllRequiredDependenciesSatisfied_ReturnsTrue

Confirms that validation returns `true` when every required dependency of the plugin is present and satisfied in the environment.

### public async Task ValidateDependenciesAsync_WithMissingRequiredDependency_ReturnsFalse

Verifies that validation returns `false` when at least one required dependency is missing, correctly reflecting an unsatisfied dependency graph.

### public async Task ValidateDependenciesAsync_WithMissingOptionalDependency_ReturnsTrue

Ensures that the absence of an optional dependency does not cause validation to fail; the method returns `true` because optional dependencies are not mandatory.

### public async Task HasCircularDependenciesAsync_WithNullPlugin_ThrowsArgumentNullException

Validates that `HasCircularDependenciesAsync` throws an `ArgumentNullException` when a null plugin is supplied, protecting against null input.

### public async Task HasCircularDependenciesAsync_WithNoCircularDependencies_ReturnsFalse

Confirms that a plugin graph free of cycles causes the method to return `false`, indicating no circular dependencies were detected.

### public async Task ResolveSingleDependencyAsync_WithValidDependency_ReturnsDependencyPlugin

Tests that resolving a single, valid dependency by its identifier returns the corresponding plugin object.

### public async Task ResolveSingleDependencyAsync_WithMissingDependency_ReturnsNull

Verifies that attempting to resolve a dependency that does not exist yields a null result rather than throwing an exception or returning a stub.

### public async Task GetDependentsAsync_ReturnsPluginsThatDependOnGivenPlugin

Ensures that querying for dependents of a specific plugin returns the set of plugins that declare a dependency on it, correctly reflecting reverse-dependency relationships.

### public async Task ClearDependencyCacheAsync_ClearsTheCache

Validates that after clearing the dependency cache, a subsequent resolution call re-fetches dependencies from the loader instead of serving stale cached data.

## Usage

```csharp
// Example 1: Running all tests in a CI pipeline
// Typically executed via a test runner such as `dotnet test`.
// The test class is discovered automatically; no manual instantiation is required.
// This example shows how a build script might invoke the tests.

// In a CI script (e.g., GitHub Actions, Azure DevOps):
// dotnet test ./tests/DotnetPluginEngine.Tests.csproj --filter "FullyQualifiedName~DependencyResolutionServiceTests"
```

```csharp
// Example 2: Executing a specific test programmatically using xUnit (if applicable)
// While normally run via a test runner, tests can be instantiated and invoked directly
// for debugging or custom test harness scenarios.

var testInstance = new DependencyResolutionServiceTests();

// Run a single test method
await testInstance.ResolveDependenciesAsync_WithSingleDependency_ResolvesDependency();

// Run another test to verify cache behavior
await testInstance.ResolveDependenciesAsync_WithCachedResult_ReturnsCache();
```

## Notes

- **Null input handling**: Multiple tests explicitly verify that methods throw `ArgumentNullException` when a null plugin is passed. Consumers of `DependencyResolutionService` must perform null checks before invoking these methods to avoid runtime exceptions.
- **Cache consistency**: The test `ClearDependencyCacheAsync_ClearsTheCache` implies that the cache is mutable and shared across calls. In multi-threaded scenarios, callers should ensure that cache clearing and dependency resolution are synchronized appropriately to avoid race conditions where one thread clears the cache while another reads from it.
- **Optional vs. required dependencies**: Validation logic distinguishes between required and optional dependencies. Missing optional dependencies do not cause validation failure, as confirmed by `ValidateDependenciesAsync_WithMissingOptionalDependency_ReturnsTrue`. Systems integrating this service should model their dependency metadata accordingly.
- **Circular dependency detection**: The test `HasCircularDependenciesAsync_WithNoCircularDependencies_ReturnsFalse` suggests that the service performs graph traversal. For large plugin graphs, callers should be aware of potential performance implications and consider caching the result if the graph is static.
- **Return values for missing dependencies**: `ResolveSingleDependencyAsync_WithMissingDependency_ReturnsNull` indicates that null is a valid return value and must be handled by callers to avoid null-reference errors downstream.
- **Test isolation**: Each test method is asynchronous and self-contained. They do not share mutable state beyond what the test framework manages, ensuring deterministic outcomes when run individually or as a suite.
