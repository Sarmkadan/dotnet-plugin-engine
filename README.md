// ... existing content ...

## PluginOperationResultExtensions

The `PluginOperationResultExtensions` class provides utility methods for working with `PluginOperationResult` instances. It allows you to check for failures in a collection of results, convert results to batch operation formats, generate detailed summaries, and convert generic results to non-generic forms.

### Usage Example

```csharp
var results = new List<PluginOperationResult<string>>
{
    new PluginOperationResult<string> { Success = true, Data = "Plugin1" },
    new PluginOperationResult<string> { Success = false, ErrorMessage = "Invalid input" },
    new PluginOperationResult<string> { Success = true, Data = "Plugin3" }
};

if (results.HasFailures())
{
    var batchResult = results.ToBatchResult();
    var summary = batchResult.GetDetailedSummary();
    var nonGenericResult = batchResult.ToNonGeneric();

    Console.WriteLine(summary);
    Console.WriteLine(nonGenericResult);
}
```
