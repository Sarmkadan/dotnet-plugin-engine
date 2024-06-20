#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using PluginEngine.Execution;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Extension methods for <see cref="PluginLifecycleTests"/> that provide additional test scenarios
/// and helper methods for working with plugin lifecycle hooks.
/// </summary>
public static class PluginLifecycleTestsExtensions
{
    /// <summary>
    /// Verifies that lifecycle hooks properly handle null cancellation tokens.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static async Task Hooks_HandleNullCancellationToken(this PluginLifecycleTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var lifecycle = new PluginLifecycleTests.RecordingLifecycle();

        // Call hooks with null cancellation token (should use default)
        await lifecycle.OnBeforeLoadAsync(null);
        await lifecycle.OnAfterLoadAsync(null);
        await lifecycle.OnBeforeUnloadAsync(null);
        await lifecycle.OnAfterUnloadAsync(null);

        lifecycle.CallLog.Should().HaveCount(4);
        lifecycle.CallLog.Should().ContainInOrder(
            nameof(IPluginLifecycle.OnBeforeLoadAsync),
            nameof(IPluginLifecycle.OnAfterLoadAsync),
            nameof(IPluginLifecycle.OnBeforeUnloadAsync),
            nameof(IPluginLifecycle.OnAfterUnloadAsync));
    }

    /// <summary>
    /// Tests that the lifecycle hooks can be called multiple times without side effects.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static async Task Hooks_CanBeCalledMultipleTimes(this PluginLifecycleTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var lifecycle = new PluginLifecycleTests.RecordingLifecycle();

        // First call sequence
        await lifecycle.OnBeforeLoadAsync();
        await lifecycle.OnAfterLoadAsync();
        await lifecycle.OnBeforeUnloadAsync();
        await lifecycle.OnAfterUnloadAsync();

        // Second call sequence - should work without issues
        await lifecycle.OnBeforeLoadAsync();
        await lifecycle.OnAfterLoadAsync();
        await lifecycle.OnBeforeUnloadAsync();
        await lifecycle.OnAfterUnloadAsync();

        lifecycle.CallLog.Should().HaveCount(8);
        lifecycle.CallLog.Should().ContainInOrder(
            nameof(IPluginLifecycle.OnBeforeLoadAsync),
            nameof(IPluginLifecycle.OnAfterLoadAsync),
            nameof(IPluginLifecycle.OnBeforeUnloadAsync),
            nameof(IPluginLifecycle.OnAfterUnloadAsync),
            nameof(IPluginLifecycle.OnBeforeLoadAsync),
            nameof(IPluginLifecycle.OnAfterLoadAsync),
            nameof(IPluginLifecycle.OnBeforeUnloadAsync),
            nameof(IPluginLifecycle.OnAfterUnloadAsync));
    }

    /// <summary>
    /// Verifies that lifecycle hooks maintain correct state across multiple invocations.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static async Task Hooks_MaintainStateAcrossInvocations(this PluginLifecycleTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var lifecycle = new PluginLifecycleTests.RecordingLifecycle();

        // Load sequence
        await lifecycle.OnBeforeLoadAsync();
        await lifecycle.OnAfterLoadAsync();

        // Unload sequence
        await lifecycle.OnBeforeUnloadAsync();
        await lifecycle.OnAfterUnloadAsync();

        // Load again to verify state is reset
        await lifecycle.OnBeforeLoadAsync();
        await lifecycle.OnAfterLoadAsync();

        lifecycle.CallLog.Should().HaveCount(6);
        lifecycle.CallLog.Should().Equal(
            nameof(IPluginLifecycle.OnBeforeLoadAsync),
            nameof(IPluginLifecycle.OnAfterLoadAsync),
            nameof(IPluginLifecycle.OnBeforeUnloadAsync),
            nameof(IPluginLifecycle.OnAfterUnloadAsync),
            nameof(IPluginLifecycle.OnBeforeLoadAsync),
            nameof(IPluginLifecycle.OnAfterLoadAsync));
    }

    /// <summary>
    /// Tests that cancellation tokens are properly respected during hook execution.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static async Task Hooks_RespectCancellationTokens(this PluginLifecycleTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        using var cts = new CancellationTokenSource();
        var lifecycle = new PluginLifecycleTests.RecordingLifecycle();

        // Cancel before any hooks are called
        cts.Cancel();

        // Hooks should still be callable but respect the cancellation
        await lifecycle.OnBeforeLoadAsync(cts.Token);
        await lifecycle.OnAfterLoadAsync(cts.Token);
        await lifecycle.OnBeforeUnloadAsync(cts.Token);
        await lifecycle.OnAfterUnloadAsync(cts.Token);

        lifecycle.CallLog.Should().HaveCount(4);
        lifecycle.CallLog.Should().ContainInOrder(
            nameof(IPluginLifecycle.OnBeforeLoadAsync),
            nameof(IPluginLifecycle.OnAfterLoadAsync),
            nameof(IPluginLifecycle.OnBeforeUnloadAsync),
            nameof(IPluginLifecycle.OnAfterUnloadAsync));
    }
}