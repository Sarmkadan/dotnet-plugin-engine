#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text;

namespace PluginEngine.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="VersionMismatchException"/> to enhance error handling and reporting.
/// </summary>
public static class VersionMismatchExceptionExtensions
{
    /// <summary>
    /// Creates a formatted error message suitable for user display or logging.
    /// </summary>
    /// <param name="exception">The VersionMismatchException instance.</param>
    /// <returns>A formatted error message string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string GetFormattedErrorMessage(this VersionMismatchException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var builder = new StringBuilder();
        builder.AppendLine("Version Mismatch Error:");
        builder.AppendLine($"Component: {exception.ComponentType} - {exception.ComponentName}");
        builder.AppendLine($"Expected Version: {exception.ExpectedVersion}");
        builder.AppendLine($"Actual Version: {exception.ActualVersion}");

        if (!string.IsNullOrEmpty(exception.Message))
        {
            builder.AppendLine();
            builder.AppendLine("Details:");
            builder.AppendLine(exception.Message);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Checks if the version mismatch is critical (major version difference).
    /// </summary>
    /// <param name="exception">The VersionMismatchException instance.</param>
    /// <returns>True if the major version numbers differ; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsCriticalVersionMismatch(this VersionMismatchException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (string.IsNullOrEmpty(exception.ExpectedVersion) || string.IsNullOrEmpty(exception.ActualVersion))
        {
            return false;
        }

        try
        {
            var expectedParts = exception.ExpectedVersion.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var actualParts = exception.ActualVersion.Split('.', StringSplitOptions.RemoveEmptyEntries);

            if (expectedParts.Length >= 1 && actualParts.Length >= 1 &&
                int.TryParse(expectedParts[0], out var expectedMajor) &&
                int.TryParse(actualParts[0], out var actualMajor))
            {
                return expectedMajor != actualMajor;
            }

            return false;
        }
        catch
        {
            // If parsing fails, assume it's not critical
            return false;
        }
    }

    /// <summary>
    /// Creates a new VersionMismatchException with additional context information.
    /// </summary>
    /// <param name="exception">The VersionMismatchException instance.</param>
    /// <param name="key">The context key.</param>
    /// <param name="value">The context value.</param>
    /// <returns>A new VersionMismatchException with the added context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    public static VersionMismatchException WithContext(this VersionMismatchException exception, string key, object value)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(key);

        var newException = new VersionMismatchException(
            exception.Message,
            exception.ExpectedVersion,
            exception.ActualVersion,
            exception.ComponentType,
            exception.ComponentName,
            exception.InnerException
        );

        // Copy context from original exception
        if (exception.Context.Count > 0)
        {
            foreach (var kvp in exception.Context)
            {
                newException.Context[kvp.Key] = kvp.Value;
            }
        }

        newException.Context[key] = value;
        newException.ErrorCode = exception.ErrorCode;

        return newException;
    }

    /// <summary>
    /// Creates a simplified version of the exception message that can be safely logged.
    /// </summary>
    /// <param name="exception">The VersionMismatchException instance.</param>
    /// <returns>A simplified error message string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string GetSimplifiedMessage(this VersionMismatchException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return $"VersionMismatch[{exception.ComponentType}:{exception.ComponentName}] Expected: {exception.ExpectedVersion}, Actual: {exception.ActualVersion}";
    }
}