# PluginLifecycleTests

`PluginLifecycleTests` is a test class that validates the lifecycle hook behavior defined by the `IPluginLifecycle` interface within the plugin engine. It ensures that asynchronous load and unload sequences invoke the `Before` and `After` hooks in the correct order, that required hooks are properly enforced, that the interface itself remains optional for plugin implementations, and that cancellation tokens are correctly forwarded through the lifecycle pipeline.

## API

### public Task OnBeforeLoadAsync
Represents the test-side implementation or mock of the `OnBeforeLoadAsync` lifecycle hook. Invoked immediately before a plugin is loaded. Returns a `Task` to support asynchronous preparation logic.

### public Task OnAfterLoadAsync
Represents the test-side implementation or mock of the `OnAfterLoadAsync` lifecycle hook. Invoked immediately after a plugin has been loaded. Returns a `Task` to support asynchronous post-load logic.

### public Task OnBeforeUnloadAsync
Represents the test-side implementation or mock of the `OnBeforeUnloadAsync` lifecycle hook. Invoked immediately before a plugin is unloaded. Returns a `Task` to support asynchronous pre-unload logic.

### public Task OnAfterUnloadAsync
Represents the test-side implementation or mock of the `OnAfterUnloadAsync` lifecycle hook. Invoked immediately after a plugin has been unloaded. Returns a `Task` to support asynchronous cleanup logic.

### public void IPluginLifecycle_HasRequiredLoadHooks
A test method that verifies the `IPluginLifecycle` interface declares the load-related hooks (`OnBeforeLoadAsync` and `OnAfterLoadAsync`) as required members. Throws an assertion exception if the interface definition does not mandate these hooks.

### public void IPluginLifecycle_HasRequiredUnloadHooks
A test method that verifies the `IPluginLifecycle` interface declares the unload-related hooks (`OnBeforeUnloadAsync` and `OnAfterUnloadAsync`) as required members. Throws an assertion exception if the interface definition does not mandate these hooks.

### public void IPluginLifecycle_AllHooksReturnTask
A test method that verifies every hook declared on `IPluginLifecycle` returns `Task`. Throws an assertion exception if any hook has a return type other than `Task`.

### public async Task LoadSequence_InvokesBeforeThenAfterLoad
An asynchronous test method that validates the load sequence ordering: `OnBeforeLoadAsync` must complete before `OnAfterLoadAsync` begins. Throws an assertion exception if the invocation order is reversed or if either hook is skipped.

### public async Task UnloadSequence_InvokesBeforeThenAfterUnload
An asynchronous test method that validates the unload sequence ordering: `OnBeforeUnloadAsync` must complete before `OnAfterUnloadAsync` begins. Throws an assertion exception if the invocation order is reversed or if either hook is skipped.

### public async Task FullLifecycle_LoadThenUnload_CallsHooksInCorrectOrder
An asynchronous test method that exercises the complete lifecycle by loading and then unloading a plugin. It asserts that all four hooks are called and that the global ordering is: `OnBeforeLoadAsync` → `OnAfterLoadAsync` → `OnBeforeUnloadAsync` → `OnAfterUnloadAsync`. Throws an assertion exception if any hook is missing or out of sequence.

### public void IPluginLifecycle_IsAnInterface_SoImplementationIsOptional
A test method that confirms `IPluginLifecycle` is defined as an interface, meaning plugin types are not forced to implement it unless they opt in. Throws an assertion exception if the type is not an interface or if the engine incorrectly requires implementation.

### public async Task RecordingLifecycle_CancellationToken_IsForwardedToHooks
An asynchronous test method that verifies the `CancellationToken` provided during a lifecycle operation is propagated to each hook. It typically uses a recording mechanism to capture the token received inside the hooks and asserts equality with the original token. Throws an assertion exception if any hook receives a different or default token.

## Usage

### Verifying a Plugin Implements the Full Lifecycle Correctly
```csharp
[TestMethod]
public async Task MyPlugin_FullLifecycle_HooksCalledInOrder()
{
    var test = new PluginLifecycleTests();
    // The test internally sets up a plugin that records hook invocations.
    await test.FullLifecycle_LoadThenUnload_CallsHooksInCorrectOrder();
    // If this completes without assertion failure, the lifecycle order is correct.
}
```

### Confirming CancellationToken Propagation in a Custom Host
```csharp
[TestMethod]
public async Task CustomHost_ForwardsCancellationToken_ToLifecycleHooks()
{
    var test = new PluginLifecycleTests();
    using var cts = new CancellationTokenSource();

    // This test method validates that the token reaches every hook.
    await test.RecordingLifecycle_CancellationToken_IsForwardedToHooks();

    // Additional host-specific assertions can follow.
    Assert.IsTrue(cts.IsCancellationRequested == false, "Token should not be cancelled prematurely.");
}
```

## Notes

- **Ordering enforcement**: The load and unload sequence tests rely on strict temporal ordering. Any concurrent execution of `Before` and `After` hooks within the same phase will cause assertion failures.
- **Thread safety**: The test methods themselves are not guaranteed to be thread-safe for parallel execution. They are designed to run sequentially within a test framework. The underlying lifecycle hooks under test are expected to be invoked sequentially by the plugin engine.
- **Cancellation token equality**: The `RecordingLifecycle_CancellationToken_IsForwardedToHooks` test expects reference or structural equality of the token. Wrapping or replacing the token inside the engine will break this test.
- **Interface optionality**: `IPluginLifecycle_IsAnInterface_SoImplementationIsOptional` guards against accidental conversion of the interface to an abstract base class or a required attribute-based contract. Any change that forces implementation will cause this test to fail.
- **Return type constraint**: `IPluginLifecycle_AllHooksReturnTask` ensures that no hook accidentally returns `ValueTask` or a non-awaitable type, preserving a uniform async pattern across the lifecycle.
