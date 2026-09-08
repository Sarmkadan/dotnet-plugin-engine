# ErrorHandlingMiddleware

`ErrorHandlingMiddleware` wraps the next `PluginOperationDelegate`, records failures on its `PluginOperationContext`, and logs the error. It handles both `PluginException` instances and other exceptions.

## Build status

The implementation is present at `src/PluginEngine/Middleware/ErrorHandlingMiddleware.cs`, but `src/PluginEngine/PluginEngine.csproj` contains a `Compile Remove` entry for that file. Consequently, `ErrorHandlingMiddleware`, `ErrorHandlingMiddlewareExtensions`, and `UseErrorHandling` are excluded from the current `PluginEngine` build and are not available from the compiled assembly or package.

The API and examples below describe the code in that source file. They require the file to be included in a build before they can be used by a consumer.

## Signatures

| Member | Signature | Behavior |
| --- | --- | --- |
| Constructor | `ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, bool continueOnError = false)` | Stores the logger and determines whether caught exceptions are rethrown. |
| Invocation | `Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)` | Awaits `next(context)`, records and logs any exception it catches, then either rethrows or suppresses it according to `continueOnError`. |
| Pipeline extension | `PluginMiddlewarePipeline UseErrorHandling(this PluginMiddlewarePipeline pipeline, bool continueOnError = false)` | Adds an error-handling delegate to the pipeline and returns the result of `pipeline.Use(...)`. A new middleware and console-backed logger are created each time that delegate handles an operation. |

## `continueOnError` semantics

The default value is `false`. After recording and logging a caught exception, the middleware uses a bare `throw`, so the original exception continues to the caller with its existing type and stack information.

When `continueOnError` is `true`, the caught exception is not rethrown. `InvokeAsync` then completes normally. This setting does not retry the operation, invoke `next` again, or mark the operation successful; the failing call to `next` has already unwound, and the context remains unsuccessful with the exception attached.

Both branches apply the same `continueOnError` behavior to `PluginException` and to all other `Exception` instances.

## Context changes on failure

For every caught exception, the middleware writes exactly these two members of the supplied `PluginOperationContext`:

| Context member | Value written |
| --- | --- |
| `Exception` | The same exception instance that was caught. |
| `IsSuccessful` | `false`. |

The middleware does not change `OperationType`, `Plugin`, `Metadata`, `StartTimeMs`, or `EndTimeMs`.

## Logging behavior

For a `PluginException`, the error log includes `context.OperationType`, `context.Plugin.Name`, the exception message, and details selected from the exception type:

| Exception type | Detail text |
| --- | --- |
| `PluginLoadException` | Load stage and error code. |
| `DependencyResolutionException` | The unresolved dependencies joined with `", "`. |
| `VersionMismatchException` | Expected and actual versions. |
| Other `PluginException` | The runtime exception type name. |

For any other `Exception`, the middleware logs the exception together with the operation type and plugin name as an unhandled error.

## Example

The following illustrates the source API with suppression enabled. After the delegate throws, the call completes without rethrowing, while the context retains the failure:

```csharp
using Microsoft.Extensions.Logging;
using PluginEngine.Middleware;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var middleware = new ErrorHandlingMiddleware(
    loggerFactory.CreateLogger<ErrorHandlingMiddleware>(),
    continueOnError: true);

var context = new PluginOperationContext
{
    OperationType = "Load",
    Plugin = plugin
};

await middleware.InvokeAsync(
    context,
    _ => throw new InvalidOperationException("Load failed."));

// The exception was suppressed, but the failure was recorded.
Console.WriteLine(context.IsSuccessful);       // False
Console.WriteLine(context.Exception?.Message); // Load failed.
```

Pipeline registration uses the extension method and the same default of rethrowing errors:

```csharp
var pipeline = new PluginMiddlewarePipeline()
    .UseErrorHandling(continueOnError: false);
```
