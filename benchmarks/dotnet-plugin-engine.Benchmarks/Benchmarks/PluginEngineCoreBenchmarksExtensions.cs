using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using PluginEngine.Domain.Entities;

namespace PluginEngine.Benchmarks;

/// <summary>
/// Extension methods for <see cref="PluginEngineCoreBenchmarks"/> to provide additional benchmarking scenarios
/// and helper methods for common benchmark patterns.
/// </summary>
public static class PluginEngineCoreBenchmarksExtensions
{
    /// <summary>
    /// Extension method to benchmark plugin loading with warm-up phase.
    /// Measures the time to load all plugins with a warm-up iteration to account for JIT compilation.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Task representing the benchmark operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
    [BenchmarkCategory("Engine Operations - Warm-up")]
    [Benchmark]
    public static async Task LoadAllPlugins_WithWarmUp(this PluginEngineCoreBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        // Warm-up: load plugins once to account for JIT compilation
        await benchmarks.LoadAllPlugins_ThroughEngine();

        // Actual benchmark: load plugins again
        await benchmarks.LoadAllPlugins_ThroughEngine();
    }

    /// <summary>
    /// Extension method to benchmark plugin unloading with cleanup phase.
    /// Measures the time to unload all plugins with a cleanup iteration to ensure clean state.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Task representing the benchmark operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
    [BenchmarkCategory("Engine Operations - Cleanup")]
    [Benchmark]
    public static async Task UnloadAllPlugins_WithCleanup(this PluginEngineCoreBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        // Setup: load plugins first
        await benchmarks.LoadAllPlugins_ThroughEngine();

        // Actual benchmark: unload plugins
        await benchmarks.UnloadAllPlugins_ThroughEngine();

        // Cleanup: ensure no plugins remain loaded
        await benchmarks.UnloadAllPlugins_ThroughEngine();
    }

    /// <summary>
    /// Extension method to benchmark engine lifecycle with multiple operations.
    /// Measures the time to perform a complete engine lifecycle: initialize, load, check health, unload.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Task representing the benchmark operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
    [BenchmarkCategory("Engine Operations - Lifecycle")]
    [Benchmark]
    public static async Task CompleteEngineLifecycle(this PluginEngineCoreBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        await benchmarks.Initialize_Engine();
        await benchmarks.LoadAllPlugins_ThroughEngine();
        await benchmarks.GetHealthInfo();
        await benchmarks.UnloadAllPlugins_ThroughEngine();
        await benchmarks.GlobalCleanup();
    }

    /// <summary>
    /// Extension method to benchmark plugin status checks.
    /// Measures the time to perform multiple status checks on the engine.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Task representing the benchmark operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
    [BenchmarkCategory("Engine Operations - Status")]
    [Benchmark]
    public static async Task MultipleStatusChecks(this PluginEngineCoreBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        // Load plugins first to have meaningful status
        await benchmarks.LoadAllPlugins_ThroughEngine();

        // Perform multiple status checks
        await benchmarks.GetStatus();
        await benchmarks.GetStatus();
        await benchmarks.GetStatus();
    }
}