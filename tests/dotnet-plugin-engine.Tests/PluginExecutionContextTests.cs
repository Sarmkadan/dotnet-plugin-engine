// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using PluginEngine.Execution;

namespace PluginEngine.Tests;

public class PluginExecutionContextTests
{
    private PluginExecutionContext CreateContext() => new()
    {
        Plugin = new Plugin { Id = "test-plugin", Name = "Test", Version = "1.0.0" },
        OperationType = "Execute"
    };

    [Fact]
    public void Constructor_GeneratesUniqueExecutionId()
    {
        var ctx1 = CreateContext();
        var ctx2 = CreateContext();
        ctx1.ExecutionId.Should().NotBe(ctx2.ExecutionId);
    }

    [Fact]
    public void Constructor_SetsStartedAtToNow()
    {
        var before = DateTime.UtcNow;
        var ctx = CreateContext();
        ctx.StartedAtUtc.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Constructor_DefaultStateIsRunning()
    {
        var ctx = CreateContext();
        ctx.State.Should().Be(ExecutionState.Running);
    }

    [Fact]
    public void Constructor_DataDictionaryIsEmpty()
    {
        var ctx = CreateContext();
        ctx.Data.Should().BeEmpty();
    }

    [Fact]
    public void Duration_BeforeCompletion_ReturnsElapsedTime()
    {
        var ctx = CreateContext();
        Thread.Sleep(10);
        ctx.Duration.TotalMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CompleteSuccess_SetsCompletedState()
    {
        var ctx = CreateContext();
        ctx.CompleteSuccess("result-value");
        ctx.State.Should().Be(ExecutionState.Completed);
        ctx.CompletedAtUtc.Should().NotBeNull();
        ctx.Result.Should().Be("result-value");
    }

    [Fact]
    public void CompleteSuccess_NullResult_SetsCompletedWithNullResult()
    {
        var ctx = CreateContext();
        ctx.CompleteSuccess();
        ctx.State.Should().Be(ExecutionState.Completed);
        ctx.Result.Should().BeNull();
    }

    [Fact]
    public void Duration_AfterCompletion_IsFixed()
    {
        var ctx = CreateContext();
        Thread.Sleep(10);
        ctx.CompleteSuccess();
        var duration1 = ctx.Duration;
        Thread.Sleep(10);
        var duration2 = ctx.Duration;
        duration1.Should().Be(duration2);
    }

    [Fact]
    public void Data_CanStoreAndRetrieveValues()
    {
        var ctx = CreateContext();
        ctx.Data["key"] = "value";
        ctx.Data["key"].Should().Be("value");
    }

    [Fact]
    public void Exception_DefaultsToNull()
    {
        var ctx = CreateContext();
        ctx.Exception.Should().BeNull();
    }

    [Fact]
    public void Exception_CanBeSet()
    {
        var ctx = CreateContext();
        var ex = new InvalidOperationException("test");
        ctx.Exception = ex;
        ctx.Exception.Should().BeSameAs(ex);
    }
}
