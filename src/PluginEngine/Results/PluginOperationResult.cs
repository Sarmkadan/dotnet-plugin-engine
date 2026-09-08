#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Results;

/// <summary>
/// Standard result wrapper for plugin operations.
/// Provides consistent error handling and response formatting.
/// </summary>
public class PluginOperationResult
{
    /// <summary>
    /// Default error code for unexpected failures.
    /// </summary>
    public const int DefaultErrorCode = 500;

    /// <summary>
    /// Error code for plugin load failures.
    /// </summary>
    public const int PluginLoadErrorCode = 1001;

    /// <summary>
    /// Error code for dependency resolution failures.
    /// </summary>
    public const int DependencyResolutionErrorCode = 1002;

    /// <summary>
    /// Error code for version mismatch failures.
    /// </summary>
    public const int VersionMismatchErrorCode = 1003;
    /// <summary>
    /// Initializes a result for object-initializer compatibility.
    /// Prefer the static factory methods when creating operation results.
    /// </summary>
    public PluginOperationResult()
    {
    }

    /// <summary>
    /// Initializes a fully defined operation result.
    /// </summary>
    /// <param name="success">Whether the operation succeeded.</param>
    /// <param name="message">A descriptive success message or the required failure message.</param>
    /// <param name="errorCode">The error code for a failed operation.</param>
    /// <param name="errorDetails">Additional details for a failed operation.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="success"/> is <see langword="false"/> and <paramref name="message"/> is empty.
    /// </exception>
    protected PluginOperationResult(
        bool success,
        string message,
        int? errorCode,
        string? errorDetails,
        long durationMs)
    {
        if (!success)
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Success = success;
        Message = message ?? string.Empty;
        ErrorCode = success ? null : errorCode;
        ErrorDetails = success ? null : errorDetails;
        DurationMs = durationMs;
    }

    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Descriptive message about the operation.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Error code for failed operations.
    /// </summary>
    public int? ErrorCode { get; init; }

    /// <summary>
    /// Detailed error information.
    /// </summary>
    public string? ErrorDetails { get; init; }

    /// <summary>
    /// Operation execution time in milliseconds.
    /// </summary>
    public long DurationMs { get; init; }

    /// <summary>
    /// Timestamp when operation occurred.
    /// </summary>
    public DateTime TimestampUtc { get; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful operation result.
    /// </summary>
    /// <param name="message">A descriptive success message.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <returns>A successful operation result without error information.</returns>
    public static PluginOperationResult CreateSuccess(string message, long durationMs = 0)
    {
        return new PluginOperationResult(true, message, null, null, durationMs);
    }

    /// <summary>
    /// Creates a failed operation result.
    /// </summary>
    /// <param name="message">The required error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="details">Optional error details.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <returns>A failed operation result.</returns>
    /// <exception cref="ArgumentException"><paramref name="message"/> is empty.</exception>
    public static PluginOperationResult CreateFailure(
        string message,
        int errorCode = DefaultErrorCode,
        string? details = null,
        long durationMs = 0)
    {
        return new PluginOperationResult(false, message, errorCode, details, durationMs);
    }

    /// <summary>
    /// Creates a result from an exception.
    /// </summary>
    /// <param name="ex">The exception that caused the operation to fail.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <returns>A failed operation result containing information from the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
    public static PluginOperationResult FromException(Exception ex, long durationMs = 0)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return CreateFailure(
            ex.Message,
            GetErrorCode(ex),
            ex.InnerException?.Message,
            durationMs);
    }

    /// <summary>
    /// Maps an exception to the corresponding plugin operation error code.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>The corresponding error code.</returns>
    protected static int GetErrorCode(Exception exception)
    {
        return exception switch
        {
            PluginLoadException => PluginLoadErrorCode,
            DependencyResolutionException => DependencyResolutionErrorCode,
            VersionMismatchException => VersionMismatchErrorCode,
            _ => DefaultErrorCode
        };
    }
}

/// <summary>
/// Generic result wrapper for plugin operations that return data.
/// </summary>
public sealed class PluginOperationResult<T> : PluginOperationResult
{
    /// <summary>
    /// Initializes a result for object-initializer compatibility.
    /// Prefer the static factory methods when creating operation results.
    /// </summary>
    public PluginOperationResult()
    {
    }

    private PluginOperationResult(
        bool success,
        string message,
        T? data,
        int? errorCode,
        string? errorDetails,
        long durationMs)
        : base(success, message, errorCode, errorDetails, durationMs)
    {
        Data = data;
    }

    /// <summary>
    /// Result data from the operation.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Creates a successful operation result with data.
    /// </summary>
    /// <param name="data">The operation data.</param>
    /// <param name="message">A descriptive success message.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <returns>A successful operation result containing <paramref name="data"/>.</returns>
    public static PluginOperationResult<T> CreateSuccess(
        T data,
        string message,
        long durationMs = 0)
    {
        return new PluginOperationResult<T>(true, message, data, null, null, durationMs);
    }

    /// <summary>
    /// Creates a failed operation result.
    /// </summary>
    /// <param name="message">The required error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="details">Optional error details.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <returns>A failed operation result without data.</returns>
    /// <exception cref="ArgumentException"><paramref name="message"/> is empty.</exception>
    public static PluginOperationResult<T> CreateFailure(
        string message,
        int errorCode = DefaultErrorCode,
        string? details = null,
        long durationMs = 0)
    {
        return new PluginOperationResult<T>(false, message, default, errorCode, details, durationMs);
    }

    /// <summary>
    /// Creates a result from an exception.
    /// </summary>
    /// <param name="ex">The exception that caused the operation to fail.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <returns>A failed operation result containing information from the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
    public static PluginOperationResult<T> FromException(Exception ex, long durationMs = 0)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return CreateFailure(ex.Message, GetErrorCode(ex), ex.InnerException?.Message, durationMs);
    }
}

/// <summary>
/// Batch result wrapper for operations affecting multiple plugins.
/// </summary>
public sealed class PluginBatchOperationResult
{
    /// <summary>
    /// Number of successful operations.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed operations.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Results for each plugin operation.
    /// </summary>
    public List<(Guid PluginId, string PluginName, PluginOperationResult Result)> Results { get; } = [];

    /// <summary>
    /// Overall operation success (all or most succeeded).
    /// </summary>
    public bool IsSuccessful => FailureCount == 0 || SuccessCount > FailureCount;

    /// <summary>
    /// Total number of operations recorded in the batch.
    /// </summary>
    public int TotalCount => Results.Count;

    /// <summary>
    /// Total execution time in milliseconds.
    /// </summary>
    public long TotalDurationMs { get; set; }

    /// <summary>
    /// Adds a result to the batch.
    /// </summary>
    public void AddResult(Guid pluginId, string pluginName, PluginOperationResult result)
    {
        Results.Add((pluginId, pluginName, result));

        if (result.Success)
            SuccessCount++;
        else
            FailureCount++;
    }

    /// <summary>
    /// Gets summary statistics.
    /// </summary>
    public string GetSummary()
    {
        return $"Batch Operation: {SuccessCount} succeeded, {FailureCount} failed, " +
               $"Total time: {TotalDurationMs}ms";
    }
}
