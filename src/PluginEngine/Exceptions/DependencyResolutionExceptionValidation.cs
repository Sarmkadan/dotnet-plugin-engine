#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace PluginEngine.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="DependencyResolutionException"/> instances.
/// </summary>
public static class DependencyResolutionExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="DependencyResolutionException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DependencyResolutionException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate VersionConstraint
        if (value.VersionConstraint is { Length: > 0 } versionConstraint)
        {
            if (versionConstraint.Length > 256)
            {
                problems.Add("VersionConstraint exceeds maximum length of 256 characters.");
            }

            if (versionConstraint.Any(c => char.IsControl(c)))
            {
                problems.Add("VersionConstraint contains control characters.");
            }
        }

        // Validate Reason
        if (!Enum.IsDefined(value.Reason))
        {
            problems.Add("Reason has an invalid enum value.");
        }

        // Validate UnresolvedDependencies
        if (value.UnresolvedDependencies is null)
        {
            problems.Add("UnresolvedDependencies collection is null.");
        }
        else
        {
            if (value.UnresolvedDependencies.Count > 1000)
            {
                problems.Add("UnresolvedDependencies collection exceeds maximum size of 1000 items.");
            }

            var duplicateDependencies = value.UnresolvedDependencies
                .GroupBy(d => d, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateDependencies.Count > 0)
            {
                problems.Add($"UnresolvedDependencies contains {duplicateDependencies.Count} duplicate entries: {string.Join(", ", duplicateDependencies.Take(5))}{(duplicateDependencies.Count > 5 ? "..." : "")}.");
            }

            foreach (var dependency in value.UnresolvedDependencies)
            {
                if (string.IsNullOrWhiteSpace(dependency))
                {
                    problems.Add("UnresolvedDependencies contains null, empty, or whitespace-only entries.");
                    break; // Don't add multiple entries for the same issue
                }

                if (dependency.Length > 256)
                {
                    problems.Add("UnresolvedDependencies contains entries exceeding maximum length of 256 characters.");
                    break;
                }

                if (dependency.Any(c => char.IsControl(c)))
                {
                    problems.Add("UnresolvedDependencies contains entries with control characters.");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DependencyResolutionException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DependencyResolutionException value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="DependencyResolutionException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this DependencyResolutionException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DependencyResolutionException is invalid. Problems:\n{string.Join("\n", problems)}",
                nameof(value));
        }
    }
}