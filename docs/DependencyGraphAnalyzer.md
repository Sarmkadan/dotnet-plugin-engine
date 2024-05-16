# DependencyGraphAnalyzer

A utility class for analyzing the dependency graph of a .NET plugin, including detection of circular dependencies, complexity scoring, and visualization generation.

## API

### `DependencyGraphAnalyzer`
The main analyzer class for plugin dependency graphs. Initializes a new instance for analyzing a specific plugin's dependencies.

### `public async Task<string> GenerateGraphVisualizationAsync()`
Generates a visual representation of the dependency graph as a DOT (Graphviz) formatted string.

- **Returns**: A `Task<string>` resolving to a DOT-formatted string representing the dependency graph.
- **Exceptions**: Throws `InvalidOperationException` if the analyzer has not been initialized or if analysis has not been performed.

### `public async Task<DependencyAnalysisReport> AnalyzeAsync()`
Performs a full analysis of the plugin's dependency graph, including circular dependency detection and complexity scoring.

- **Returns**: A `Task<DependencyAnalysisReport>` containing detailed analysis results.
- **Exceptions**: Throws `InvalidOperationException` if the analyzer is not properly initialized (e.g., `PluginName` not set).

### `public async Task<List<Guid>> FindDependentsAsync(Guid pluginId)`
Finds all plugins that directly or indirectly depend on the plugin identified by `pluginId`.

- **Parameters**:
  - `pluginId` (Guid): The unique identifier of the plugin whose dependents are to be found.
- **Returns**: A `Task<List<Guid>>` resolving to a list of plugin IDs that depend on the specified plugin.
- **Exceptions**: Throws `InvalidOperationException` if analysis has not been performed.

### `public required string PluginName { get; set; }`
Gets or sets the name of the plugin being analyzed. Must be set before calling `AnalyzeAsync`.

### `public int DirectDependencies { get; }`
Gets the number of direct dependencies of the analyzed plugin.

### `public int TotalDependencies { get; }`
Gets the total number of dependencies (direct and transitive) of the analyzed plugin.

### `public bool HasCircularDependencies { get; }`
Gets a value indicating whether the dependency graph contains any circular dependencies.

### `public int ComplexityScore { get; }`
Gets a numeric score representing the complexity of the dependency graph (higher values indicate more complex dependency chains).

### `public DateTime AnalyzedAtUtc { get; }`
Gets the UTC timestamp when the analysis was completed.

### `public List<string> Issues { get; }`
Gets a list of issues or warnings detected during analysis (e.g., missing dependencies, version conflicts).

### `public string GetComplexityLevel()`
Returns a human-readable label describing the complexity level based on the `ComplexityScore`.

- **Returns**: A `string` representing the complexity level (e.g., "Low", "Medium", "High").

## Usage

### Basic Analysis and Reporting
