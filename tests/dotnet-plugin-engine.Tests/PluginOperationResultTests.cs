// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using PluginEngine.Exceptions;
using PluginEngine.Results;
using Xunit;

namespace PluginEngine.Tests;

public class PluginOperationResultTests
{
    // ── PluginOperationResult ──────────────────────────────────────────

    [Fact]
    public void CreateSuccess_WithMessage_SetsSuccessTrueAndPreservesMessage()
    {
        var result = PluginOperationResult.CreateSuccess("Plugin activated.", durationMs: 42);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Plugin activated.");
        result.DurationMs.Should().Be(42);
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void CreateFailure_WithCustomErrorCode_SetsSuccessFalseAndErrorCode()
    {
        var result = PluginOperationResult.CreateFailure("Load failed.", errorCode: 404, details: "Assembly not found.");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(404);
        result.ErrorDetails.Should().Be("Assembly not found.");
        result.Message.Should().Be("Load failed.");
    }

    [Fact]
    public void FromException_WithPluginLoadException_ReturnsErrorCode1001()
    {
        var ex = new PluginLoadException("Failed to load assembly.", "MyPlugin", "/plugins/my.dll", PluginLoadStage.AssemblyResolution);

        var result = PluginOperationResult.FromException(ex);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(1001);
        result.Message.Should().Be(ex.Message);
    }

    [Fact]
    public void FromException_WithDependencyResolutionException_ReturnsErrorCode1002()
    {
        var ex = new DependencyResolutionException("Cannot resolve dep.", DependencyResolutionReason.DependencyNotFound);

        var result = PluginOperationResult.FromException(ex);

        result.ErrorCode.Should().Be(1002);
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void FromException_WithUnknownException_ReturnsDefaultErrorCode500()
    {
        var ex = new InvalidOperationException("Something went wrong.");

        var result = PluginOperationResult.FromException(ex, durationMs: 10);

        result.ErrorCode.Should().Be(500);
        result.DurationMs.Should().Be(10);
    }

    [Fact]
    public void FromException_WithInnerException_IncludesInnerExceptionMessageInDetails()
    {
        var inner = new FileNotFoundException("dll missing");
        var ex = new PluginLoadException("Outer failure.", inner);

        var result = PluginOperationResult.FromException(ex);

        result.ErrorDetails.Should().Be(inner.Message);
    }

    // ── PluginOperationResult<T> ───────────────────────────────────────

    [Fact]
    public void Generic_CreateSuccess_WithData_SetsBothSuccessAndData()
    {
        var data = new { Name = "AuthPlugin", Version = "2.0.0" };

        var result = PluginOperationResult<object>.CreateSuccess(data, "Plugin loaded.");

        result.Success.Should().BeTrue();
        result.Data.Should().Be(data);
        result.Message.Should().Be("Plugin loaded.");
    }

    [Fact]
    public void Generic_CreateFailure_LeavesDataNull()
    {
        var result = PluginOperationResult<string>.CreateFailure("Failed.", errorCode: 500);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
    }

    // ── PluginBatchOperationResult ─────────────────────────────────────

    [Fact]
    public void BatchResult_AddResult_IncrementsSuccessAndFailureCountsCorrectly()
    {
        var batch = new PluginBatchOperationResult();
        var pluginId1 = Guid.NewGuid();
        var pluginId2 = Guid.NewGuid();

        batch.AddResult(pluginId1, "PluginA", PluginOperationResult.CreateSuccess("ok"));
        batch.AddResult(pluginId2, "PluginB", PluginOperationResult.CreateFailure("err"));

        batch.SuccessCount.Should().Be(1);
        batch.FailureCount.Should().Be(1);
        batch.Results.Should().HaveCount(2);
    }

    [Fact]
    public void BatchResult_IsSuccessful_WhenAllSucceed_ReturnsTrue()
    {
        var batch = new PluginBatchOperationResult();
        batch.AddResult(Guid.NewGuid(), "A", PluginOperationResult.CreateSuccess("ok"));
        batch.AddResult(Guid.NewGuid(), "B", PluginOperationResult.CreateSuccess("ok"));

        batch.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void BatchResult_IsSuccessful_WhenMoreFailuresThanSuccesses_ReturnsFalse()
    {
        var batch = new PluginBatchOperationResult();
        batch.AddResult(Guid.NewGuid(), "A", PluginOperationResult.CreateSuccess("ok"));
        batch.AddResult(Guid.NewGuid(), "B", PluginOperationResult.CreateFailure("err"));
        batch.AddResult(Guid.NewGuid(), "C", PluginOperationResult.CreateFailure("err"));

        batch.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public void BatchResult_GetSummary_ContainsCountsAndTiming()
    {
        var batch = new PluginBatchOperationResult { TotalDurationMs = 250 };
        batch.AddResult(Guid.NewGuid(), "A", PluginOperationResult.CreateSuccess("ok"));
        batch.AddResult(Guid.NewGuid(), "B", PluginOperationResult.CreateFailure("err"));

        var summary = batch.GetSummary();

        summary.Should().Contain("1").And.Contain("250ms");
    }
}
