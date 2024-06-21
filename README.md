// ... existing content ...

## PluginExceptionExtensions

The `PluginExceptionExtensions` class provides utility methods for working with `PluginException` instances. It allows you to add additional context and error codes to exceptions, as well as generate diagnostic reports and user-friendly messages.

### Usage Example

```csharp
try
{
    // Code that might throw a PluginException
}
catch (PluginException ex)
{
    var exceptionWithErrorCode = ex.WithErrorCode("PLUGIN_LOAD_FAILED");
    var exceptionWithContext = exceptionWithErrorCode.WithContext("pluginName", "MyPlugin");

    Console.WriteLine(exceptionWithContext.ToDiagnosticReport());
    Console.WriteLine(exceptionWithContext.ToUserFriendlyMessage());

    if (exceptionWithContext.IsErrorCode("PLUGIN_LOAD_FAILED"))
    {
        Console.WriteLine("Plugin load failed");
    }
}
```

// ... existing content ...
