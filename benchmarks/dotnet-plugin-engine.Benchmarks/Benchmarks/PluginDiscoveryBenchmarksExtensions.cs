using System;
using System.Threading.Tasks;

/// <summary>
/// Provides extension methods for running plugin discovery benchmarks in various configurations.
/// </summary>
public static class PluginDiscoveryBenchmarksExtensions
{
    /// <summary>
    /// Runs all plugin discovery benchmarks sequentially: empty directory discovery, 50 plugin discovery, and 200 plugin discovery.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance to run.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
    public static async Task RunDiscoveryBenchmarks(this PluginDiscoveryBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        await benchmarks.Discover_EmptyDirectory();
        await benchmarks.Discover_50Plugins();
        await benchmarks.Discover_200Plugins();
    }

    /// <summary>
    /// Runs all plugin metadata benchmarks sequentially: single plugin metadata extraction, plugin file validation, and plugin filtering.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance to run.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
    public static async Task RunMetadataBenchmarks(this PluginDiscoveryBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        await benchmarks.GetPluginMetadata_Single();
        await benchmarks.ValidatePluginFiles();
        await benchmarks.FilterValidPlugins();
    }

    /// <summary>
    /// Executes a complete plugin discovery benchmark workflow including global setup, discovery benchmarks, metadata benchmarks, and global cleanup.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance to run.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
    public static async Task RunFullDiscoveryBenchmark(this PluginDiscoveryBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

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

    /// <summary>
    /// Executes plugin discovery benchmarks concurrently for the specified number of iterations.
    /// Each iteration runs empty directory discovery, 50 plugin discovery, and 200 plugin discovery in sequence.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance to run.</param>
    /// <param name="iterations">The number of concurrent iterations to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="iterations"/> is less than 1.</exception>
    public static async Task RunConcurrentDiscoveryBenchmarks(this PluginDiscoveryBenchmarks benchmarks, int iterations)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);

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
