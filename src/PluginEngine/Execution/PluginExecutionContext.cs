// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Execution;

/// <summary>
/// Represents the execution context for a plugin operation.
/// Tracks execution state, performance metrics, and resource usage.
/// </summary>
public class PluginExecutionContext
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
    Running,
    Completed,
    Failed,
    Cancelled,
    Timeout
}

/// <summary>
/// Performance metrics for an execution.
/// </summary>
public class ExecutionMetrics
{
    public long CpuTimeMs { get; set; }
    public long MemoryBytesAllocated { get; set; }
    public int GarbageCollections { get; set; }
    public DateTime CollectedAtUtc { get; } = DateTime.UtcNow;

    public Dictionary<string, long> CustomMetrics { get; } = [];
}

/// <summary>
/// Summary of a completed execution.
/// </summary>
public class ExecutionSummary
{
    public required Guid ExecutionId { get; set; }
    public required string PluginName { get; set; }
    public required string OperationType { get; set; }
    public required ExecutionState State { get; set; }
    public required TimeSpan Duration { get; set; }
    public required DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public required bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }

    public override string ToString()
    {
        var status = IsSuccessful ? "Success" : "Failed";
        return $"{PluginName} {OperationType} [{status}] - {Duration.TotalMilliseconds:F0}ms";
    }
}
