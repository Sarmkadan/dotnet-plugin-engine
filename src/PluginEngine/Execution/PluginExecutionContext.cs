#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Execution;

/// <summary>
/// Represents the execution context for a plugin operation.
/// Tracks execution state, performance metrics, and resource usage.
/// </summary>
public sealed class PluginExecutionContext
{
    /// <summary>
    /// Unique identifier for this execution.
    /// </summary>
    public Guid ExecutionId { get; } = Guid.NewGuid();

    /// <summary>
    /// Plugin being executed.
    /// </summary>
    public required Plugin Plugin { get; set; }

    /// <summary>
    /// Operation type (Load, Unload, Execute, etc.).
    /// </summary>
    public required string OperationType { get; set; }

    /// <summary>
    /// Execution start time.
    /// </summary>
    public DateTime StartedAtUtc { get; } = DateTime.UtcNow;

    /// <summary>
    /// Execution completion time.
    /// </summary>
    public DateTime? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Total execution duration.
    /// </summary>
    public TimeSpan Duration => (CompletedAtUtc ?? DateTime.UtcNow) - StartedAtUtc;

    /// <summary>
    /// Current execution state.
    /// </summary>
    public ExecutionState State { get; set; } = ExecutionState.Running;

    /// <summary>
    /// Custom context data.
    /// </summary>
    public Dictionary<string, object> Data { get; } = [];

    /// <summary>
    /// Any error that occurred during execution.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Execution result value.
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Performance metrics collected during execution.
    /// </summary>
    public ExecutionMetrics Metrics { get; } = new();

    /// <summary>
    /// Marks execution as completed successfully.
    /// </summary>
    public void CompleteSuccess(object? result = null)
    {
        CompletedAtUtc = DateTime.UtcNow;
        State = ExecutionState.Completed;
        Result = result;
    }

    /// <summary>
    /// Marks execution as completed with failure.
    /// </summary>
    public void CompleteFailed(Exception ex)
    {
        CompletedAtUtc = DateTime.UtcNow;
        State = ExecutionState.Failed;
        Exception = ex;
    }

    /// <summary>
    /// Marks execution as cancelled.
    /// </summary>
    public void Cancel()
    {
        CompletedAtUtc = DateTime.UtcNow;
        State = ExecutionState.Cancelled;
    }

    /// <summary>
    /// Gets summary information about the execution.
    /// </summary>
    public ExecutionSummary GetSummary() => new()
    {
        ExecutionId = ExecutionId,
        PluginName = Plugin.Name,
        OperationType = OperationType,
        State = State,
        Duration = Duration,
        StartedAtUtc = StartedAtUtc,
        CompletedAtUtc = CompletedAtUtc,
        IsSuccessful = State == ExecutionState.Completed,
        ErrorMessage = Exception?.Message
    };
}

/// <summary>
/// Enumeration of possible execution states.
/// </summary>
public enum ExecutionState
{
    /// <summary>
    /// The plugin is currently running.
    /// </summary>
    Running,
    /// <summary>
    /// The plugin completed successfully.
    /// </summary>
    Completed,
    /// <summary>
    /// The plugin failed during execution.
    /// </summary>
    Failed,
    /// <summary>
    /// The plugin execution was cancelled.
    /// </summary>
    Cancelled,
    /// <summary>
    /// The plugin execution timed out.
    /// </summary>
    Timeout
}

/// <summary>
/// Performance metrics for an execution.
/// </summary>
public sealed class ExecutionMetrics
{
    /// <summary>
    /// CPU time consumed during execution, in milliseconds.
    /// </summary>
    public long CpuTimeMs { get; set; }

    /// <summary>
    /// Total memory bytes allocated during execution.
    /// </summary>
    public long MemoryBytesAllocated { get; set; }

    /// <summary>
    /// Number of garbage collections that occurred during execution.
    /// </summary>
    public int GarbageCollections { get; set; }

    /// <summary>
    /// Timestamp when the metrics were collected (UTC).
    /// </summary>
    public DateTime CollectedAtUtc { get; } = DateTime.UtcNow;

    /// <summary>
    /// Custom metrics collected during execution.
    /// </summary>
    public Dictionary<string, long> CustomMetrics { get; } = [];
}

/// <summary>
/// Summary of a completed execution.
/// </summary>
public sealed class ExecutionSummary
{
    /// <summary>
    /// Unique identifier for the execution.
    /// </summary>
    public required Guid ExecutionId { get; set; }

    /// <summary>
    /// Name of the plugin that was executed.
    /// </summary>
    public required string PluginName { get; set; }

    /// <summary>
    /// Type of operation that was performed (Load, Unload, Execute, etc.).
    /// </summary>
    public required string OperationType { get; set; }

    /// <summary>
    /// Final state of the execution.
    /// </summary>
    public required ExecutionState State { get; set; }

    /// <summary>
    /// Total duration of the execution.
    /// </summary>
    public required TimeSpan Duration { get; set; }

    /// <summary>
    /// Start time of the execution (UTC).
    /// </summary>
    public required DateTime StartedAtUtc { get; set; }

    /// <summary>
    /// Completion time of the execution (UTC), if completed.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Whether the execution was successful.
    /// </summary>
    public required bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if the execution failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Returns a string representation of the execution summary.
    /// </summary>
    /// <returns>A formatted string containing plugin name, operation type, status, and duration.</returns>
    public override string ToString()
    {
        var status = IsSuccessful ? "Success" : "Failed";
        return $"{PluginName} {OperationType} [{status}] - {Duration.TotalMilliseconds:F0}ms";
    }
}
