#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace PluginEngine.Exceptions;

/// <summary>
/// Extension methods for PluginException to provide additional functionality
/// for working with plugin errors and diagnostics.
/// </summary>
public static class PluginExceptionExtensions
{
    /// <summary>
    /// Creates a new PluginException with the same properties but a different error code.
    /// </summary>
    /// <param name="exception">The original exception</param>
    /// <param name="newErrorCode">The new error code to set</param>
    /// <returns>A new PluginException instance with the updated error code</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="newErrorCode"/> is <see langword="null"/></exception>
    public static PluginException WithErrorCode(this PluginException exception, string newErrorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(newErrorCode);

        return new PluginException(exception.Message, newErrorCode, exception.InnerException)
        {
            EntityId = exception.EntityId,
            Context = new Dictionary<string, object>(exception.Context)
        };
    }

    /// <summary>
    /// Adds multiple context entries to the exception at once.
    /// </summary>
    /// <param name="exception">The exception to add context to</param>
    /// <param name="keyValuePairs">Key-value pairs to add to the context</param>
    /// <returns>The same exception instance for method chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    public static PluginException WithContext(this PluginException exception, params (string Key, object Value)[] keyValuePairs)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(keyValuePairs);

        foreach (var (key, value) in keyValuePairs)
        {
            exception.Context[key] = value;
        }

        return exception;
    }

    /// <summary>
    /// Creates a diagnostic report string that includes all exception details in a structured format.
    /// Useful for logging and debugging.
    /// </summary>
    /// <param name="exception">The exception to generate a report for</param>
    /// <param name="includeStackTrace">Whether to include the full stack trace</param>
    /// <returns>A formatted diagnostic report string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    public static string ToDiagnosticReport(this PluginException exception, bool includeStackTrace = false)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var report = new StringBuilder();

        report.AppendLine("=== PLUGIN EXCEPTION DIAGNOSTIC REPORT ===");
        report.AppendLine($"Error Code: {exception.ErrorCode}");
        report.AppendLine($"Message: {exception.Message}");

        if (exception.EntityId.HasValue)
        {
            report.AppendLine($"Entity ID: {exception.EntityId}");
        }

        if (exception.Context.Count > 0)
        {
            report.AppendLine("Context:");
            foreach (var kvp in exception.Context)
            {
                report.AppendLine($"  - {kvp.Key}: {kvp.Value}");
            }
        }

        if (exception.InnerException is not null)
        {
            report.AppendLine($"Inner Exception: {exception.InnerException.GetType().Name}");
            report.AppendLine($"  Message: {exception.InnerException.Message}");
        }

        if (includeStackTrace && exception.StackTrace is not null)
        {
            report.AppendLine("\n=== STACK TRACE ===");
            report.AppendLine(exception.StackTrace);
        }

        report.AppendLine("=== END REPORT ===");
        return report.ToString();
    }

    /// <summary>
    /// Creates a simplified error message that can be safely exposed to end users.
    /// Strips internal details like error codes and entity IDs from the message.
    /// </summary>
    /// <param name="exception">The exception to simplify</param>
    /// <returns>A user-friendly error message without internal implementation details</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    public static string ToUserFriendlyMessage(this PluginException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Remove error code prefix if present (e.g., "[PLUGIN_ERROR] Message" -> "Message")
        var message = exception.Message;
        if (message.StartsWith('[') && message.IndexOf(']') is var bracketIndex && bracketIndex > 0)
        {
            message = message.Substring(bracketIndex + 1).Trim();
        }

        return message;
    }

    /// <summary>
    /// Determines if this exception represents a specific error type by checking the error code.
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <param name="errorCode">The error code to match against</param>
    /// <returns>True if the error code matches, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="errorCode"/> is <see langword="null"/></exception>
    public static bool IsErrorCode(this PluginException exception, string errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(errorCode);

        return string.Equals(exception.ErrorCode, errorCode, StringComparison.Ordinal);
    }
}
