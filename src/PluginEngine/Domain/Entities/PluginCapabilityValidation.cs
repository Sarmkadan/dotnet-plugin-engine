#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Provides validation helpers for <see cref="PluginCapability"/> instances.
/// </summary>
public static class PluginCapabilityValidation
{
    /// <summary>
    /// Validates a <see cref="PluginCapability"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The capability to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PluginCapability value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            problems.Add("Id cannot be empty (Guid.Empty).");
        }

        // Validate PluginId
        if (value.PluginId == Guid.Empty)
        {
            problems.Add("PluginId cannot be empty (Guid.Empty).");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name cannot be null or whitespace.");
        }

        // Validate Version
        if (string.IsNullOrWhiteSpace(value.Version))
        {
            problems.Add("Version cannot be null or whitespace.");
        }
        else if (!System.Version.TryParse(value.Version, out _))
        {
            problems.Add("Version must be a valid version string (e.g., 1.0.0).");
        }

        // Validate InterfaceTypeName
        if (string.IsNullOrWhiteSpace(value.InterfaceTypeName))
        {
            problems.Add("InterfaceTypeName cannot be null or whitespace.");
        }

        // Validate ImplementationTypeName
        if (string.IsNullOrWhiteSpace(value.ImplementationTypeName))
        {
            problems.Add("ImplementationTypeName cannot be null or whitespace.");
        }

        // Validate Description (should not be null or whitespace)
        if (string.IsNullOrWhiteSpace(value.Description))
        {
            problems.Add("Description cannot be null or whitespace.");
        }

        // Validate Tags (should be parseable)
        if (!string.IsNullOrWhiteSpace(value.Tags))
        {
            var tags = value.Tags.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            if (tags.Count == 0)
            {
                problems.Add("Tags must contain at least one non-empty tag.");
            }
        }

        // Validate CreatedAt (should not be default DateTime)
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt cannot be default DateTime.");
        }

        // Validate ModifiedAt (should not be default DateTime and should be >= CreatedAt)
        if (value.ModifiedAt == default)
        {
            problems.Add("ModifiedAt cannot be default DateTime.");
        }
        else if (value.ModifiedAt < value.CreatedAt)
        {
            problems.Add("ModifiedAt cannot be earlier than CreatedAt.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="PluginCapability"/> instance is valid.
    /// </summary>
    /// <param name="value">The capability to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PluginCapability value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="PluginCapability"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with the validation problems if it is not.
    /// </summary>
    /// <param name="value">The capability to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this PluginCapability value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PluginCapability is invalid. Problems: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}