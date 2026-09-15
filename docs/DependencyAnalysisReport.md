# DependencyAnalysisReport

Report from dependency graph analysis.

## API

### `public required string PluginName { get; set; }`

Name of the analyzed plugin.

### `public int DirectDependencies { get; set; }`

Number of direct dependencies of the plugin.

### `public int TotalDependencies { get; set; }`

Total number of dependencies (including transitive).

### `public bool HasCircularDependencies { get; set; }`

Indicates whether circular dependencies were detected.

### `public int ComplexityScore { get; set; }`

Calculated complexity score (0-100).

#### Complexity Score Computation

The complexity score is calculated as follows:

1. **Direct dependency score**: `DirectDependencies × PerDependencyWeight` (where `PerDependencyWeight = 10`)
2. **Total dependency score**: `min(TotalDependencies × TransitiveDependencyWeight, ComplexityScoreCap)` (where `TransitiveDependencyWeight = 2` and `ComplexityScoreCap = 100`)
3. **Optional dependency factor**: `OptionalDependenciesCount × 5`
4. **Final score**: `min(Total score from steps 1-3, ComplexityScoreCap)`

### `public DateTime AnalyzedAtUtc { get; set; }`

Timestamp when the analysis was performed (UTC).

### `public List<string> Issues { get; set; } = []`

List of identified issues during analysis.

### `public string GetComplexityLevel()`

Returns a human-readable complexity level based on the complexity score.

#### Complexity Level Thresholds

- **Simple**: ComplexityScore < 20
- **Moderate**: 20 ≤ ComplexityScore < 50
- **Complex**: 50 ≤ ComplexityScore < 75
- **Very Complex**: ComplexityScore ≥ 75

### Extension Methods

#### `public static List<string> GetCriticalIssues(this DependencyGraphAnalyzer analyzer, DependencyAnalysisReport report)`

Filters the issues list to return only critical issues that require immediate attention.

**Critical issues include:**
- Issues containing "Circular" (circular dependencies)
- Issues containing "High number" (high number of direct dependencies)
- Issues containing "More than 50%" (majority of dependencies are optional)

#### `public static bool HasHealthyDependencies(this DependencyGraphAnalyzer analyzer, DependencyAnalysisReport report)`

Determines if a plugin has healthy dependency structure based on analysis.

**Returns true when:**
- No circular dependencies exist
- Direct dependencies ≤ PluginEngineConstants.MaxDirectDependencies (20)
- No issues found in the report
- No critical issues found

#### `public static string GetDependencyHealthSummary(this DependencyGraphAnalyzer analyzer, DependencyAnalysisReport report)`

Gets a summary string that describes the dependency health of a plugin.

**Returns:**
- When critical issues exist: `"CRITICAL: {comma-separated critical issues}"`
- When circular dependencies exist: `"WARNING: Circular dependencies detected"`
- When direct dependencies exceed limit: `"WARNING: High direct dependency count ({count}) - consider refactoring"`
- When other issues exist: `"INFO: {count} issues found - review recommended"`
- When healthy: `"HEALTHY: All dependency checks passed"`

## Usage