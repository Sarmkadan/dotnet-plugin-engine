# PluginOperationResultTestsExtensions

A static utility class providing assertion helpers and factory methods for testing `PluginOperationResult<T>`, `PluginOperationResult`, and `PluginBatchOperationResult` instances. These extension methods streamline validation of success/failure states, error codes, timing metadata, payload data, and batch result counts in unit test scenarios.

## API

### ShouldBeSuccessfulWithMessage

```csharp
public static void ShouldBeSuccessfulWithMessage(this PluginOperationResult result, string expectedMessage)
```

Asserts that the operation result indicates success and that its message matches `expectedMessage`. Throws an assertion exception if the result is not successful or the message differs.

**Parameters:**
- `result` — the `PluginOperationResult` to inspect.
- `expectedMessage` — the exact message string expected.

**Return value:** void.

**Throws:** assertion exception on mismatch or failure state.

---

### ShouldBeFailureWithErrorCode

```csharp
public static void ShouldBeFailureWithErrorCode(this PluginOperationResult result, string expectedErrorCode)
```

Asserts that the operation result indicates failure and that its error code equals `expectedErrorCode`. Throws if the result is successful or the error code does not match.

**Parameters:**
- `result` — the `PluginOperationResult` to inspect.
- `expectedErrorCode` — the exact error code string expected.

**Return value:** void.

**Throws:** assertion exception on mismatch or success state.

---

### ShouldHaveDurationMs

```csharp
public static void ShouldHaveDurationMs(this PluginOperationResult result, long expectedDurationMs, long toleranceMs = 0)
```

Asserts that the operation’s recorded duration in milliseconds falls within `[expectedDurationMs - toleranceMs, expectedDurationMs + toleranceMs]`. Throws if the actual duration is outside this range.

**Parameters:**
- `result` — the `PluginOperationResult` to inspect.
- `expectedDurationMs` — the target duration in milliseconds.
- `toleranceMs` — allowed deviation; defaults to 0 for exact match.

**Return value:** void.

**Throws:** assertion exception when duration is out of tolerance.

---

### CreateAndAssertSuccess\<T\>

```csharp
public static PluginOperationResult<T> CreateAndAssertSuccess<T>(T data, string message = null)
```

Factory that constructs a successful `PluginOperationResult<T>` with the given payload and optional message, then immediately asserts it is in a success state. Returns the created result for further inspection.

**Parameters:**
- `data` — the payload of type `T` to embed.
- `message` — optional success message.

**Return value:** a new `PluginOperationResult<T>` in success state.

**Throws:** assertion exception if the constructed result is not successful.

---

### CreateAndAssertFailure

```csharp
public static PluginOperationResult CreateAndAssertFailure(string errorCode, string message = null)
```

Factory that constructs a failed `PluginOperationResult` with the given error code and optional message, then immediately asserts it is in a failure state. Returns the created result.

**Parameters:**
- `errorCode` — the error code string.
- `message` — optional failure message.

**Return value:** a new `PluginOperationResult` in failure state.

**Throws:** assertion exception if the constructed result is not a failure.

---

### ShouldHaveData\<T\>

```csharp
public static void ShouldHaveData<T>(this PluginOperationResult<T> result, T expectedData)
```

Asserts that the generic operation result carries a payload equal to `expectedData`. Equality is determined by the default comparer for `T`. Throws if the result has no data or the data differs.

**Parameters:**
- `result` — the `PluginOperationResult<T>` to inspect.
- `expectedData` — the expected payload value.

**Return value:** void.

**Throws:** assertion exception on missing or mismatched data.

---

### ShouldHaveCounts

```csharp
public static void ShouldHaveCounts(this PluginBatchOperationResult result, int expectedTotal, int expectedSuccessful, int expectedFailed)
```

Asserts that a batch operation result reports the exact total, successful, and failed counts. All three values must match simultaneously.

**Parameters:**
- `result` — the `PluginBatchOperationResult` to inspect.
- `expectedTotal` — expected total item count.
- `expectedSuccessful` — expected successful item count.
- `expectedFailed` — expected failed item count.

**Return value:** void.

**Throws:** assertion exception if any count differs.

---

### ShouldHaveResultCount

```csharp
public static void ShouldHaveResultCount(this PluginBatchOperationResult result, int expectedCount)
```

Asserts that the batch result contains exactly `expectedCount` individual operation results. Throws if the inner result collection size differs.

**Parameters:**
- `result` — the `PluginBatchOperationResult` to inspect.
- `expectedCount` — expected number of inner results.

**Return value:** void.

**Throws:** assertion exception on count mismatch.

---

### CreateBatchWithResults

```csharp
public static PluginBatchOperationResult CreateBatchWithResults(params PluginOperationResult[] results)
```

Factory that constructs a `PluginBatchOperationResult` wrapping the provided individual operation results. The batch aggregates counts from the supplied results.

**Parameters:**
- `results` — a params array of `PluginOperationResult` instances to include.

**Return value:** a new `PluginBatchOperationResult` containing the given results.

**Throws:** no direct throws; invalid arguments may cause downstream assertion failures.

---

### ShouldHaveErrorDetails

```csharp
public static void ShouldHaveErrorDetails(this PluginOperationResult result, string expectedErrorCode, string expectedMessage)
```

Combined assertion that the operation result is a failure with both the specified error code and message. Throws if either property differs or the result is successful.

**Parameters:**
- `result` — the `PluginOperationResult` to inspect.
- `expectedErrorCode` — the exact error code expected.
- `expectedMessage` — the exact error message expected.

**Return value:** void.

**Throws:** assertion exception on any mismatch.

---

### ShouldHaveTotalDurationMs

```csharp
public static void ShouldHaveTotalDurationMs(this PluginBatchOperationResult result, long expectedDurationMs, long toleranceMs = 0)
```

Asserts that the batch operation’s total duration in milliseconds falls within the tolerance range around `expectedDurationMs`. Throws if the actual total duration is outside the range.

**Parameters:**
- `result` — the `PluginBatchOperationResult` to inspect.
- `expectedDurationMs` — the target total duration in milliseconds.
- `toleranceMs` — allowed deviation; defaults to 0.

**Return value:** void.

**Throws:** assertion exception when total duration is out of tolerance.

## Usage

### Example 1: Validating a successful single operation with payload

```csharp
var result = PluginOperationResultTestsExtensions.CreateAndAssertSuccess(
    data: new PluginData { Id = 42, Name = "Export" },
    message: "Export completed"
);

result.ShouldBeSuccessfulWithMessage("Export completed");
result.ShouldHaveData(new PluginData { Id = 42, Name = "Export" });
result.ShouldHaveDurationMs(expectedDurationMs: 120, toleranceMs: 10);
```

### Example 2: Constructing and asserting a batch with mixed outcomes

```csharp
var success1 = PluginOperationResultTestsExtensions.CreateAndAssertSuccess("item1");
var failure = PluginOperationResultTestsExtensions.CreateAndAssertFailure("ERR_TIMEOUT", "Connection lost");
var success2 = PluginOperationResultTestsExtensions.CreateAndAssertSuccess("item2");

var batch = PluginOperationResultTestsExtensions.CreateBatchWithResults(success1, failure, success2);

batch.ShouldHaveCounts(expectedTotal: 3, expectedSuccessful: 2, expectedFailed: 1);
batch.ShouldHaveResultCount(3);
batch.ShouldHaveTotalDurationMs(expectedDurationMs: 350, toleranceMs: 50);
failure.ShouldHaveErrorDetails("ERR_TIMEOUT", "Connection lost");
```

## Notes

- All assertion methods throw on the first mismatch encountered; they do not accumulate multiple failures.
- `ShouldHaveDurationMs` and `ShouldHaveTotalDurationMs` accept a tolerance to accommodate minor timing variations in test environments. Use a tolerance of 0 only when durations are deterministic or mocked.
- `ShouldHaveData<T>` relies on `T`’s default equality comparer. For reference types without overridden equality, this performs reference equality, which may cause unexpected failures if distinct but semantically equal objects are compared.
- Factory methods `CreateAndAssertSuccess<T>` and `CreateAndAssertFailure` perform an immediate self-check, ensuring that malformed result objects are caught at creation time rather than later in the test.
- These methods are designed for single-threaded test execution. They hold no mutable shared state and are safe to call concurrently, but the results they inspect are not thread-safe and should not be mutated by multiple threads during assertion.
- `CreateBatchWithResults` derives aggregate counts from the supplied results’ states. Passing results whose internal state is later changed will not update the batch retroactively.
