#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Provides useful extension methods for <see cref="AssemblyLoadContextInfo"/> to
/// enhance plugin isolation context management and monitoring.
/// </summary>
public static class AssemblyLoadContextInfoExtensions
{
    /// <summary>
    /// Determines if the load context has exceeded the specified memory threshold.
    /// </summary>
    /// <param name="context">The assembly load context.</param>
    /// <param name="thresholdBytes">The memory threshold in bytes to check against.</param>
    /// <returns>True if memory usage exceeds the threshold; otherwise, false.</returns>
    public static bool HasMemoryExceeded(this AssemblyLoadContextInfo context, long thresholdBytes)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.MemoryUsageBytes > thresholdBytes;
    }

    /// <summary>
    /// Gets the age of the load context in minutes since creation.
    /// </summary>
    /// <param name="context">The assembly load context.</param>
    /// <returns>The age in minutes, or 0 if the context is null.</returns>
    public static double GetAgeInMinutes(this AssemblyLoadContextInfo context)
    {
        if (context is null)
            return 0;

        var age = DateTime.UtcNow - context.CreatedAt;
        return age.TotalMinutes;
    }

    /// <summary>
    /// Gets the time since last activity in minutes.
    /// </summary>
    /// <param name="context">The assembly load context.</param>
    /// <returns>The minutes since last activity, or 0 if the context is null.</returns>
    public static double GetInactivityMinutes(this AssemblyLoadContextInfo context)
    {
        if (context is null)
            return 0;

        var inactivity = DateTime.UtcNow - context.LastActivityAt;
        return inactivity.TotalMinutes;
    }

    /// <summary>
    /// Determines if the load context is considered stale based on inactivity.
    /// A context is stale if it hasn't been active for more than the specified minutes.
    /// </summary>
    /// <param name="context">The assembly load context.</param>
    /// <param name="staleThresholdMinutes">The inactivity threshold in minutes.</param>
    /// <returns>True if the context is stale; otherwise, false.</returns>
    public static bool IsStale(this AssemblyLoadContextInfo context, int staleThresholdMinutes)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.GetInactivityMinutes() > staleThresholdMinutes;
    }

    /// <summary>
    /// Gets a detailed status report for monitoring and diagnostic purposes.
    /// </summary>
    /// <param name="context">The assembly load context.</param>
    /// <returns>A formatted status report string.</returns>
    public static string GetDetailedStatusReport(this AssemblyLoadContextInfo context)
    {
        if (context is null)
            return "Context is null";

        var ageMinutes = context.GetAgeInMinutes();
        var inactivityMinutes = context.GetInactivityMinutes();
        var assemblyCount = context.GetAssemblyCount();
        var memoryMb = Math.Round(context.MemoryUsageBytes / (1024.0 * 1024.0), 2);

        return $"""
Load Context Status Report
========================
Context ID: {context.ContextId}
Name: {context.Name}
Plugin ID: {context.PluginId}
Status: {(context.IsActive ? "ACTIVE" : "INACTIVE")}

Age: {ageMinutes:F2} minutes
Last Activity: {inactivityMinutes:F2} minutes ago
Valid: {context.IsValid()}

Memory Usage: {memoryMb} MB
Assembly Count: {assemblyCount}
Loaded Types: {context.LoadedTypeCount}

Assemblies: {string.Join(", ", context.LoadedAssemblies.Take(5))}{(context.LoadedAssemblies.Count > 5 ? "..." : "")}
""";
    }

    /// <summary>
    /// Determines if the load context is healthy based on various health metrics.
    /// A context is considered healthy if:
    /// - It's active
    /// - It's valid
    /// - Memory usage is reasonable (less than 500MB by default)
    /// - Has recent activity (within last 60 minutes by default)
    /// </summary>
    /// <param name="context">The assembly load context.</param>
    /// <param name="maxMemoryBytes">Maximum allowed memory usage in bytes (default: 500MB).</param>
    /// <param name="maxInactivityMinutes">Maximum allowed inactivity in minutes (default: 60).</param>
    /// <returns>True if the context is healthy; otherwise, false.</returns>
    public static bool IsHealthy(
        this AssemblyLoadContextInfo context,
        long maxMemoryBytes = 524288000,
        int maxInactivityMinutes = 60)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.IsActive &&
               context.IsValid() &&
               !context.HasMemoryExceeded(maxMemoryBytes) &&
               !context.IsStale(maxInactivityMinutes);
    }

    /// <summary>
    /// Gets a list of assembly names that match the specified pattern.
    /// </summary>
    /// <param name="context">The assembly load context.</param>
    /// <param name="pattern">The search pattern (supports wildcards).</param>
    /// <returns>A list of matching assembly names.</returns>
    public static IEnumerable<string> FindAssembliesByPattern(
        this AssemblyLoadContextInfo context,
        string pattern)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        return context.LoadedAssemblies
            .Where(assembly => assembly.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets the memory usage in a human-readable format (KB, MB, GB).
    /// </summary>
    /// <param name="context">The assembly load context.</param>
    /// <returns>A formatted memory usage string.</returns>
    public static string GetMemoryUsageString(this AssemblyLoadContextInfo context)
    {
        if (context is null)
            return "0 bytes";

        var bytes = context.MemoryUsageBytes;

        if (bytes < 1024)
            return $"{bytes} bytes";

        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F2} KB";

        if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0):F2} MB";

        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}