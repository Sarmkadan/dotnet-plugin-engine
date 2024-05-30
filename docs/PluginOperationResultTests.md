# PluginOperationResultTests
The `PluginOperationResultTests` class is designed to test the functionality of the `PluginOperationResult` type, which represents the outcome of a plugin operation. This class contains a set of test methods that verify the correctness of the `PluginOperationResult` type in various scenarios, including successful and failed operations, error handling, and batch result processing.

## API
The `PluginOperationResultTests` class contains the following public members:
* `CreateSuccess_WithMessage_SetsSuccessTrueAndPreservesMessage`: Verifies that creating a successful `PluginOperationResult` with a message sets the `Success` property to `true` and preserves the message.
* `CreateFailure_WithCustomErrorCode_SetsSuccessFalseAndErrorCode`: Verifies that creating a failed `PluginOperationResult` with a custom error code sets the `Success` property to `false` and sets the error code.
* `FromException_WithPluginLoadException_ReturnsErrorCode1001`: Verifies that creating a `PluginOperationResult` from a `PluginLoadException` returns an error code of 1001.
* `FromException_WithDependencyResolutionException_ReturnsErrorCode1002`: Verifies that creating a `PluginOperationResult` from a `DependencyResolutionException` returns an error code of 1002.
* `FromException_WithUnknownException_ReturnsDefaultErrorCode500`: Verifies that creating a `PluginOperationResult` from an unknown exception returns a default error code of 500.
* `FromException_WithInnerException_IncludesInnerExceptionMessageInDetails`: Verifies that creating a `PluginOperationResult` from an exception with an inner exception includes the inner exception message in the details.
* `Generic_CreateSuccess_WithData_SetsBothSuccessAndData`: Verifies that creating a successful `PluginOperationResult` with data sets both the `Success` property and the data.
* `Generic_CreateFailure_LeavesDataNull`: Verifies that creating a failed `PluginOperationResult` leaves the data null.
* `BatchResult_AddResult_IncrementsSuccessAndFailureCountsCorrectly`: Verifies that adding a result to a batch result increments the success and failure counts correctly.
* `BatchResult_IsSuccessful_WhenAllSucceed_ReturnsTrue`: Verifies that a batch result is considered successful when all operations succeed.
* `BatchResult_IsSuccessful_WhenMoreFailuresThanSuccesses_ReturnsFalse`: Verifies that a batch result is not considered successful when more operations fail than succeed.
* `BatchResult_GetSummary_ContainsCountsAndTiming`: Verifies that the summary of a batch result contains the counts and timing information.

## Usage
Here are two examples of using the `PluginOperationResult` type:
```csharp
// Example 1: Creating a successful PluginOperationResult
var result = PluginOperationResult.CreateSuccess("Operation completed successfully");
Console.WriteLine(result.Success); // Output: True
Console.WriteLine(result.Message); // Output: Operation completed successfully

// Example 2: Creating a failed PluginOperationResult with a custom error code
var result2 = PluginOperationResult.CreateFailure(1003, "Operation failed with custom error code");
Console.WriteLine(result2.Success); // Output: False
Console.WriteLine(result2.ErrorCode); // Output: 1003
Console.WriteLine(result2.Message); // Output: Operation failed with custom error code
```

## Notes
When using the `PluginOperationResult` type, note that the `Success` property is set to `true` only when the operation is successful, and the `ErrorCode` property is set to a non-zero value only when the operation fails. Additionally, the `Message` property is preserved when creating a successful `PluginOperationResult` with a message. The `PluginOperationResult` type is designed to be thread-safe, and its methods can be safely called from multiple threads concurrently. However, the `BatchResult` class is not designed to be thread-safe, and its methods should not be called concurrently from multiple threads.
