#nullable enable
using FluentAssertions;
using PluginEngine.Domain.Entities;
using PluginEngine.Execution;
using Xunit;

namespace PluginEngine.Tests;

public sealed class PluginExecutionContextTests
{
    private static Plugin MakePlugin(string name = "TestPlugin") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Version = "1.0.0",
        AssemblyPath = "/plugins/test.dll"
    };

    private static PluginExecutionContext MakeContext(string op = "Load") => new()
    {
        Plugin = MakePlugin(),
        OperationType = op
    };

    // ── Initial state ───────────────────────────────────────────────────────

    [Fact]
    public void NewContext_HasUniqueExecutionId()
    {
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        ctx1.ExecutionId.Should().NotBe(ctx2.ExecutionId);
    }

    [Fact]
    public void NewContext_StateIsRunning()
    {
        var ctx = MakeContext();

        ctx.State.Should().Be(ExecutionState.Running);
    }

    [Fact]
    public void NewContext_StartedAtUtcIsRecentlySet()
    {
        var before = DateTime.UtcNow;
        var ctx = MakeContext();
        var after = DateTime.UtcNow;

        ctx.StartedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void NewContext_CompletedAtUtcIsNull()
    {
        var ctx = MakeContext();

        ctx.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void NewContext_DataDictionaryIsEmpty()
    {
        var ctx = MakeContext();

        ctx.Data.Should().BeEmpty();
    }

    // ── CompleteSuccess ─────────────────────────────────────────────────────

    [Fact]
    public void CompleteSuccess_SetsStateToCompleted()
    {
        var ctx = MakeContext();

        ctx.CompleteSuccess();

        ctx.State.Should().Be(ExecutionState.Completed);
    }

    [Fact]
    public void CompleteSuccess_SetsCompletedAtUtc()
    {
        var ctx = MakeContext();

        ctx.CompleteSuccess();

        ctx.CompletedAtUtc.Should().NotBeNull()
            .And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void CompleteSuccess_WithResult_StoresResult()
    {
        var ctx = MakeContext();
        var result = new { Status = "ok" };

        ctx.CompleteSuccess(result);

        ctx.Result.Should().BeSameAs(result);
    }

    [Fact]
    public void CompleteSuccess_DurationIsPositive()
    {
        var ctx = MakeContext();

        ctx.CompleteSuccess();

        ctx.Duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    // ── CompleteFailed ──────────────────────────────────────────────────────

    [Fact]
    public void CompleteFailed_SetsStateToFailed()
    {
        var ctx = MakeContext();

        ctx.CompleteFailed(new InvalidOperationException("oops"));

        ctx.State.Should().Be(ExecutionState.Failed);
    }

    [Fact]
    public void CompleteFailed_StoresException()
    {
        var ctx = MakeContext();
        var ex = new InvalidOperationException("test error");

        ctx.CompleteFailed(ex);

        ctx.Exception.Should().BeSameAs(ex);
    }

    [Fact]
    public void CompleteFailed_SetsCompletedAtUtc()
    {
        var ctx = MakeContext();

        ctx.CompleteFailed(new Exception("err"));

        ctx.CompletedAtUtc.Should().NotBeNull();
    }

    // ── Cancel ──────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_SetsStateToCancelled()
    {
        var ctx = MakeContext();

        ctx.Cancel();

        ctx.State.Should().Be(ExecutionState.Cancelled);
    }

    [Fact]
    public void Cancel_SetsCompletedAtUtc()
    {
        var ctx = MakeContext();

        ctx.Cancel();

        ctx.CompletedAtUtc.Should().NotBeNull();
    }

    // ── GetSummary ──────────────────────────────────────────────────────────

    [Fact]
    public void GetSummary_AfterCompleteSuccess_IsSuccessfulTrue()
    {
        var ctx = MakeContext("Execute");
        ctx.CompleteSuccess();

        var summary = ctx.GetSummary();

        summary.IsSuccessful.Should().BeTrue();
        summary.OperationType.Should().Be("Execute");
    }

    [Fact]
    public void GetSummary_AfterCompleteFailed_IsSuccessfulFalse()
    {
        var ctx = MakeContext();
        ctx.CompleteFailed(new Exception("boom"));

        var summary = ctx.GetSummary();

        summary.IsSuccessful.Should().BeFalse();
        summary.ErrorMessage.Should().Be("boom");
    }

    [Fact]
    public void GetSummary_ContainsPluginName()
    {
        var ctx = MakeContext();
        ctx.CompleteSuccess();

        var summary = ctx.GetSummary();

        summary.PluginName.Should().Be(ctx.Plugin.Name);
    }

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

    [Fact]
    public void Metrics_ArePresentOnNewContext()
    {
        var ctx = MakeContext();

        ctx.Metrics.Should().NotBeNull();
    }

    [Fact]
    public void Metrics_CustomMetricsDictionaryIsEmpty()
    {
        var ctx = MakeContext();

        ctx.Metrics.CustomMetrics.Should().BeEmpty();
    }
}
