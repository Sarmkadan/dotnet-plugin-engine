using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotnetPluginEngine.Benchmarks
{
    /// <summary>
    /// Extension methods that make it easier to work with <see cref="DependencyResolutionBenchmarks"/>.
    /// The methods use only the public members that exist on the benchmark class.
    /// </summary>
    public static class DependencyResolutionBenchmarksExtensions
    {
        /// <summary>
        /// Executes every resolve benchmark in a deterministic order.
        /// Useful for ad‑hoc runs or when you want to ensure the benchmark
        /// class is exercised without using BenchmarkDotNet.
        /// </summary>
        public static void RunAll(this DependencyResolutionBenchmarks bench)
        {
            // Ensure any required global state is prepared.
            bench.GlobalSetup();

            bench.Resolve_EmptyGraph();
            bench.Resolve_LinearChain();
            bench.Resolve_DiamondPattern();
            bench.Resolve_CircularDependency();
            bench.Resolve_LargeGraph();
            bench.Resolve_VersionConstraints();
            bench.Resolve_PluginMetadataDependencies();
            bench.Resolve_CircularDependencyDeep();
            bench.Resolve_MissingDependencies();
        }

        /// <summary>
        /// Runs each resolve benchmark once and records the elapsed time.
        /// Returns a dictionary that maps the benchmark method name to its duration.
        /// </summary>
        public static IReadOnlyDictionary<string, TimeSpan> MeasureAll(this DependencyResolutionBenchmarks bench)
        {
            var results = new Dictionary<string, TimeSpan>(StringComparer.Ordinal);

            // Helper to time a single action.
            void Measure(string name, Action action)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                results[name] = sw.Elapsed;
            }

            bench.GlobalSetup();

            Measure(nameof(bench.Resolve_EmptyGraph), bench.Resolve_EmptyGraph);
            Measure(nameof(bench.Resolve_LinearChain), bench.Resolve_LinearChain);
            Measure(nameof(bench.Resolve_DiamondPattern), bench.Resolve_DiamondPattern);
            Measure(nameof(bench.Resolve_CircularDependency), bench.Resolve_CircularDependency);
            Measure(nameof(bench.Resolve_LargeGraph), bench.Resolve_LargeGraph);
            Measure(nameof(bench.Resolve_VersionConstraints), bench.Resolve_VersionConstraints);
            Measure(nameof(bench.Resolve_PluginMetadataDependencies), bench.Resolve_PluginMetadataDependencies);
            Measure(nameof(bench.Resolve_CircularDependencyDeep), bench.Resolve_CircularDependencyDeep);
            Measure(nameof(bench.Resolve_MissingDependencies), bench.Resolve_MissingDependencies);

            return results;
        }

        /// <summary>
        /// Performs a warm‑up run of all resolve benchmarks a configurable number of times.
        /// This can be useful to mitigate JIT warm‑up effects before a measured run.
        /// </summary>
        /// <param name="iterations">How many times each benchmark should be executed.</param>
        public static void WarmupAll(this DependencyResolutionBenchmarks bench, int iterations = 3)
        {
            if (iterations <= 0) return;

            bench.GlobalSetup();

            for (int i = 0; i < iterations; i++)
            {
                bench.Resolve_EmptyGraph();
                bench.Resolve_LinearChain();
                bench.Resolve_DiamondPattern();
                bench.Resolve_CircularDependency();
                bench.Resolve_LargeGraph();
                bench.Resolve_VersionConstraints();
                bench.Resolve_PluginMetadataDependencies();
                bench.Resolve_CircularDependencyDeep();
                bench.Resolve_MissingDependencies();
            }
        }
    }
}
