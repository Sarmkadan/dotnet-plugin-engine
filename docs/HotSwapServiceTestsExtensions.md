# HotSwapServiceTestsExtensions

A static utility class providing factory methods and assertion helpers for testing the hot-swap plugin infrastructure. It centralizes the creation of test doubles, temporary assemblies, and service instances, and offers canned validation routines that verify swap outcomes, history tracking, and callback behavior without duplicating assertion logic across test suites.

## API

### CreateTestInstance

```csharp
public static HotSwapServiceTests CreateTestInstance()
```

Creates and returns a new instance of the `HotSwapServiceTests` test harness. This harness encapsulates the full lifecycle of a hot-swap scenario, including service creation, plugin loading, swap execution, and history inspection.

- **Returns:** A freshly initialized `HotSwapServiceTests` instance ready for test execution.
- **Throws:** May throw if any internal initialization step fails (e.g., missing dependencies).

### CreatePlugin

```csharp
public static Plugin CreatePlugin(string assemblyPath, string pluginTypeName)
```

Loads a plugin from the specified assembly path and instantiates the given type as a `Plugin`.

- **Parameters:**
  - `assemblyPath` — The full file path to the .NET assembly containing the plugin type.
  - `pluginTypeName` — The fully qualified type name of the plugin class to instantiate.
- **Returns:** A `Plugin` instance loaded from the assembly.
- **Throws:** `FileNotFoundException` if the assembly does not exist; `TypeLoadException` if the type cannot be resolved; `InvalidCastException` if the type does not derive from `Plugin`.

### CreateTempDll

```csharp
public static string CreateTempDll(string sourceCode, string assemblyName)
```

Compiles the provided C# source code into a temporary DLL file and returns its full path. The generated assembly is written to the system’s temporary directory and is suitable for use in plugin loading tests.

- **Parameters:**
  - `sourceCode` — A string containing valid C# source code for a plugin assembly.
  - `assemblyName` — The desired name for the output assembly (without extension).
- **Returns:** The absolute file path of the compiled temporary DLL.
- **Throws:** `CompilationException` (or similar) if the source code contains errors; `IOException` if the temporary file cannot be written.

### CreateService

```csharp
public static HotSwapService CreateService()
```

Constructs and returns a fully initialized `HotSwapService` instance configured for testing. The returned service is ready to accept plugin load and swap operations.

- **Returns:** A new `HotSwapService` instance.
- **Throws:** May throw if service dependencies cannot be resolved or if configuration is invalid.

### ShouldBeSuccessfulSwap

```csharp
public static void ShouldBeSuccessfulSwap(HotSwapServiceTests testInstance)
```

Asserts that the most recent swap operation performed on the given test instance completed successfully. This method inspects the outcome recorded in the test harness and fails the test if the swap was not successful.

- **Parameters:**
  - `testInstance` — The `HotSwapServiceTests` instance whose last swap outcome is to be validated.
- **Throws:** An assertion failure (typically via a unit-test framework) if the swap was not successful.

### ShouldBeFailedSwap

```csharp
public static void ShouldBeFailedSwap(HotSwapServiceTests testInstance)
```

Asserts that the most recent swap operation performed on the given test instance failed. This is the inverse of `ShouldBeSuccessfulSwap` and is used to verify expected failure paths.

- **Parameters:**
  - `testInstance` — The `HotSwapServiceTests` instance whose last swap outcome is to be validated.
- **Throws:** An assertion failure if the swap did not fail.

### ShouldHaveSingleHistoryEntry

```csharp
public static void ShouldHaveSingleHistoryEntry(HotSwapServiceTests testInstance)
```

Asserts that the swap history recorded by the test instance contains exactly one entry. This is typically used immediately after a single swap operation to confirm that history tracking is functioning correctly and no duplicate or spurious entries were created.

- **Parameters:**
  - `testInstance` — The `HotSwapServiceTests` instance whose swap history is to be inspected.
- **Throws:** An assertion failure if the history count is not exactly one.

### CallbackShouldReceivePlugin

```csharp
public static void CallbackShouldReceivePlugin(HotSwapServiceTests testInstance, Plugin expectedPlugin)
```

Asserts that the swap callback registered in the test instance was invoked with the specified plugin. This verifies that the callback mechanism correctly propagates the new plugin instance after a swap.

- **Parameters:**
  - `testInstance` — The `HotSwapServiceTests` instance whose callback invocation is to be validated.
  - `expectedPlugin` — The `Plugin` instance that the callback is expected to have received.
- **Throws:** An assertion failure if the callback was not invoked or was invoked with a different plugin.

### CreateAndTestCanSwap

```csharp
public static Plugin CreateAndTestCanSwap(string sourceCode, string assemblyName, string pluginTypeName)
```

A convenience method that compiles source code into a temporary DLL, loads the specified plugin type from it, and immediately asserts that the loaded plugin can be used in a swap operation. This combines `CreateTempDll`, `CreatePlugin`, and a swap-viability check into a single call.

- **Parameters:**
  - `sourceCode` — C# source code for the plugin assembly.
  - `assemblyName` — The name for the compiled assembly.
  - `pluginTypeName` — The fully qualified type name of the plugin to load and test.
- **Returns:** The `Plugin` instance that was loaded and verified as swappable.
- **Throws:** Any exception propagated from `CreateTempDll` or `CreatePlugin`; an assertion failure if the plugin cannot be swapped.

## Usage

### Example 1: End-to-end successful swap test

```csharp
// Arrange
var testInstance = HotSwapServiceTestsExtensions.CreateTestInstance();
string sourceCode = @"
    using dotnet_plugin_engine;
    public class TestPlugin : Plugin
    {
        public override void Execute() { }
    }";
string dllPath = HotSwapServiceTestsExtensions.CreateTempDll(sourceCode, "TestAssembly");
Plugin plugin = HotSwapServiceTestsExtensions.CreatePlugin(dllPath, "TestPlugin");

// Act
testInstance.PerformSwap(plugin);

// Assert
HotSwapServiceTestsExtensions.ShouldBeSuccessfulSwap(testInstance);
HotSwapServiceTestsExtensions.ShouldHaveSingleHistoryEntry(testInstance);
HotSwapServiceTestsExtensions.CallbackShouldReceivePlugin(testInstance, plugin);
```

### Example 2: Verifying a swap failure scenario

```csharp
// Arrange
var testInstance = HotSwapServiceTestsExtensions.CreateTestInstance();
string invalidSource = @"public class BadPlugin { }"; // Does not inherit Plugin
string dllPath = HotSwapServiceTestsExtensions.CreateTempDll(invalidSource, "BadAssembly");

// Act & Assert
Assert.Throws<InvalidCastException>(() =>
{
    HotSwapServiceTestsExtensions.CreatePlugin(dllPath, "BadPlugin");
});

// Alternatively, if a swap is forced with an incompatible instance:
testInstance.PerformSwapWithInvalidPlugin();
HotSwapServiceTestsExtensions.ShouldBeFailedSwap(testInstance);
```

## Notes

- **Temporary DLL lifecycle:** Assemblies created by `CreateTempDll` are written to the system temporary directory but are not automatically deleted. Tests should clean up these files to avoid accumulation, especially when running in CI environments with limited disk space.
- **Assertion framework dependency:** The `ShouldBe*` and `CallbackShould*` methods rely on an underlying assertion framework (e.g., NUnit, xUnit). They will throw framework-specific exceptions on failure; catch blocks should account for this when testing negative paths.
- **Thread safety:** All methods are static and operate on the instances passed to them. `HotSwapServiceTests` instances are not inherently thread-safe. Concurrent calls to assertion methods or factory methods that share the same test instance may produce race conditions. Each test should operate on its own isolated instance.
- **`CreateAndTestCanSwap` atomicity:** This method performs compilation, loading, and a swap-viability assertion in sequence. A failure at any stage aborts the entire operation, but the temporary DLL remains on disk. Callers should handle cleanup separately.
- **History assertions:** `ShouldHaveSingleHistoryEntry` assumes a known initial history state (typically empty). If a test instance is reused across multiple swap operations without resetting history, this assertion will produce false negatives.
