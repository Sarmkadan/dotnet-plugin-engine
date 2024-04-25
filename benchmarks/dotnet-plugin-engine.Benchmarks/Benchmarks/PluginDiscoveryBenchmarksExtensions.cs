using System;
using System.Threading.Tasks;

public static class PluginDiscoveryBenchmarksExtensions
{
    public static async Task RunDiscoveryBenchmarks(this PluginDiscoveryBenchmarks benchmarks)
    {
        await benchmarks.Discover_EmptyDirectory();
        await benchmarks.Discover_50Plugins();
        await benchmarks.Discover_200Plugins();
    }

    public static async Task RunMetadataBenchmarks(this PluginDiscoveryBenchmarks benchmarks)
    {
        await benchmarks.GetPluginMetadata_Single();
        await benchmarks.ValidatePluginFiles();
        await benchmarks.FilterValidPlugins();
    }

    public static async Task RunFullDiscoveryBenchmark(this PluginDiscoveryBenchmarks benchmarks)
    {
        await benchmarks.GlobalSetup();
        try
        {
            await benchmarks.RunDiscoveryBenchmarks();
            await benchmarks.RunMetadataBenchmarks();
        }
        finally
        {
            benchmarks.GlobalCleanup();
        }
    }

    public static async Task RunConcurrentDiscoveryBenchmarks(this PluginDiscoveryBenchmarks benchmarks, int iterations)
    {
        await benchmarks.GlobalSetup();
        try
        {
            var tasks = new Task[iterations];
            for (int i = 0; i < iterations; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    await benchmarks.Discover_EmptyDirectory();
                    await benchmarks.Discover_50Plugins();
                    await benchmarks.Discover_200Plugins();
                });
            }
            await Task.WhenAll(tasks);
        }
        finally
        {
            benchmarks.GlobalCleanup();
        }
    }
}
