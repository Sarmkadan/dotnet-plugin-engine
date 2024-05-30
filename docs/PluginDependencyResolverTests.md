# PluginDependencyResolverTests
The `PluginDependencyResolverTests` class is designed to test the functionality of the plugin dependency resolver in the `dotnet-plugin-engine` project. It provides a set of test methods to ensure that the resolver correctly handles various dependency scenarios, including linear chains, circular dependencies, and diamond dependencies. These tests cover different aspects of the resolver's behavior, such as determining the install order of plugins, finding conflicts, and building resolution plans.

## API
The `PluginDependencyResolverTests` class contains the following public members:
* `GetInstallOrderAsync_NoDependencies_ReturnsSinglePlugin`: Tests that a plugin with no dependencies is returned as the only item in the install order.
* `GetInstallOrderAsync_LinearChain_DependencyComesFirst`: Verifies that in a linear chain of dependencies, the dependent plugin comes before the depending plugin in the install order.
* `GetInstallOrderAsync_CircularDependency_ReturnsFailure`: Checks that a circular dependency between plugins results in a failure.
* `GetInstallOrderAsync_DiamondDependency_ProducesValidOrder`: Ensures that a diamond dependency (where two plugins depend on the same third plugin) produces a valid install order.
* `FindConflictsAsync_NoConflicts_ReturnsEmptyList`: Tests that when there are no conflicts, an empty list is returned.
* `FindConflictsAsync_IncompatibleConstraints_DetectsConflict`: Verifies that incompatible constraints between plugins are detected as conflicts.
* `BuildResolutionPlanAsync_PluginNotLoaded_ReturnsFailure`: Checks that building a resolution plan for a plugin that is not loaded results in a failure.
* `BuildResolutionPlanAsync_NoDependencies_PlanHasOneStep`: Tests that a plugin with no dependencies has a resolution plan with one step.
* `BuildResolutionPlanAsync_WithSatisfiedDependency_StepMarkedAlreadySatisfied`: Ensures that when a dependency is already satisfied, the corresponding step in the resolution plan is marked as already satisfied.
* `BuildResolutionPlanAsync_WithConflict_PlanIsNotExecutable`: Verifies that a resolution plan with a conflict is not executable.

## Usage
Here are two examples of using the `PluginDependencyResolverTests` class:
```csharp
// Example 1: Testing the install order of plugins with linear dependencies
var resolver = new PluginDependencyResolver();
var pluginA = new Plugin("A");
var pluginB = new Plugin("B", new[] { pluginA });
var pluginC = new Plugin("C", new[] { pluginB });

await resolver.GetInstallOrderAsync(new[] { pluginA, pluginB, pluginC });
// Expected result: [pluginA, pluginB, pluginC]

// Example 2: Detecting conflicts between plugins with incompatible constraints
var pluginX = new Plugin("X", constraints: new[] { new Constraint("OS", "Windows") });
var pluginY = new Plugin("Y", constraints: new[] { new Constraint("OS", "Linux") });

await resolver.FindConflictsAsync(new[] { pluginX, pluginY });
// Expected result: [Conflict between pluginX and pluginY due to incompatible OS constraints]
```

## Notes
When using the `PluginDependencyResolverTests` class, keep in mind the following edge cases and thread-safety considerations:
* The resolver assumes that plugins are properly registered and loaded before attempting to resolve dependencies.
* Circular dependencies will always result in a failure, as they cannot be resolved.
* The resolver is designed to be thread-safe, but concurrent access to the same plugin instances may still lead to unexpected behavior.
* In cases where multiple plugins have the same dependency, the resolver will attempt to find a valid order that satisfies all dependencies. However, if no such order exists, a failure will be returned.
