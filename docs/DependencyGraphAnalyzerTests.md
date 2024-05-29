# DependencyGraphAnalyzerTests
The `DependencyGraphAnalyzerTests` class is designed to test the functionality of the `DependencyGraphAnalyzer` class, which is responsible for analyzing the dependencies between plugins in the dotnet-plugin-engine project. This class contains a set of test methods that verify the correctness of the `DependencyGraphAnalyzer` class in various scenarios, including finding dependents, analyzing dependencies, and generating graph visualizations.

## API
* `public DependencyGraphAnalyzerTests`: The constructor for the `DependencyGraphAnalyzerTests` class.
* `public async Task FindDependentsAsync_WithNoDependents_ReturnsEmpty`: Tests that the `FindDependentsAsync` method returns an empty list when there are no dependents. Parameters: none. Return value: a task that completes when the test is finished. Throws: none.
* `public async Task FindDependentsAsync_WithOneDependentPlugin_ReturnsThatPlugin`: Tests that the `FindDependentsAsync` method returns a list containing a single dependent plugin when there is one dependent plugin. Parameters: none. Return value: a task that completes when the test is finished. Throws: none.
* `public async Task FindDependentsAsync_WithMultipleDependents_ReturnsAll`: Tests that the `FindDependentsAsync` method returns a list containing all dependent plugins when there are multiple dependent plugins. Parameters: none. Return value: a task that completes when the test is finished. Throws: none.
* `public async Task FindDependentsAsync_WithEmptyPluginList_ReturnsEmpty`: Tests that the `FindDependentsAsync` method returns an empty list when the plugin list is empty. Parameters: none. Return value: a task that completes when the test is finished. Throws: none.
* `public async Task AnalyzeAsync_WithNoDependencies_ReturnsZeroCounts`: Tests that the `AnalyzeAsync` method returns zero counts when there are no dependencies. Parameters: none. Return value: a task that completes when the test is finished. Throws: none.
* `public async Task AnalyzeAsync_WithCircularDependency_ReportsIssue`: Tests that the `AnalyzeAsync` method reports an issue when there is a circular dependency. Parameters: none. Return value: a task that completes when the test is finished. Throws: none.
* `public async Task AnalyzeAsync_WithFewDependencies_HasSimpleComplexityLevel`: Tests that the `AnalyzeAsync` method has a simple complexity level when there are few dependencies. Parameters: none. Return value: a task that completes when the test is finished. Throws: none.
* `public async Task AnalyzeAsync_ReturnsPluginName`: Tests that the `AnalyzeAsync` method returns the plugin name. Parameters: none. Return value: a task that completes when the test is finished. Throws: none.
* `public async Task GenerateGraphVisualizationAsync_WithSimplePlugin_ReturnsNonEmptyString`: Tests that the `GenerateGraphVisualizationAsync` method returns a non-empty string when the plugin is simple. Parameters: none. Return value: a task that completes when the test is finished. Throws: none.
* `public void GetComplexityLevel_MapsScoreToExpectedLabel`: Tests that the `GetComplexityLevel` method maps the score to the expected label. Parameters: none. Return value: none. Throws: none.

## Usage
The following examples demonstrate how to use the `DependencyGraphAnalyzerTests` class:
```csharp
// Example 1: Running a test to find dependents
var analyzerTests = new DependencyGraphAnalyzerTests();
await analyzerTests.FindDependentsAsync_WithOneDependentPlugin_ReturnsThatPlugin();

// Example 2: Running a test to analyze dependencies
var analyzerTests = new DependencyGraphAnalyzerTests();
await analyzerTests.AnalyzeAsync_WithFewDependencies_HasSimpleComplexityLevel();
```

## Notes
* The `DependencyGraphAnalyzerTests` class is designed to be used in a testing environment, and its methods should not be called in a production environment.
* The class is not thread-safe, and its methods should not be called concurrently.
* The class assumes that the `DependencyGraphAnalyzer` class is correctly implemented and does not test its internal logic.
* The class may throw exceptions if the `DependencyGraphAnalyzer` class is not correctly implemented or if there are issues with the test data.
