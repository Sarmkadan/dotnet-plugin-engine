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
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Descriptive message about the operation.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error code for failed operations.
    /// </summary>
    public int? ErrorCode { get; set; }

    /// <summary>
    /// Detailed error information.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Operation execution time in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Timestamp when operation occurred.
    /// </summary>
    public DateTime TimestampUtc { get; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful operation result.
    /// </summary>
    public static PluginOperationResult CreateSuccess(string message, long durationMs = 0)
    {
        return new PluginOperationResult
        {
            Success = true,
            Message = message,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a failed operation result.
    /// </summary>
    public static PluginOperationResult CreateFailure(
        string message,
        int errorCode = 500,
        string? details = null,
        long durationMs = 0)
    {
        return new PluginOperationResult
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            ErrorDetails = details,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a result from an exception.
    /// </summary>
    public static PluginOperationResult FromException(Exception ex, long durationMs = 0)
    {
        var errorCode = ex switch
        {
            PluginLoadException => 1001,
            DependencyResolutionException => 1002,
            VersionMismatchException => 1003,
            _ => 500
        };

        return CreateFailure(ex.Message, errorCode, ex.InnerException?.Message, durationMs);
    }
}

/// <summary>
/// Generic result wrapper for plugin operations that return data.
/// </summary>
public class PluginOperationResult<T> : PluginOperationResult
{
    /// <summary>
    /// Result data from the operation.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Creates a successful operation result with data.
    /// </summary>
    public static PluginOperationResult<T> CreateSuccess(
        T data,
        string message,
        long durationMs = 0)
    {
        return new PluginOperationResult<T>
        {
            Success = true,
            Message = message,
            Data = data,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a failed operation result.
    /// </summary>
    public static PluginOperationResult<T> CreateFailure(
        string message,
        int errorCode = 500,
        string? details = null,
        long durationMs = 0)
    {
        return new PluginOperationResult<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            ErrorDetails = details,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a result from an exception.
    /// </summary>
    public static PluginOperationResult<T> FromException(Exception ex, long durationMs = 0)
    {
        var errorCode = ex switch
        {
            PluginLoadException => 1001,
            DependencyResolutionException => 1002,
            VersionMismatchException => 1003,
            _ => 500
        };

        return CreateFailure(ex.Message, errorCode, ex.InnerException?.Message, durationMs);
    }
}

/// <summary>
/// Batch result wrapper for operations affecting multiple plugins.
/// </summary>
public class PluginBatchOperationResult
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
