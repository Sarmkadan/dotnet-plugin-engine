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
/// Tests verifying that <see cref="IPluginLifecycle"/> hooks are invoked correctly
/// during plugin load and unload operations.
/// </summary>
public sealed class PluginLifecycleTests
{
    // ── Helper ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// A test double that records the order in which lifecycle methods are called.
    /// </summary>
    private sealed class RecordingLifecycle : IPluginLifecycle
    {
        private readonly List<string> _callLog = [];

        public IReadOnlyList<string> CallLog => _callLog;

        public Task OnBeforeLoadAsync(CancellationToken cancellationToken = default)
        {
            _callLog.Add(nameof(OnBeforeLoadAsync));
            return Task.CompletedTask;
        }

        public Task OnAfterLoadAsync(CancellationToken cancellationToken = default)
        {
            _callLog.Add(nameof(OnAfterLoadAsync));
            return Task.CompletedTask;
        }

        public Task OnBeforeUnloadAsync(CancellationToken cancellationToken = default)
        {
            _callLog.Add(nameof(OnBeforeUnloadAsync));
            return Task.CompletedTask;
        }

        public Task OnAfterUnloadAsync(CancellationToken cancellationToken = default)
        {
            _callLog.Add(nameof(OnAfterUnloadAsync));
            return Task.CompletedTask;
        }
    }

    // ── Interface contract ─────────────────────────────────────────────────────

    [Fact]
    public void IPluginLifecycle_HasRequiredLoadHooks()
    {
        var type = typeof(IPluginLifecycle);

        type.GetMethod(nameof(IPluginLifecycle.OnBeforeLoadAsync)).Should().NotBeNull(
            because: "plugins need a hook called before the load sequence completes");

        type.GetMethod(nameof(IPluginLifecycle.OnAfterLoadAsync)).Should().NotBeNull(
            because: "plugins need a hook called after the load sequence completes");
    }

    [Fact]
    public void IPluginLifecycle_HasRequiredUnloadHooks()
    {
        var type = typeof(IPluginLifecycle);

        type.GetMethod(nameof(IPluginLifecycle.OnBeforeUnloadAsync)).Should().NotBeNull(
            because: "plugins need a hook called before unloading begins");

        type.GetMethod(nameof(IPluginLifecycle.OnAfterUnloadAsync)).Should().NotBeNull(
            because: "plugins need a hook called after unloading completes");
    }

    [Fact]
    public void IPluginLifecycle_AllHooksReturnTask()
    {
        var methods = typeof(IPluginLifecycle).GetMethods();

        foreach (var method in methods)
        {
            method.ReturnType.Should().Be(typeof(Task),
                because: $"{method.Name} must return Task so it can be awaited");
        }
    }

    // ── Invocation order ───────────────────────────────────────────────────────

    [Fact]
    public async Task LoadSequence_InvokesBeforeThenAfterLoad()
    {
        var lifecycle = new RecordingLifecycle();

        // Simulate the loader calling hooks in the correct order
        await lifecycle.OnBeforeLoadAsync();
        await lifecycle.OnAfterLoadAsync();

        lifecycle.CallLog.Should().ContainInOrder(
            nameof(IPluginLifecycle.OnBeforeLoadAsync),
            nameof(IPluginLifecycle.OnAfterLoadAsync));
    }

    [Fact]
    public async Task UnloadSequence_InvokesBeforeThenAfterUnload()
    {
        var lifecycle = new RecordingLifecycle();

        // Simulate the loader calling hooks in the correct order
        await lifecycle.OnBeforeUnloadAsync();
        await lifecycle.OnAfterUnloadAsync();

        lifecycle.CallLog.Should().ContainInOrder(
            nameof(IPluginLifecycle.OnBeforeUnloadAsync),
            nameof(IPluginLifecycle.OnAfterUnloadAsync));
    }

    [Fact]
    public async Task FullLifecycle_LoadThenUnload_CallsHooksInCorrectOrder()
    {
        var lifecycle = new RecordingLifecycle();

        await lifecycle.OnBeforeLoadAsync();
        await lifecycle.OnAfterLoadAsync();
        await lifecycle.OnBeforeUnloadAsync();
        await lifecycle.OnAfterUnloadAsync();

        lifecycle.CallLog.Should().Equal(
            nameof(IPluginLifecycle.OnBeforeLoadAsync),
            nameof(IPluginLifecycle.OnAfterLoadAsync),
            nameof(IPluginLifecycle.OnBeforeUnloadAsync),
            nameof(IPluginLifecycle.OnAfterUnloadAsync));
    }

    // ── Optional implementation ────────────────────────────────────────────────

    [Fact]
    public void IPluginLifecycle_IsAnInterface_SoImplementationIsOptional()
    {
        // Plugins that do NOT implement IPluginLifecycle skip hooks entirely.
        // The loader uses reflection to discover implementors, so a plain plugin
        // class with no lifecycle interface is loaded without any hook invocation.
        typeof(IPluginLifecycle).IsInterface.Should().BeTrue(
            because: "making it an interface lets plugins opt-in without a base class requirement");
    }

    [Fact]
    public async Task RecordingLifecycle_CancellationToken_IsForwardedToHooks()
    {
        using var cts = new CancellationTokenSource();
        var lifecycle = new RecordingLifecycle();

        // Hooks must accept (and honour) a CancellationToken
        await lifecycle.OnBeforeLoadAsync(cts.Token);
        await lifecycle.OnAfterLoadAsync(cts.Token);
        await lifecycle.OnBeforeUnloadAsync(cts.Token);
        await lifecycle.OnAfterUnloadAsync(cts.Token);

        lifecycle.CallLog.Should().HaveCount(4);
    }
}
