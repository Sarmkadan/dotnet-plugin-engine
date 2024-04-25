using System;
using System.Threading.Tasks;

namespace dotnet_plugin_engine_benchmarks.Benchmarks
{
    public static class PluginExecutionBenchmarksExtensions
    {
        public static async Task Warmup(this PluginExecutionBenchmarks benchmarks)
        {
            await benchmarks.PluginLifecycle_Initialize();
            await benchmarks.PluginLifecycle_Cleanup();
        }

        public static async Task MeasurePluginOperationDuration(this PluginExecutionBenchmarks benchmarks, int iterations)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                await benchmarks.ExecutePluginOperation_Single();
            }
            sw.Stop();
            Console.WriteLine($"Average duration: {sw.Elapsed.TotalMilliseconds / iterations} ms");
        }

        public static async Task CompareBatchExecution(this PluginExecutionBenchmarks benchmarks, int batchSize)
        {
            var batchDuration = await benchmarks.ExecuteOperations_Batch(batchSize);
            var singleDuration = await benchmarks.ExecuteOperations_Batch(1);
            Console.WriteLine($"Batch execution duration: {batchDuration.TotalMilliseconds} ms");
            Console.WriteLine($"Single execution duration: {singleDuration.TotalMilliseconds} ms");
        }
    }
}
