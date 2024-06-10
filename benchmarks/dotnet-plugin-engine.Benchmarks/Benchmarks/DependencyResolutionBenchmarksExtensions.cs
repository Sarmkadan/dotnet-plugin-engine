using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PluginEngine.Benchmarks
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
        /// <param name="bench">The benchmark instance to run.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bench"/> is <see langword="null"/>.</exception>
        public static void RunAll(this DependencyResolutionBenchmarks bench)
        {
            ArgumentNullException.ThrowIfNull(bench);

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
        /// <param name="bench">The benchmark instance to measure.</param>
        /// <returns>A read-only dictionary mapping benchmark names to their execution times.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bench"/> is <see langword="null"/>.</exception>
        public static IReadOnlyDictionary<string, TimeSpan> MeasureAll(this DependencyResolutionBenchmarks bench)
        {
            ArgumentNullException.ThrowIfNull(bench);

            var results = new Dictionary<string, TimeSpan>(StringComparer.Ordinal);

            // Helper to time a single action.
            void Measure(string name, Action action)
            {
                ArgumentNullException.ThrowIfNull(name);
                ArgumentNullException.ThrowIfNull(action);

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
        /// <param name="bench">The benchmark instance to warm up.</param>
        /// <param name="iterations">How many times each benchmark should be executed. Must be greater than zero.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bench"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="iterations"/> is less than or equal to zero.</exception>
        public static void WarmupAll(this DependencyResolutionBenchmarks bench, int iterations = 3)
        {
            ArgumentNullException.ThrowIfNull(bench);

            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations), iterations, "Iterations must be greater than zero.");
            }

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
}}
