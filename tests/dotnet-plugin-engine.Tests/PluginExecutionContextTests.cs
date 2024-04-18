#nullable enable
using FluentAssertions;
using PluginEngine.Domain.Entities;
using PluginEngine.Execution;
using Xunit;

/// <summary>
/// Tests for the PluginExecutionContext class.
/// </summary>
public sealed class PluginExecutionContextTests
{
    /// <summary>
    /// Creates a new Plugin instance with the specified name.
    /// </summary>
    /// <param name="name">The name of the plugin.</param>
    /// <returns>A new Plugin instance.</returns>
    private static Plugin MakePlugin(string name = "TestPlugin") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Version = "1.0.0",
        AssemblyPath = "/plugins/test.dll"
    };

    /// <summary>
    /// Creates a new PluginExecutionContext instance with the specified operation type.
    /// </summary>
    /// <param name="op">The operation type.</param>
    /// <returns>A new PluginExecutionContext instance.</returns>
    private static PluginExecutionContext MakeContext(string op = "Load") => new()
    {
        Plugin = MakePlugin(),
        OperationType = op
    };

    // ── Initial state ───────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a new PluginExecutionContext instance has a unique execution ID.
    /// </summary>
    [Fact]
    public void NewContext_HasUniqueExecutionId()
    {
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        ctx1.ExecutionId.Should().NotBe(ctx2.ExecutionId);
    }

    /// <summary>
    /// Verifies that a new PluginExecutionContext instance is in the Running state.
    /// </summary>
    [Fact]
    public void NewContext_StateIsRunning()
    {
        var ctx = MakeContext();

        ctx.State.Should().Be(ExecutionState.Running);
    }

    /// <summary>
    /// Verifies that a new PluginExecutionContext instance has a recently set StartedAtUtc value.
    /// </summary>
    [Fact]
    public void NewContext_StartedAtUtcIsRecentlySet()
    {
        var before = DateTime.UtcNow;
        var ctx = MakeContext();
        var after = DateTime.UtcNow;

        ctx.StartedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    /// <summary>
    /// Verifies that a new PluginExecutionContext instance has a null CompletedAtUtc value.
    /// </summary>
    [Fact]
    public void NewContext_CompletedAtUtcIsNull()
    {
        var ctx = MakeContext();

        ctx.CompletedAtUtc.Should().BeNull();
    }

    /// <summary>
    /// Verifies that a new PluginExecutionContext instance has an empty Data dictionary.
    /// </summary>
    [Fact]
    public void NewContext_DataDictionaryIsEmpty()
    {
        var ctx = MakeContext();

        ctx.Data.Should().BeEmpty();
    }

    // ── CompleteSuccess ─────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that calling CompleteSuccess on a PluginExecutionContext instance sets its state to Completed.
    /// </summary>
    [Fact]
    public void CompleteSuccess_SetsStateToCompleted()
    {
        var ctx = MakeContext();

        ctx.CompleteSuccess();

        ctx.State.Should().Be(ExecutionState.Completed);
    }

    /// <summary>
    /// Verifies that calling CompleteSuccess on a PluginExecutionContext instance sets its CompletedAtUtc value.
    /// </summary>
    [Fact]
    public void CompleteSuccess_SetsCompletedAtUtc()
    {
        var ctx = MakeContext();

        ctx.CompleteSuccess();

        ctx.CompletedAtUtc.Should().NotBeNull()
            .And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Verifies that calling CompleteSuccess on a PluginExecutionContext instance stores the result.
    /// </summary>
    /// <param name="result">The result to store.</param>
    [Fact]
    public void CompleteSuccess_WithResult_StoresResult()
    {
        var ctx = MakeContext();
        var result = new { Status = "ok" };

        ctx.CompleteSuccess(result);

        ctx.Result.Should().BeSameAs(result);
    }

    /// <summary>
    /// Verifies that calling CompleteSuccess on a PluginExecutionContext instance sets its Duration value to a positive value.
    /// </summary>
    [Fact]
    public void CompleteSuccess_DurationIsPositive()
    {
        var ctx = MakeContext();

        ctx.CompleteSuccess();

        ctx.Duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    // ── CompleteFailed ──────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that calling CompleteFailed on a PluginExecutionContext instance sets its state to Failed.
    /// </summary>
    /// <param name="ex">The exception to store.</param>
    [Fact]
    public void CompleteFailed_SetsStateToFailed()
    {
        var ctx = MakeContext();

        ctx.CompleteFailed(new InvalidOperationException("oops"));

        ctx.State.Should().Be(ExecutionState.Failed);
    }

    /// <summary>
    /// Verifies that calling CompleteFailed on a PluginExecutionContext instance stores the exception.
    /// </summary>
    /// <param name="ex">The exception to store.</param>
    [Fact]
    public void CompleteFailed_StoresException()
    {
        var ctx = MakeContext();
        var ex = new InvalidOperationException("test error");

        ctx.CompleteFailed(ex);

        ctx.Exception.Should().BeSameAs(ex);
    }

    /// <summary>
    /// Verifies that calling CompleteFailed on a PluginExecutionContext instance sets its CompletedAtUtc value.
    /// </summary>
    [Fact]
    public void CompleteFailed_SetsCompletedAtUtc()
    {
        var ctx = MakeContext();

        ctx.CompleteFailed(new Exception("err"));

        ctx.CompletedAtUtc.Should().NotBeNull();
    }

    // ── Cancel ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that calling Cancel on a PluginExecutionContext instance sets its state to Cancelled.
    /// </summary>
    [Fact]
    public void Cancel_SetsStateToCancelled()
    {
        var ctx = MakeContext();

        ctx.Cancel();

        ctx.State.Should().Be(ExecutionState.Cancelled);
    }

    /// <summary>
    /// Verifies that calling Cancel on a PluginExecutionContext instance sets its CompletedAtUtc value.
    /// </summary>
    [Fact]
    public void Cancel_SetsCompletedAtUtc()
    {
        var ctx = MakeContext();

        ctx.Cancel();

        ctx.CompletedAtUtc.Should().NotBeNull();
    }

    // ── GetSummary ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that calling GetSummary on a PluginExecutionContext instance after CompleteSuccess returns a successful summary.
    /// </summary>
    [Fact]
    public void GetSummary_AfterCompleteSuccess_IsSuccessfulTrue()
    {
        var ctx = MakeContext("Execute");
        ctx.CompleteSuccess();

        var summary = ctx.GetSummary();

        summary.IsSuccessful.Should().BeTrue();
        summary.OperationType.Should().Be("Execute");
    }

    /// <summary>
    /// Verifies that calling GetSummary on a PluginExecutionContext instance after CompleteFailed returns a failed summary.
    /// </summary>
    [Fact]
    public void GetSummary_AfterCompleteFailed_IsSuccessfulFalse()
    {
        var ctx = MakeContext();
        ctx.CompleteFailed(new Exception("boom"));

        var summary = ctx.GetSummary();

        summary.IsSuccessful.Should().BeFalse();
        summary.ErrorMessage.Should().Be("boom");
    }

    /// <summary>
    /// Verifies that calling GetSummary on a PluginExecutionContext instance returns a summary with the plugin name.
    /// </summary>
    [Fact]
    public void GetSummary_ContainsPluginName()
    {
        var ctx = MakeContext();
        ctx.CompleteSuccess();

        var summary = ctx.GetSummary();

        summary.PluginName.Should().Be(ctx.Plugin.Name);
    }

    /// <summary>
    /// Verifies that calling GetSummary on a PluginExecutionContext instance returns a summary with key information in its ToString method.
    /// </summary>
    [Fact]
    public void GetSummary_ToStringContainsKeyInfo()
    {
        var ctx = MakeContext("Load");
        ctx.CompleteSuccess();

        var summary = ctx.GetSummary();
        var str = summary.ToString();

        str.Should().Contain("Load")
            .And.Contain("Success");
    }

    // ── Data dictionary ─────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a PluginExecutionContext instance can store arbitrary values in its Data dictionary.
    /// </summary>
    [Fact]
    public void Data_CanStoreArbitraryValues()
    {
        var ctx = MakeContext();
        ctx.Data["key"] = 42;
        ctx.Data["other"] = "hello";

        ctx.Data["key"].Should().Be(42);
        ctx.Data["other"].Should().Be("hello");
    }

    // ── Metrics ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a new PluginExecutionContext instance has a non-null Metrics object.
    /// </summary>
    [Fact]
    public void Metrics_ArePresentOnNewContext()
    {
        var ctx = MakeContext();

        ctx.Metrics.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that a new PluginExecutionContext instance has an empty CustomMetrics dictionary.
    /// </summary>
    [Fact]
    public void Metrics_CustomMetricsDictionaryIsEmpty()
    {
        var ctx = MakeContext();

        ctx.Metrics.CustomMetrics.Should().BeEmpty();
    }
}
