#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;

namespace PluginEngine.Configuration;

/// <summary>
/// Extension methods for <see cref="PluginEngineOptions"/>.
/// </summary>
public static class PluginEngineOptionsExtensions
{
    /// <summary>
    /// Creates a shallow copy of the <see cref="PluginEngineOptions"/> instance.
    /// </summary>
    /// <param name="options">The options to clone.</param>
    /// <returns>A shallow copy of the options.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="options"/> is null.</exception>
    public static PluginEngineOptions Clone(this PluginEngineOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        return new PluginEngineOptions
        {
            PluginDirectory = options.PluginDirectory,
            EnableHotReload = options.EnableHotReload,
            HotReloadCheckIntervalMs = options.HotReloadCheckIntervalMs,
            EnableDependencyCaching = options.EnableDependencyCaching,
            OperationTimeoutMs = options.OperationTimeoutMs,
            EnableLogging = options.EnableLogging,
            LogLevel = options.LogLevel,
            MaxConcurrentPluginLoads = options.MaxConcurrentPluginLoads,
            DependencyCacheTtlMinutes = options.DependencyCacheTtlMinutes,
            TargetFramework = options.TargetFramework,
            StrictVersionChecking = options.StrictVersionChecking,
            EnableCircularDependencyDetection = options.EnableCircularDependencyDetection,
            MaxDependencyResolutionAttempts = options.MaxDependencyResolutionAttempts
        };
    }

    /// <summary>
    /// Determines whether hot reload is enabled.
    /// </summary>
    /// <param name="options">The options to check.</param>
    /// <returns>True if hot reload is enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="options"/> is null.</exception>
    public static bool IsHotReloadEnabled(this PluginEngineOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        return options.EnableHotReload;
    }

    /// <summary>
    /// Gets the absolute path of the plugin directory.
    /// </summary>
    /// <param name="options">The options containing the plugin directory.</param>
    /// <returns>The absolute path of the plugin directory.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">If <see cref="PluginEngineOptions.PluginDirectory"/> is null, empty, or consists only of white-space characters.</exception>
    public static string GetAbsolutePluginDirectory(this PluginEngineOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(options.PluginDirectory))
            throw new ArgumentException("Plugin directory cannot be empty.", nameof(options.PluginDirectory));

        return Path.GetFullPath(options.PluginDirectory);
    }

    /// <summary>
    /// Validates the plugin engine options and throws an exception if validation fails.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="options"/> is null.</exception>
    /// <exception cref="InvalidOperationException">If the options are invalid. The exception message lists all validation errors.</exception>
    public static void EnsureValid(this PluginEngineOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (!options.IsValid())
        {
            var errors = options.GetValidationErrors();
            throw new InvalidOperationException($"Plugin engine options are invalid: {string.Join("; ", errors)}");
        }
    }
}