// ... existing content ...

## HotSwapServiceExtensions

The `HotSwapServiceExtensions` class provides a set of extension methods for facilitating plugin hot-swapping, allowing plugins to be swapped out while the application is running. This enables features like live updates and A/B testing.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using PluginEngine.Domain.Entities; // Adjust the namespace if necessary

public class HotSwapExample
{
    public async Task DemonstrateHotSwap()
    {
        // Check if hot swap is supported
        if (!HotSwapServiceExtensions.CanSwap)
        {
            Console.WriteLine("Hot swap is not supported.");
            return;
        }

        try
        {
            // Perform a plugin swap
            var swapResult = await HotSwapServiceExtensions.SwapPluginAsync("MyPlugin", "NewPluginVersion");
            if (swapResult.IsSuccess)
            {
                Console.WriteLine("Plugin swapped successfully.");
            }
            else
            {
                Console.WriteLine($"Swap failed: {swapResult.ErrorMessage}");
            }

            // Get the last swap record
            var lastSwapRecordResult = await HotSwapServiceExtensions.GetLastSwapRecordAsync();
            if (lastSwapRecordResult.IsSuccess && lastSwapRecordResult.Value != null)
            {
                Console.WriteLine($"Last swap record: {lastSwapRecordResult.Value.PluginName} -> {lastSwapRecordResult.Value.NewPluginName}");
            }

            // Get swap history
            var swapHistoryResult = await HotSwapServiceExtensions.GetSwapHistoryAsync();
            if (swapHistoryResult.IsSuccess)
            {
                foreach (var record in swapHistoryResult.Value)
                {
                    Console.WriteLine($"Swap record: {record.PluginName} -> {record.NewPluginName} at {record.Timestamp}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
```
