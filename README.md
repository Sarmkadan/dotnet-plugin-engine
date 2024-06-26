// ... existing content ...

## HotSwapServiceTestsExtensions

The `HotSwapServiceTestsExtensions` class provides a set of extension methods for testing hot swap functionality. These methods enable creation of test instances and verification of swap results.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;

public class HotSwapExample
{
    public async Task DemonstrateHotSwap()
    {
        // Create a test plugin
        var plugin = HotSwapServiceTestsExtensions.CreatePlugin();

        // Create a test hot swap service
        var hotSwapService = HotSwapServiceTestsExtensions.CreateService();

        // Perform a successful swap
        HotSwapServiceTestsExtensions.ShouldBeSuccessfulSwap(hotSwapService, plugin);

        // Verify the swap result
        HotSwapServiceTestsExtensions.ShouldHaveSingleHistoryEntry(hotSwapService, plugin);

        // Test a failed swap
        HotSwapServiceTestsExtensions.ShouldBeFailedSwap(hotSwapService, plugin);

        // Create and test a plugin that can be swapped
        var (swappablePlugin, _) = HotSwapServiceTestsExtensions.CreateAndTestCanSwap();
    }
}
```
