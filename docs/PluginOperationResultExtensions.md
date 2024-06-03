# PluginOperationResultExtensions

The `PluginOperationResultExtensions` class provides a set of static extension methods designed to simplify the manipulation, aggregation, and analysis of `PluginOperationResult` instances within the `dotnet-plugin-engine` framework. It facilitates the conversion between single and batch operation results, enables quick failure detection across collections, and generates human-readable summaries for logging or diagnostic purposes without requiring additional boilerplate code in consumer applications.

## API

### ToBatchResult (IEnumerable)
```csharp
public static PluginBatchOperationResult ToBatchResult(this IEnumerable<PluginOperationResult> results)
```
Aggregates a sequence of individual `PluginOperationResult` objects into a single `PluginBatchOperationResult`. This method iterates through the provided collection to consolidate success states, error messages, and metadata into a unified batch context.

*   **Parameters**:
    *   `results`: The enumerable collection of operation results to aggregate.
*   **Returns**: A `PluginBatchOperationResult` containing the combined status of all input results.
*   **Throws**: `ArgumentNullException` if `results` is `null`.

### ToBatchResult (Single)
```csharp
public static PluginBatchOperationResult ToBatchResult(this PluginOperationResult result)
```
Wraps a single `PluginOperationResult` instance into a `PluginBatchOperationResult`. This is useful for normalizing return types when a method signature expects a batch result but only a single operation was performed.

*   **Parameters**:
    *   `result`: The single operation result to wrap.
*   **Returns**: A `PluginBatchOperationResult` containing the single input result as its only member.
*   **Throws**: `ArgumentNullException` if `result` is `null`.

### HasFailures
```csharp
public static bool HasFailures(this IEnumerable<PluginOperationResult> results)
```
Evaluates a collection of operation results to determine if any individual result indicates a failure state. This provides a short-circuiting check to avoid processing further if an error has already occurred.

*   **Parameters**:
    *   `results`: The enumerable collection of operation results to inspect.
*   **Returns**: `true` if at least one result in the collection represents a failure; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if `results` is `null`.

### GetDetailedSummary
```csharp
public static string GetDetailedSummary(this IEnumerable<PluginOperationResult> results)
```
Generates a formatted string containing a comprehensive summary of the operation results. The output typically includes the total count, success/failure ratios, and specific error messages for failed operations, suitable for logging or exception details.

*   **Parameters**:
    *   `results`: The enumerable collection of operation results to summarize.
*   **Returns**: A formatted string representing the detailed status of the operations.
*   **Throws**: `ArgumentNullException` if `results` is `null`.

### ToNonGeneric
```csharp
public static PluginOperationResult ToNonGeneric(this PluginOperationResult result)
```
Converts a strongly-typed or generic-derived `PluginOperationResult` into the base non-generic `PluginOperationResult` type. This is primarily used for interoperability scenarios where generic type parameters must be erased.

*   **Parameters**:
    *   `result`: The operation result to convert.
*   **Returns**: The base `PluginOperationResult` instance.
*   **Throws**: `ArgumentNullException` if `result` is `null`.

## Usage

### Aggregating Results and Checking for Errors
The following example demonstrates executing multiple plugin operations, aggregating them into a batch result, and efficiently checking for failures before proceeding.

```csharp
using DotNetPluginEngine;
using System.Collections.Generic;
using System.Linq;

public class PluginExecutor
{
    public void ExecutePlugins(IEnumerable<IPlugin> plugins)
    {
        var results = new List<PluginOperationResult>();

        foreach (var plugin in plugins)
        {
            results.Add(plugin.Execute());
        }

        // Check for any failures quickly without iterating manually
        if (results.HasFailures())
        {
            var summary = results.GetDetailedSummary();
            System.Console.WriteLine($"Execution failed: {summary}");
            return;
        }

        // Convert to batch result for final reporting
        var batchResult = results.ToBatchResult();
        ProcessSuccessfulBatch(batchResult);
    }

    private void ProcessSuccessfulBatch(PluginBatchOperationResult batch)
    {
        // Handle success logic
    }
}
```

### Normalizing Single Results to Batch Context
This example shows how to handle a scenario where a single plugin execution needs to be returned in a format consistent with batch processing pipelines.

```csharp
using DotNetPluginEngine;

public class SinglePluginHandler
{
    public PluginBatchOperationResult HandleSinglePlugin(IPlugin plugin)
    {
        var singleResult = plugin.Execute();

        if (!singleResult.IsSuccess)
        {
            // Convert single failure to batch format for consistent upstream handling
            return singleResult.ToBatchResult();
        }

        // Wrap success in batch format
        return singleResult.ToBatchResult();
    }
}
```

## Notes

*   **Null Safety**: All extension methods defined in this class perform null checks on their primary input arguments (`this` parameter). Passing `null` to any of these methods will result in an `ArgumentNullException`. Callers should ensure collections or result instances are instantiated before invocation.
*   **Enumeration Behavior**: Methods accepting `IEnumerable<PluginOperationResult>` (such as `HasFailures`, `GetDetailedSummary`, and the collection overload of `ToBatchResult`) enumerate the source collection. If the provided enumerable yields results dynamically or has side effects during enumeration, these side effects will occur when the extension method is called. The enumeration happens once per method call.
*   **Thread Safety**: This class is stateless and consists entirely of static methods that operate solely on provided parameters. It is thread-safe for concurrent calls provided that the input collections (`IEnumerable<PluginOperationResult>`) are not modified by other threads during enumeration. If the underlying collection is being modified concurrently, external synchronization is required by the caller.
*   **Empty Collections**: Invoking `ToBatchResult` or `GetDetailedSummary` on an empty enumerable is valid and will return a batch result indicating zero operations or a summary string reflecting an empty set, respectively. `HasFailures` will return `false` for an empty collection.
