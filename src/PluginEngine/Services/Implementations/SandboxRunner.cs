#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Executes plugin code within a controlled sandbox, enforcing timeouts,
/// memory limits, and exception isolation to prevent a misbehaving plugin
/// from destabilizing the host process.
/// </summary>
public sealed class SandboxRunner
{
    private readonly ILogger<SandboxRunner> _logger;
    private readonly IOptions<PluginEngineOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxRunner"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="options">Plugin engine configuration options.</param>
    public SandboxRunner(ILogger<SandboxRunner> logger, IOptions<PluginEngineOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Executes <paramref name="action"/> inside a sandbox with the configured timeout.
    /// Returns a <see cref="SandboxResult"/> indicating success or failure.
    /// </summary>
    /// <param name="pluginId">The plugin whose code is being executed.</param>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <param name="cancellationToken">External cancellation token.</param>
    /// <returns>A <see cref="SandboxResult"/> with execution outcome and timing data.</returns>
    public async Task<SandboxResult> ExecuteAsync(
        Guid pluginId, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        var timeout = TimeSpan.FromMilliseconds(_options.Value.OperationTimeoutMs);
        var sw = Stopwatch.StartNew();
        _logger.LogDebug("Sandbox executing plugin {PluginId} with timeout {Timeout}ms.", pluginId, timeout.TotalMilliseconds);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            await action(cts.Token);
            sw.Stop();
            _logger.LogInformation("Plugin {PluginId} executed successfully in {Elapsed}ms.", pluginId, sw.ElapsedMilliseconds);
            return SandboxResult.Success(pluginId, sw.Elapsed);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            _logger.LogWarning("Plugin {PluginId} execution timed out after {Elapsed}ms.", pluginId, sw.ElapsedMilliseconds);
            return SandboxResult.TimedOut(pluginId, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Plugin {PluginId} execution faulted after {Elapsed}ms.", pluginId, sw.ElapsedMilliseconds);
            return SandboxResult.Faulted(pluginId, sw.Elapsed, ex);
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> inside a sandbox and returns its result on success.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="pluginId">The plugin whose code is being executed.</param>
    /// <param name="func">The function to execute.</param>
    /// <param name="cancellationToken">External cancellation token.</param>
    /// <returns>A <see cref="SandboxResult{T}"/> with the value or failure details.</returns>
    public async Task<SandboxResult<T>> ExecuteAsync<T>(
        Guid pluginId, Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        var timeout = TimeSpan.FromMilliseconds(_options.Value.OperationTimeoutMs);
        var sw = Stopwatch.StartNew();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            var result = await func(cts.Token);
            sw.Stop();
            return SandboxResult<T>.Success(pluginId, sw.Elapsed, result);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            return SandboxResult<T>.TimedOut(pluginId, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return SandboxResult<T>.Faulted(pluginId, sw.Elapsed, ex);
        }
    }

    /// <summary>
    /// Validates that the current process memory consumption is below the configured
    /// threshold, returning <c>false</c> if the host is under memory pressure.
    /// </summary>
    /// <returns><c>true</c> if memory usage is within acceptable limits.</returns>
    public bool IsMemoryWithinLimits()
    {
        long maxBytes = _options.Value.MaxConcurrentPluginLoads * 256L * 1024 * 1024;
        if (maxBytes <= 0)
        {
            return true;
        }

        var used = GC.GetTotalMemory(forceFullCollection: false);
        var withinLimits = used < maxBytes;

        if (!withinLimits)
        {
            _logger.LogWarning("Memory pressure: {Used} bytes used, limit is {Max} bytes.", used, maxBytes);
        }

        return withinLimits;
    }
}

/// <summary>
/// Encapsulates the result of a sandboxed plugin execution.
/// </summary>
public sealed class SandboxResult
{
    /// <summary>Gets the plugin that was executed.</summary>
    public Guid PluginId { get; init; }

    /// <summary>Gets the wall-clock duration of the execution.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Gets the outcome of the execution.</summary>
    public SandboxOutcome Outcome { get; init; }

    /// <summary>Gets the exception if the execution faulted.</summary>
    public Exception? Exception { get; init; }

    internal static SandboxResult Success(Guid pluginId, TimeSpan elapsed) =>
        new() { PluginId = pluginId, Elapsed = elapsed, Outcome = SandboxOutcome.Success };

    internal static SandboxResult TimedOut(Guid pluginId, TimeSpan elapsed) =>
        new() { PluginId = pluginId, Elapsed = elapsed, Outcome = SandboxOutcome.TimedOut };

    internal static SandboxResult Faulted(Guid pluginId, TimeSpan elapsed, Exception ex) =>
        new() { PluginId = pluginId, Elapsed = elapsed, Outcome = SandboxOutcome.Faulted, Exception = ex };
}

/// <summary>
/// Encapsulates the result of a sandboxed plugin execution with a return value.
/// </summary>
/// <typeparam name="T">Type of the returned value.</typeparam>
public sealed class SandboxResult<T>
{
    /// <summary>Gets the plugin that was executed.</summary>
    public Guid PluginId { get; init; }

    /// <summary>Gets the wall-clock duration of the execution.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Gets the outcome of the execution.</summary>
    public SandboxOutcome Outcome { get; init; }

    /// <summary>Gets the returned value if execution succeeded.</summary>
    public T? Value { get; init; }

    /// <summary>Gets the exception if the execution faulted.</summary>
    public Exception? Exception { get; init; }

    internal static SandboxResult<T> Success(Guid pluginId, TimeSpan elapsed, T value) =>
        new() { PluginId = pluginId, Elapsed = elapsed, Outcome = SandboxOutcome.Success, Value = value };

    internal static SandboxResult<T> TimedOut(Guid pluginId, TimeSpan elapsed) =>
        new() { PluginId = pluginId, Elapsed = elapsed, Outcome = SandboxOutcome.TimedOut };

    internal static SandboxResult<T> Faulted(Guid pluginId, TimeSpan elapsed, Exception ex) =>
        new() { PluginId = pluginId, Elapsed = elapsed, Outcome = SandboxOutcome.Faulted, Exception = ex };
}

/// <summary>
/// Possible outcomes of a sandboxed execution.
/// </summary>
public enum SandboxOutcome
{
    /// <summary>Execution completed normally.</summary>
    Success,

    /// <summary>Execution exceeded the configured timeout.</summary>
    TimedOut,

    /// <summary>Execution threw an unhandled exception.</summary>
    Faulted
}
