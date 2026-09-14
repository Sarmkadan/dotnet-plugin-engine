using BenchmarkDotNet.Attributes;
using PluginEngine.Exceptions;
using System;

namespace PluginEngine.Benchmarks;

/// <summary>
/// Benchmarks for DependencyResolutionException operations - measuring performance of exception creation, manipulation, and serialization.
/// </summary>
[MemoryDiagnoser]
public class DependencyResolutionExceptionBenchmarks
{
    private string _testMessage;
    private Guid _testPluginId;
    private string _testVersionConstraint;
    private DependencyResolutionReason _testReason;

    /// <summary>
    /// Sets up test data for dependency resolution exception benchmarks.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _testMessage = "Dependency resolution failed for test plugin";
        _testPluginId = Guid.NewGuid();
        _testVersionConstraint = "[1.0.0, 2.0.0)";
        _testReason = DependencyResolutionReason.VersionMismatch;
    }

    /// <summary>
    /// Benchmarks creating a DependencyResolutionException with just a message.
    /// </summary>
    [Benchmark]
    public DependencyResolutionException Create_WithMessage()
    {
        return new DependencyResolutionException(_testMessage);
    }

    /// <summary>
    /// Benchmarks creating a DependencyResolutionException with message and reason.
    /// </summary>
    [Benchmark]
    public DependencyResolutionException Create_WithMessageAndReason()
    {
        return new DependencyResolutionException(_testMessage, _testReason);
    }

    /// <summary>
    /// Benchmarks creating a DependencyResolutionException with full details.
    /// </summary>
    [Benchmark]
    public DependencyResolutionException Create_WithFullDetails()
    {
        return new DependencyResolutionException(_testMessage, _testPluginId, _testVersionConstraint, _testReason);
    }

    /// <summary>
    /// Benchmarks adding a single unresolved dependency to an exception.
    /// </summary>
    [Benchmark]
    public DependencyResolutionException AddUnresolvedDependency_Single()
    {
        var exception = new DependencyResolutionException(_testMessage);
        return exception.AddUnresolvedDependency("TestPlugin.Dependency");
    }

    /// <summary>
    /// Benchmarks adding multiple unresolved dependencies to an exception.
    /// </summary>
    [Benchmark]
    public DependencyResolutionException AddUnresolvedDependency_Multiple()
    {
        var exception = new DependencyResolutionException(_testMessage);
        exception.AddUnresolvedDependency("TestPlugin.Core");
        exception.AddUnresolvedDependency("TestPlugin.Logging");
        exception.AddUnresolvedDependency("TestPlugin.Data");
        exception.AddUnresolvedDependency("TestPlugin.Network");
        return exception;
    }

    /// <summary>
    /// Benchmarks calling ToString() on an exception with no unresolved dependencies.
    /// </summary>
    [Benchmark]
    public string ToString_NoDependencies()
    {
        var exception = new DependencyResolutionException(_testMessage);
        return exception.ToString();
    }

    /// <summary>
    /// Benchmarks calling ToString() on an exception with unresolved dependencies.
    /// </summary>
    [Benchmark]
    public string ToString_WithDependencies()
    {
        var exception = new DependencyResolutionException(_testMessage);
        exception.AddUnresolvedDependency("TestPlugin.Core");
        exception.AddUnresolvedDependency("TestPlugin.Logging");
        return exception.ToString();
    }

    /// <summary>
    /// Benchmarks setting and getting the DependencyPluginId property.
    /// </summary>
    [Benchmark]
    public Guid DependencyPluginId_GetSet()
    {
        var exception = new DependencyResolutionException(_testMessage);
        exception.DependencyPluginId = _testPluginId;
        return exception.DependencyPluginId;
    }

    /// <summary>
    /// Benchmarks setting and getting the VersionConstraint property.
    /// </summary>
    [Benchmark]
    public string VersionConstraint_GetSet()
    {
        var exception = new DependencyResolutionException(_testMessage);
        exception.VersionConstraint = _testVersionConstraint;
        return exception.VersionConstraint;
    }

    /// <summary>
    /// Benchmarks setting and getting the Reason property.
    /// </summary>
    [Benchmark]
    public DependencyResolutionReason Reason_GetSet()
    {
        var exception = new DependencyResolutionException(_testMessage);
        exception.Reason = _testReason;
        return exception.Reason;
    }
}