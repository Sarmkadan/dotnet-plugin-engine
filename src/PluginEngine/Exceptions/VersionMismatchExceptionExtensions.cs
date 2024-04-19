#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace PluginEngine.Exceptions;

/// <summary>
/// Provides extension methods for VersionMismatchException to enhance error handling and reporting.
/// </summary>
public static class VersionMismatchExceptionExtensions
{
    /// <summary>
    /// Creates a formatted error message suitable for user display or logging.
    /// </summary>
    /// <param name="exception">The VersionMismatchException instance.</param>
    /// <returns>A formatted error message string.</returns>
    public static string GetFormattedErrorMessage(this VersionMismatchException exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

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
    public static bool IsCriticalVersionMismatch(this VersionMismatchException exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
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
    public static VersionMismatchException WithContext(this VersionMismatchException exception, string key, object value)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var newException = new VersionMismatchException(
            exception.Message,
            exception.ExpectedVersion,
            exception.ActualVersion,
            exception.ComponentType,
            exception.ComponentName,
            exception.InnerException
        )
        {
            ExpectedVersion = exception.ExpectedVersion,
            ActualVersion = exception.ActualVersion,
            ComponentType = exception.ComponentType,
            ComponentName = exception.ComponentName
        };

        // Copy context from original exception if it has PluginException base
        if (exception is PluginException pluginEx && pluginEx.Context.Count > 0)
        {
            foreach (var kvp in pluginEx.Context)
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
    public static string GetSimplifiedMessage(this VersionMismatchException exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return $"VersionMismatch[{exception.ComponentType}:{exception.ComponentName}] Expected: {exception.ExpectedVersion}, Actual: {exception.ActualVersion}";
    }
}