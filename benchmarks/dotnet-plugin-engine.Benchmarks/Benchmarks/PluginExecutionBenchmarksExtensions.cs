using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Loggers;

namespace PluginEngine.Benchmarks
{
    /// <summary>
    /// Provides extension methods for benchmarking plugin execution operations.
    /// </summary>
    public static class PluginExecutionBenchmarksExtensions
    {
        /// <summary>
        /// Performs a warmup cycle to ensure plugins are loaded and ready for benchmarking.
        /// This prepares the plugin lifecycle by initializing and cleaning up before actual benchmarking.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to warm up.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        public static async Task Warmup(this PluginExecutionBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            await benchmarks.PluginLifecycle_Initialize();
            await benchmarks.PluginLifecycle_Cleanup();
        }

        /// <summary>
        /// Measures the average duration of plugin operation execution over a specified number of iterations.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <param name="iterations">The number of iterations to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="iterations"/> is less than 1.</exception>
        public static async Task MeasurePluginOperationDuration(this PluginExecutionBenchmarks benchmarks, int iterations)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                await benchmarks.ExecutePluginOperation_Single();
            }
            sw.Stop();

            var logger = new ConsoleLogger();
            logger.WriteLine($"Average duration: {sw.Elapsed.TotalMilliseconds / iterations} ms");
        }

        /// <summary>
        /// Compares batch execution performance against single execution.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <param name="batchSize">The batch size to test.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="batchSize"/> is less than 1.</exception>
        public static async Task CompareBatchExecution(this PluginExecutionBenchmarks benchmarks, int batchSize)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

            var batchDuration = await benchmarks.ExecuteOperations_Batch(batchSize);
            var singleDuration = await benchmarks.ExecuteOperations_Batch(1);

            var logger = new ConsoleLogger();
            logger.WriteLine($"Batch execution duration: {batchDuration.TotalMilliseconds} ms");
            logger.WriteLine($"Single execution duration: {singleDuration.TotalMilliseconds} ms");
        }
    }
}
