# DependencyGraphAnalyzerExtensions

Provides extension methods for analyzing dependency graphs within the plugin engine. These methods evaluate dependency health, surface critical issues, and generate simplified visual representations of dependency structures to aid in diagnostics and reporting.

## API

### GetCriticalIssues

```csharp
public static List<string> GetCriticalIssues(this DependencyGraph graph)
```

Identifies all critical-level issues present in the dependency graph. A critical issue typically indicates a condition that prevents the graph from being considered healthy, such as unresolved conflicts, missing required dependencies, or circular references that cannot be resolved automatically.

**Parameters:**
- `graph` — The `DependencyGraph` instance to analyze.

**Return Value:**
A `List<string>` containing human-readable descriptions of each critical issue found. Returns an empty list if no critical issues are detected.

**Exceptions:**
- `ArgumentNullException` — Thrown when `graph` is `null`.

---

### HasHealthyDependencies

```csharp
public static bool HasHealthyDependencies(this DependencyGraph graph)
```

Performs a quick health check on the dependency graph. Returns `true` if the graph contains no critical issues and all dependencies are in a resolvable, consistent state; otherwise returns `false`.

**Parameters:**
- `graph` — The `DependencyGraph` instance to evaluate.

**Return Value:**
`true` if the dependency graph is considered healthy; `false` otherwise.

**Exceptions:**
- `ArgumentNullException` — Thrown when `graph` is `null`.

---

### GetDependencyHealthSummary

```csharp
public static string GetDependencyHealthSummary(this DependencyGraph graph)
```

Produces a formatted summary string describing the overall health of the dependency graph. The summary includes the count of critical issues, a general health verdict, and may contain additional diagnostic context such as the number of nodes evaluated or the presence of warnings.

**Parameters:**
- `graph` — The `DependencyGraph` instance to summarize.

**Return Value:**
A `string` containing the health summary. The format is implementation-defined but intended for logging and diagnostic output.

**Exceptions:**
- `ArgumentNullException` — Thrown when `graph` is `null`.

---

### GenerateSimplifiedGraphAsync

```csharp
public static async Task<string> GenerateSimplifiedGraphAsync(this DependencyGraph graph)
```

Asynchronously generates a simplified textual representation of the dependency graph. The output is a condensed view suitable for quick inspection, omitting verbose details while preserving the essential structure and relationships between nodes.

**Parameters:**
- `graph` — The `DependencyGraph` instance to render.

**Return Value:**
A `Task<string>` that resolves to the simplified graph representation.

**Exceptions:**
- `ArgumentNullException` — Thrown when `graph` is `null`.
- `OperationCanceledException` — Thrown if the asynchronous operation is canceled.

## Usage

### Example 1: Validating Dependency Health Before Execution

```csharp
DependencyGraph graph = pluginEngine.BuildDependencyGraph();

if (!graph.HasHealthyDependencies())
{
    var criticalIssues = graph.GetCriticalIssues();
    foreach (var issue in criticalIssues)
    {
        Console.WriteLine($"CRITICAL: {issue}");
    }
    
    throw new InvalidOperationException(
        "Cannot proceed with unhealthy dependency graph.");
}

Console.WriteLine(graph.GetDependencyHealthSummary());
// Proceed with plugin execution...
```

### Example 2: Generating a Diagnostic Report

```csharp
async Task LogDependencyReportAsync(DependencyGraph graph)
{
    string summary = graph.GetDependencyHealthSummary();
    string simplifiedGraph = await graph.GenerateSimplifiedGraphAsync();
    
    var report = new StringBuilder();
    report.AppendLine("=== Dependency Health Report ===");
    report.AppendLine(summary);
    report.AppendLine();
    
    if (!graph.HasHealthyDependencies())
    {
        report.AppendLine("Critical Issues:");
        foreach (var issue in graph.GetCriticalIssues())
        {
            report.AppendLine($"  - {issue}");
        }
        report.AppendLine();
    }
    
    report.AppendLine("Simplified Graph:");
    report.AppendLine(simplifiedGraph);
    
    await File.WriteAllTextAsync("dependency-report.txt", report.ToString());
}
```

## Notes

- All methods throw `ArgumentNullException` when passed a `null` graph reference. Callers should guard against null before invoking any member.
- `GetCriticalIssues` and `HasHealthyDependencies` are synchronous and perform their analysis immediately. For large graphs, consider the potential performance impact of repeated calls; cache results if the graph has not mutated between checks.
- `HasHealthyDependencies` returns `false` whenever `GetCriticalIssues` returns a non-empty list. The two methods are logically consistent: a graph is healthy if and only if it has zero critical issues.
- `GenerateSimplifiedGraphAsync` is the only asynchronous member. It may internally traverse the graph structure, and its execution time scales with graph size. Cancellation is supported through the standard `CancellationToken` mechanism if the underlying implementation accepts one; otherwise, cancellation may not be honored mid-operation.
- These methods are designed as extension methods on `DependencyGraph`. They do not mutate the graph state and are safe to call concurrently from multiple threads, provided the underlying `DependencyGraph` instance is itself thread-safe or not being modified during the call.
- The string returned by `GetDependencyHealthSummary` is intended for human consumption. Its exact format is not contractual and may vary across versions. Do not parse it programmatically for decision-making; use `HasHealthyDependencies` and `GetCriticalIssues` for logic.
