using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Benchmarks;

/// <summary>
/// Benchmarks for dependency resolution operations - critical for plugin discovery and loading.
/// Measures throughput and memory allocations for dependency graph operations.
/// </summary>
[MemoryDiagnoser]
[HideColumns("Job", "RatioSD", "Alloc Ratio")]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class DependencyResolutionBenchmarks
{
    private IDependencyResolutionService _dependencyResolver = null!;
    private IServiceProvider _serviceProvider = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Setup dependency injection
        _serviceProvider = new ServiceCollection()
            .AddPluginEngineCore()
            .BuildServiceProvider();

        _dependencyResolver = _serviceProvider.GetRequiredService<IDependencyResolutionService>();
    }

    /// <summary>
    /// Benchmark: Empty dependency graph resolution
    /// Measures baseline performance with no dependencies
    /// </summary>
    [BenchmarkCategory("Dependency Resolution")]
    [Benchmark(Baseline = true)]
    public void Resolve_EmptyGraph()
    {
        // Simulate resolving an empty dependency graph
        var result = _dependencyResolver.ResolveDependencies(new Dictionary<string, string>());
        // No assertion needed - just measuring overhead
    }

    /// <summary>
    /// Benchmark: Simple linear dependency chain
    /// Measures performance with a simple A -> B -> C dependency chain
    /// </summary>
    [BenchmarkCategory("Dependency Resolution")]
    [Benchmark]
    public void Resolve_LinearChain()
    {
        var dependencies = new Dictionary<string, string>
        {
            {"A", "B"},
            {"B", "C"},
            {"C", ""}
        };

        var result = _dependencyResolver.ResolveDependencies(dependencies);
    }

    /// <summary>
    /// Benchmark: Diamond dependency pattern
    /// Measures performance with diamond dependency pattern (A -> B, A -> C, B -> D, C -> D)
    /// Tests the resolver's ability to handle complex dependency graphs
    /// </summary>
    [BenchmarkCategory("Dependency Resolution")]
    [Benchmark]
    public void Resolve_DiamondPattern()
    {
        var dependencies = new Dictionary<string, string>
        {
            {"A", "B,C"},
            {"B", "D"},
            {"C", "D"},
            {"D", ""}
        };

        var result = _dependencyResolver.ResolveDependencies(dependencies);
    }

    /// <summary>
    /// Benchmark: Circular dependency detection
    /// Measures performance when circular dependencies are present
    /// Tests the resolver's error handling and cycle detection
    /// </summary>
    [BenchmarkCategory("Dependency Resolution")]
    [Benchmark]
    public void Resolve_CircularDependency()
    {
        var dependencies = new Dictionary<string, string>
        {
            {"A", "B"},
            {"B", "C"},
            {"C", "A"}
        };

        try
        {
            var result = _dependencyResolver.ResolveDependencies(dependencies);
        }
        catch (DependencyResolutionException)
        {
            // Expected - just measuring the overhead
        }
    }

    /// <summary>
    /// Benchmark: Large dependency graph
    /// Measures performance with a large dependency graph (100 nodes)
    /// Tests scalability of the dependency resolution algorithm
    /// </summary>
    [BenchmarkCategory("Dependency Resolution")]
    [Benchmark]
    public void Resolve_LargeGraph()
    {
        var dependencies = new Dictionary<string, string>();

        // Create a chain of 100 dependencies
        for (var i = 0; i < 100; i++)
        {
            var name = $"Plugin{i}";
            var dependency = i > 0 ? $"Plugin{i - 1}" : "";
            dependencies[name] = dependency;
        }

        var result = _dependencyResolver.ResolveDependencies(dependencies);
    }

    /// <summary>
    /// Benchmark: Dependency resolution with version constraints
    /// Measures performance with version-constrained dependencies
    /// </summary>
    [BenchmarkCategory("Version Management")]
    [Benchmark]
    public void Resolve_VersionConstraints()
    {
        var dependencies = new Dictionary<string, string>
        {
            {"PluginA", ">=1.0.0"},
            {"PluginB", ">=2.0.0 <3.0.0"},
            {"PluginC", "^1.2.3"}
        };

        var result = _dependencyResolver.ResolveDependencies(dependencies);
    }

    /// <summary>
    /// Benchmark: Dependency resolution with plugin metadata
    /// Measures performance with plugin metadata and dependencies
    /// </summary>
    [BenchmarkCategory("Plugin Dependencies")]
    [Benchmark]
    public void Resolve_PluginMetadataDependencies()
    {
        var dependencies = new Dictionary<string, string>
        {
            {"CorePlugin", ""},
            {"FeatureA", "CorePlugin"},
            {"FeatureB", "CorePlugin"},
            {"FeatureC", "FeatureA,FeatureB"},
            {"FeatureD", "FeatureC"}
        };

        var result = _dependencyResolver.ResolveDependencies(dependencies);
    }

    /// <summary>
    /// Benchmark: Dependency resolution with circular dependency detection
    /// Measures performance of circular dependency detection
    /// </summary>
    [BenchmarkCategory("Error Handling")]
    [Benchmark]
    public void Resolve_CircularDependencyDeep()
    {
        var dependencies = new Dictionary<string, string>
        {
            {"A", "B"},
            {"B", "C"},
            {"C", "D"},
            {"D", "E"},
            {"E", "A"}
        };

        try
        {
            var result = _dependencyResolver.ResolveDependencies(dependencies);
        }
        catch (DependencyResolutionException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Benchmark: Dependency resolution with missing dependencies
    /// Measures performance when dependencies are missing
    /// </summary>
    [BenchmarkCategory("Error Handling")]
    [Benchmark]
    public void Resolve_MissingDependencies()
    {
        var dependencies = new Dictionary<string, string>
        {
            {"PluginA", "MissingPlugin"},
            {"PluginB", "PluginA"}
        };

        try
        {
            var result = _dependencyResolver.ResolveDependencies(dependencies);
        }
        catch (DependencyResolutionException)
        {
            // Expected
        }
    }
}
