#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Helpers;

/// <summary>
/// Extension methods for DependencyGraphAnalyzer providing additional utility functions
/// for dependency graph analysis and manipulation.
/// </summary>
public static class DependencyGraphAnalyzerExtensions
{
    /// <summary>
    /// Filters the issues list to return only critical issues that require immediate attention.
    /// </summary>
    /// <param name="analyzer">The analyzer instance</param>
    /// <param name="report">The dependency analysis report</param>
    /// <returns>List of critical issues</returns>
    public static List<string> GetCriticalIssues(this DependencyGraphAnalyzer analyzer, DependencyAnalysisReport report)
    {
        return report.Issues
            .Where(issue => issue.Contains("Circular") || issue.Contains("High number") || issue.Contains("More than 50%"))
            .ToList();
    }

    /// <summary>
    /// Determines if a plugin has healthy dependency structure based on analysis.
    /// </summary>
    /// <param name="analyzer">The analyzer instance</param>
    /// <param name="report">The dependency analysis report</param>
    /// <returns>True if dependencies are healthy, false otherwise</returns>
    public static bool HasHealthyDependencies(this DependencyGraphAnalyzer analyzer, DependencyAnalysisReport report)
    {
        var criticalIssues = analyzer.GetCriticalIssues(report);

        return !report.HasCircularDependencies &&
               report.DirectDependencies <= 20 &&
               report.Issues.Count == 0 &&
               criticalIssues.Count == 0;
    }

    /// <summary>
    /// Gets a summary string that describes the dependency health of a plugin.
    /// </summary>
    /// <param name="analyzer">The analyzer instance</param>
    /// <param name="report">The dependency analysis report</param>
    /// <returns>Human-readable summary of dependency health</returns>
    public static string GetDependencyHealthSummary(this DependencyGraphAnalyzer analyzer, DependencyAnalysisReport report)
    {
        var criticalIssues = analyzer.GetCriticalIssues(report);

        if (criticalIssues.Count > 0)
        {
            return $"CRITICAL: {string.Join(", ", criticalIssues)}";
        }

        if (report.HasCircularDependencies)
        {
            return "WARNING: Circular dependencies detected";
        }

        if (report.DirectDependencies > 20)
        {
            return $"WARNING: High direct dependency count ({report.DirectDependencies}) - consider refactoring";
        }

        if (report.Issues.Count > 0)
        {
            return $"INFO: {report.Issues.Count} issues found - review recommended";
        }

        return "HEALTHY: All dependency checks passed";
    }

    /// <summary>
    /// Creates a simplified dependency graph visualization focused on a specific plugin.
    /// </summary>
    /// <param name="analyzer">The analyzer instance</param>
    /// <param name="rootPlugin">The root plugin to visualize</param>
    /// <param name="maxDepth">Maximum depth to traverse in the dependency tree</param>
    /// <returns>Simplified dependency graph visualization</returns>
    public static async Task<string> GenerateSimplifiedGraphAsync(this DependencyGraphAnalyzer analyzer, Plugin rootPlugin, int maxDepth = 3)
    {
        var sb = new StringBuilder();
        var visited = new HashSet<Guid>();

        sb.AppendLine($"Simplified Dependency Graph for: {rootPlugin.Name} v{rootPlugin.Version}");
        sb.AppendLine(new string('-', 60));

        await analyzer.GenerateSimplifiedNodeAsync(sb, rootPlugin, 0, maxDepth, visited);

        return sb.ToString();
    }

    private static async Task GenerateSimplifiedNodeAsync(
        this DependencyGraphAnalyzer analyzer,
        StringBuilder sb,
        Plugin plugin,
        int currentDepth,
        int maxDepth,
        HashSet<Guid> visited)
    {
        if (visited.Contains(plugin.Id) || currentDepth > maxDepth)
        {
            return;
        }

        visited.Add(plugin.Id);

        var indent = new string(' ', currentDepth * 2);
        sb.AppendLine($"{indent}├─ {plugin.Name} v{plugin.Version}");

        if (currentDepth < maxDepth)
        {
            foreach (var dep in plugin.Dependencies.Take(5)) // Limit to 5 dependencies for readability
            {
                sb.AppendLine($"{indent}   └─ → {dep.DependencyPluginId}");
            }

            if (plugin.Dependencies.Count > 5)
            {
                sb.AppendLine($"{indent}   └─ ... and {plugin.Dependencies.Count - 5} more dependencies");
            }
        }

        visited.Remove(plugin.Id);
    }
}