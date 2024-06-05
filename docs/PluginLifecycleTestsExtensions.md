# PluginLifecycleTestsExtensions

Provides extension methods for testing plugin lifecycle hooks in the `dotnet-plugin-engine` project. These methods verify the behavior of plugin lifecycle hooks under various conditions, including cancellation, multiple invocations, and state persistence.

## API

### `Hooks_HandleNullCancellationToken`

Verifies that plugin lifecycle hooks handle a `null` cancellation token gracefully without throwing exceptions. The test ensures that the hook execution completes successfully even when no cancellation mechanism is provided.

| Parameter | Type | Description |
|-----------|------|-------------|
| `plugin` | `IPlugin` | The plugin instance under test. |
| `context` | `IPluginContext` | The plugin execution context. |

**Returns:** `Task` representing the asynchronous test execution.
**Throws:** Does not throw exceptions when the cancellation token is `null`.

---

### `Hooks_CanBeCalledMultipleTimes`

Validates that plugin lifecycle hooks can be invoked repeatedly without side effects or state corruption. The test ensures idempotent behavior across multiple invocations.

| Parameter | Type | Description |
|-----------|------|-------------|
| `plugin` | `IPlugin` | The plugin instance under test. |
| `context` | `IPluginContext` | The plugin execution context. |

**Returns:** `Task` representing the asynchronous test execution.
**Throws:** Throws if the hook behaves differently on subsequent calls.

---

### `Hooks_MaintainStateAcrossInvocations`

Checks that plugin lifecycle hooks preserve internal state between invocations. The test ensures that state modifications in one invocation are visible in subsequent calls.

| Parameter | Type | Description |
|-----------|------|-------------|
| `plugin` | `IPlugin` | The plugin instance under test. |
| `context` | `IPluginContext` | The plugin execution context. |

**Returns:** `Task` representing the asynchronous test execution.
**Throws:** Throws if state is not preserved between invocations.

---
### `Hooks_RespectCancellationTokens`

Ensures that plugin lifecycle hooks respect cancellation tokens and terminate execution when requested. The test verifies that cancellation is detected and handled appropriately.

| Parameter | Type | Description |
|-----------|------|-------------|
| `plugin` | `IPlugin` | The plugin instance under test. |
| `context` | `IPluginContext` | The plugin execution context. |
| `cancellationToken` | `CancellationToken` | The token used to signal cancellation. |

**Returns:** `Task` representing the asynchronous test execution.
**Throws:** Throws if the hook does not terminate when cancellation is requested.

## Usage

### Example 1: Testing cancellation handling
```csharp
var plugin = new SamplePlugin();
var context = new PluginContext();
var cancellationToken = new CancellationTokenSource().Token;

await PluginLifecycleTestsExtensions.Hooks_HandleNullCancellationToken(plugin, context);
await PluginLifecycleTestsExtensions.Hooks_RespectCancellationTokens(plugin, context, cancellationToken);
```

### Example 2: Validating state persistence
```csharp
var plugin = new StatefulPlugin();
var context = new PluginContext();

await PluginLifecycleTestsExtensions.Hooks_MaintainStateAcrossInvocations(plugin, context);
await PluginLifecycleTestsExtensions.Hooks_CanBeCalledMultipleTimes(plugin, context);
```

## Notes

- **Cancellation tokens:** Tests assume that the plugin lifecycle hooks check `CancellationToken.IsCancellationRequested` or `ThrowIfCancellationRequested()` explicitly. A `null` token should not cause failures, but implementations must handle it defensively.
- **Thread safety:** These methods are not thread-safe by design. They assume single-threaded test execution. Concurrent invocations may lead to race conditions in stateful plugins.
- **Stateful plugins:** Plugins relying on mutable state should ensure thread-safe state management if used in multi-threaded scenarios outside these tests.
- **Idempotency:** The `Hooks_CanBeCalledMultipleTimes` test assumes that the hook does not perform destructive operations on subsequent calls. Plugins violating this assumption will fail the test.
