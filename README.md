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
