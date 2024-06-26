## DependencyResolutionServiceTestsExtensions

The `DependencyResolutionServiceTestsExtensions` class provides utility methods for testing dependency resolution scenarios. It includes helpers to create test plugins/dependencies, configure loader services, and assert dependency resolution outcomes.

### Usage Example

```csharp
using System;
using System.Collections.Generic;

public class DependencyResolutionTest
{
    public void TestDependencyResolution()
    {
        // Create test plugin with dependencies
        var plugin = DependencyResolutionServiceTestsExtensions.CreateTestPlugin("MyPlugin");
        var dep1 = DependencyResolutionServiceTestsExtensions.CreateTestDependency("Dep1");
        var dep2 = DependencyResolutionServiceTestsExtensions.CreateTestDependency("Dep2");

        // Setup loader service with plugin
        DependencyResolutionServiceTestsExtensions.SetupLoaderServiceForPlugin(plugin);

        // Assert dependency relationships
        plugin.ShouldHaveDependencyCount(2);
        plugin.ShouldHaveDependencyOn(dep1);
        plugin.ShouldHaveDependencyOn(dep2);

        // Resolve dependencies and verify
        var resolver = DependencyResolutionServiceTestsExtensions.CreateResolver();
        resolver.ShouldResolveTo(new[] { dep1.Id, dep2.Id });
    }
}
```

## PluginOperationResultTestsExtensions

The `PluginOperationResultTestsExtensions` class offers a set of fluent helpers for creating, asserting, and inspecting `PluginOperationResult` and `PluginBatchOperationResult` objects in unit tests. It streamlines verification of success/failure states, messages, error codes, durations, result data, counts, and batch‑level details.

### Usage Example

```csharp
using System;
using PluginEngine.Results;
using PluginEngine.Tests.Extensions; // Adjust the namespace to where the extensions are defined

public class PluginOperationResultTests
{
    public void VerifySuccessResult()
    {
        // Create a successful result and assert its properties
        var success = PluginOperationResultTestsExtensions.CreateAndAssertSuccess<string>();
        PluginOperationResultTestsExtensions.ShouldBeSuccessfulWithMessage(success, "Operation completed successfully");
        PluginOperationResultTestsExtensions.ShouldHaveDurationMs(success, 120);
        PluginOperationResultTestsExtensions.ShouldHaveData(success, "ExpectedPayload");
    }

    public void VerifyFailureResult()
    {
        // Create a failure result and assert its error details
        var failure = PluginOperationResultTestsExtensions.CreateAndAssertFailure();
        PluginOperationResultTestsExtensions.ShouldBeFailureWithErrorCode(failure, 404);
        PluginOperationResultTestsExtensions.ShouldHaveErrorDetails(failure, "Resource not found");
        PluginOperationResultTestsExtensions.ShouldHaveDurationMs(failure, 30);
    }

    public void VerifyBatchResult()
    {
        // Build a batch result containing two successful operations
        var batch = PluginOperationResultTestsExtensions.CreateBatchWithResults(
            PluginOperationResultTestsExtensions.CreateAndAssertSuccess<int>(),
            PluginOperationResultTestsExtensions.CreateAndAssertSuccess<int>()
        );

        PluginOperationResultTestsExtensions.ShouldHaveResultCount(batch, 2);
        PluginOperationResultTestsExtensions.ShouldHaveCounts(batch, total: 2, successful: 2, failed: 0);
        PluginOperationResultTestsExtensions.ShouldHaveTotalDurationMs(batch, 250);
    }
}
```
