#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Runtime.CompilerServices;
using FluentAssertions;
using PluginEngine.Results;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Extension methods for testing <see cref="PluginOperationResult"/> instances.
/// </summary>
public static class PluginOperationResultTestsExtensions
{
    /// <summary>
    /// Asserts that a plugin operation result is successful and has the expected message.
    /// </summary>
    /// <param name="result">The result to assert. Cannot be null.</param>
    /// <param name="expectedMessage">The expected success message. Cannot be null or empty.</param>
    /// <param name="because">Optional reason phrase explaining the assertion.</param>
    /// <param name="testMethod">The test method name for better error messages.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="expectedMessage"/> is null or empty.</exception>
    public static void ShouldBeSuccessfulWithMessage(
        this PluginOperationResult result,
        string expectedMessage,
        string? because = null,
        [CallerMemberName] string? testMethod = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrEmpty(expectedMessage);

        result.Should().NotBeNull($"because test method '{testMethod}' passed a null result");
        result.Success.Should().BeTrue($"because {testMethod} expected success but was failure" +
            (because != null ? $" ({because})" : string.Empty));
        result.Message.Should().Be(expectedMessage, because: $"because {testMethod} expected message '{expectedMessage}'");
        result.ErrorCode.Should().BeNull($"because {testMethod} expected no error code for successful result");
    }

    /// <summary>
    /// Asserts that a plugin operation result is a failure with the expected error code.
    /// </summary>
    /// <param name="result">The result to assert. Cannot be null.</param>
    /// <param name="expectedErrorCode">The expected error code.</param>
    /// <param name="because">Optional reason phrase explaining the assertion.</param>
    /// <param name="testMethod">The test method name for better error messages.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
    public static void ShouldBeFailureWithErrorCode(
        this PluginOperationResult result,
        int expectedErrorCode,
        string? because = null,
        [CallerMemberName] string? testMethod = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.Should().NotBeNull($"because test method '{testMethod}' passed a null result");
        result.Success.Should().BeFalse($"because {testMethod} expected failure but was success" +
            (because != null ? $" ({because})" : string.Empty));
        result.ErrorCode.Should().Be(expectedErrorCode, because: $"because {testMethod} expected error code {expectedErrorCode}");
    }

    /// <summary>
    /// Asserts that a plugin operation result has the expected duration.
    /// </summary>
    /// <param name="result">The result to assert. Cannot be null.</param>
    /// <param name="expectedDurationMs">The expected duration in milliseconds.</param>
    /// <param name="because">Optional reason phrase explaining the assertion.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
    public static void ShouldHaveDurationMs(
        this PluginOperationResult result,
        int expectedDurationMs,
        string? because = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.Should().NotBeNull("result should not be null");
        result.DurationMs.Should().Be(expectedDurationMs, because: $"expected duration of {expectedDurationMs}ms" +
            (because != null ? $" ({because})" : string.Empty));
    }

    /// <summary>
    /// Creates a plugin operation result with the specified data and asserts it's successful.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="data">The data to include in the result.</param>
    /// <param name="expectedMessage">The expected success message. Cannot be null or empty.</param>
    /// <returns>The created result for further assertions.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="expectedMessage"/> is null or empty.</exception>
    public static PluginOperationResult<T> CreateAndAssertSuccess<T>(
        this PluginOperationResultTests _,
        T data,
        string expectedMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(expectedMessage);

        var result = PluginOperationResult<T>.CreateSuccess(data, expectedMessage);
        result.ShouldBeSuccessfulWithMessage(expectedMessage);
        result.Data.Should().Be(data, "because data should be preserved in successful result");
        return result;
    }

    /// <summary>
    /// Creates a plugin operation result failure and asserts it's a failure with the specified error code.
    /// </summary>
    /// <param name="errorCode">The error code to set.</param>
    /// <param name="expectedMessage">The expected error message. Cannot be null or empty.</param>
    /// <param name="details">Optional error details.</param>
    /// <returns>The created result for further assertions.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="expectedMessage"/> is null or empty.</exception>
    public static PluginOperationResult CreateAndAssertFailure(
        this PluginOperationResultTests _,
        int errorCode,
        string expectedMessage,
        string? details = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(expectedMessage);

        var result = PluginOperationResult.CreateFailure(expectedMessage, errorCode, details);
        result.ShouldBeFailureWithErrorCode(errorCode);
        result.Message.Should().Be(expectedMessage);
        if (details != null)
        {
            result.ErrorDetails.Should().Be(details);
        }
        return result;
    }

    /// <summary>
    /// Asserts that a generic plugin operation result has the expected data.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="result">The result to assert. Cannot be null.</param>
    /// <param name="expectedData">The expected data value.</param>
    /// <param name="because">Optional reason phrase explaining the assertion.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
    public static void ShouldHaveData<T>(
        this PluginOperationResult<T> result,
        T expectedData,
        string? because = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.Should().NotBeNull("result should not be null");
        result.Data.Should().Be(expectedData, because: $"expected data value" +
            (because != null ? $" ({because})" : string.Empty));
    }

    /// <summary>
    /// Asserts that a batch result has the expected success and failure counts.
    /// </summary>
    /// <param name="batch">The batch result to assert. Cannot be null.</param>
    /// <param name="expectedSuccessCount">Expected number of successful operations.</param>
    /// <param name="expectedFailureCount">Expected number of failed operations.</param>
    /// <param name="because">Optional reason phrase explaining the assertion.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="batch"/> is null.</exception>
    public static void ShouldHaveCounts(
        this PluginBatchOperationResult batch,
        int expectedSuccessCount,
        int expectedFailureCount,
        string? because = null)
    {
        ArgumentNullException.ThrowIfNull(batch);

        batch.Should().NotBeNull("batch should not be null");
        batch.SuccessCount.Should().Be(expectedSuccessCount, because: $"expected {expectedSuccessCount} successes" +
            (because != null ? $" ({because})" : string.Empty));
        batch.FailureCount.Should().Be(expectedFailureCount, because: $"expected {expectedFailureCount} failures");
        batch.TotalCount.Should().Be(expectedSuccessCount + expectedFailureCount);
    }

    /// <summary>
    /// Asserts that a batch result contains the expected number of results.
    /// </summary>
    /// <param name="batch">The batch result to assert. Cannot be null.</param>
    /// <param name="expectedCount">The expected number of results.</param>
    /// <param name="because">Optional reason phrase explaining the assertion.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="batch"/> is null.</exception>
    public static void ShouldHaveResultCount(
        this PluginBatchOperationResult batch,
        int expectedCount,
        string? because = null)
    {
        ArgumentNullException.ThrowIfNull(batch);

        batch.Should().NotBeNull("batch should not be null");
        batch.Results.Should().HaveCount(expectedCount, because: $"expected {expectedCount} results in batch" +
            (because != null ? $" ({because})" : string.Empty));
        batch.TotalCount.Should().Be(expectedCount);
    }

    /// <summary>
    /// Creates a new batch result, adds multiple results, and returns the batch for assertions.
    /// </summary>
    /// <param name="results">The results to add to the batch. Cannot be null.</param>
    /// <returns>The populated batch for further assertions.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="results"/> is null.</exception>
    public static PluginBatchOperationResult CreateBatchWithResults(
        this PluginOperationResultTests _,
        params PluginOperationResult[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var batch = new PluginBatchOperationResult();
        foreach (var result in results)
        {
            batch.AddResult(Guid.NewGuid(), "TestPlugin", result);
        }
        return batch;
    }

    /// <summary>
    /// Asserts that a result contains the expected error details.
    /// </summary>
    /// <param name="result">The result to assert. Cannot be null.</param>
    /// <param name="expectedDetails">The expected error details. Cannot be null.</param>
    /// <param name="because">Optional reason phrase explaining the assertion.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="expectedDetails"/> is null.</exception>
    public static void ShouldHaveErrorDetails(
        this PluginOperationResult result,
        string expectedDetails,
        string? because = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(expectedDetails);

        result.Should().NotBeNull("result should not be null");
        result.ErrorDetails.Should().Be(expectedDetails, because: $"expected error details" +
            (because != null ? $" ({because})" : string.Empty));
    }

    /// <summary>
    /// Asserts that a result has the expected total duration.
    /// </summary>
    /// <param name="batch">The batch result to assert. Cannot be null.</param>
    /// <param name="expectedDurationMs">The expected total duration in milliseconds.</param>
    /// <param name="because">Optional reason phrase explaining the assertion.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="batch"/> is null.</exception>
    public static void ShouldHaveTotalDurationMs(
        this PluginBatchOperationResult batch,
        int expectedDurationMs,
        string? because = null)
    {
        ArgumentNullException.ThrowIfNull(batch);

        batch.Should().NotBeNull("batch should not be null");
        batch.TotalDurationMs.Should().Be(expectedDurationMs, because: $"expected total duration of {expectedDurationMs}ms" +
            (because != null ? $" ({because})" : string.Empty));
    }
}