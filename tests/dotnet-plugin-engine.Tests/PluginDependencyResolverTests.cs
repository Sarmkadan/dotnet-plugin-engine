#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Unit tests for <see cref="PluginDependencyResolver"/>.
/// </summary>
public sealed class PluginDependencyResolverTests
{
    private readonly Mock<IPluginLoaderService> _mockLoader = new();
    private readonly Mock<ILogger<PluginDependencyResolver>> _mockLogger = new();

    private PluginDependencyResolver CreateResolver() =>
        new(_mockLoader.Object, _mockLogger.Object);

    private static Plugin MakePlugin(string name, string version = "1.0.0")
        => new() { Id = Guid.NewGuid(), Name = name, Version = version, AssemblyPath = $"/{name}.dll" };

    // ── GetInstallOrderAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetInstallOrderAsync_NoDependencies_ReturnsSinglePlugin()
    {
        var plugin   = MakePlugin("Standalone");
        var resolver = CreateResolver();

        var result = await resolver.GetInstallOrderAsync([plugin]);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].Id.Should().Be(plugin.Id);
    }

    [Fact]
    public async Task GetInstallOrderAsync_LinearChain_DependencyComesFirst()
    {
        var dep    = MakePlugin("CoreLib");
        var plugin = MakePlugin("AppPlugin");
        plugin.AddDependency(new PluginDependency
        {
            PluginId = plugin.Id, DependencyPluginId = dep.Id, MinimumVersion = "1.0.0"
        });

        var resolver = CreateResolver();
        var result   = await resolver.GetInstallOrderAsync([plugin, dep]);

        result.Success.Should().BeTrue();
        result.Data!.Should().HaveCount(2);
        result.Data![0].Id.Should().Be(dep.Id,    because: "dep must be installed before plugin");
        result.Data![1].Id.Should().Be(plugin.Id, because: "plugin depends on dep");
    }

    [Fact]
    public async Task GetInstallOrderAsync_CircularDependency_ReturnsFailure()
    {
        var a = MakePlugin("A");
        var b = MakePlugin("B");

        a.AddDependency(new PluginDependency { PluginId = a.Id, DependencyPluginId = b.Id, MinimumVersion = "1.0.0" });
        b.AddDependency(new PluginDependency { PluginId = b.Id, DependencyPluginId = a.Id, MinimumVersion = "1.0.0" });

        var resolver = CreateResolver();
        var result   = await resolver.GetInstallOrderAsync([a, b]);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(1002);
    }

    [Fact]
    public async Task GetInstallOrderAsync_DiamondDependency_ProducesValidOrder()
    {
        // A → B, A → C, B → D, C → D  (diamond)
        var d = MakePlugin("D");
        var b = MakePlugin("B");
        var c = MakePlugin("C");
        var a = MakePlugin("A");

        b.AddDependency(new PluginDependency { PluginId = b.Id, DependencyPluginId = d.Id, MinimumVersion = "1.0.0" });
        c.AddDependency(new PluginDependency { PluginId = c.Id, DependencyPluginId = d.Id, MinimumVersion = "1.0.0" });
        a.AddDependency(new PluginDependency { PluginId = a.Id, DependencyPluginId = b.Id, MinimumVersion = "1.0.0" });
        a.AddDependency(new PluginDependency { PluginId = a.Id, DependencyPluginId = c.Id, MinimumVersion = "1.0.0" });

        var resolver = CreateResolver();
        var result   = await resolver.GetInstallOrderAsync([a, b, c, d]);

        result.Success.Should().BeTrue();
        result.Data!.Should().HaveCount(4);

        // D must appear before both B and C, which must appear before A
        var order = result.Data!;
        order.IndexOf(order.First(p => p.Id == d.Id)).Should().BeLessThan(
            order.IndexOf(order.First(p => p.Id == a.Id)));
    }

    // ── FindConflictsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task FindConflictsAsync_NoConflicts_ReturnsEmptyList()
    {
        var shared = MakePlugin("SharedLib");
        var a      = MakePlugin("A");
        var b      = MakePlugin("B");

        a.AddDependency(new PluginDependency { PluginId = a.Id, DependencyPluginId = shared.Id, MinimumVersion = "1.0.0", MaximumVersion = "2.0.0" });
        b.AddDependency(new PluginDependency { PluginId = b.Id, DependencyPluginId = shared.Id, MinimumVersion = "1.5.0", MaximumVersion = "2.0.0" });

        var resolver = CreateResolver();
        var result   = await resolver.FindConflictsAsync([a, b, shared]);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task FindConflictsAsync_IncompatibleConstraints_DetectsConflict()
    {
        var shared = MakePlugin("SharedLib");
        var a      = MakePlugin("A");
        var b      = MakePlugin("B");

        // A needs >= 2.0.0, B needs <= 1.0.0 — irreconcilable
        a.AddDependency(new PluginDependency { PluginId = a.Id, DependencyPluginId = shared.Id, MinimumVersion = "2.0.0" });
        b.AddDependency(new PluginDependency { PluginId = b.Id, DependencyPluginId = shared.Id, MinimumVersion = "1.0.0", MaximumVersion = "1.0.0" });

        var resolver = CreateResolver();
        var result   = await resolver.FindConflictsAsync([a, b, shared]);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].DependencyPluginId.Should().Be(shared.Id);
        result.Data![0].ConflictingRequirements.Should().HaveCount(2);
    }

    // ── BuildResolutionPlanAsync ─────────────────────────────────────────────

    [Fact]
    public async Task BuildResolutionPlanAsync_PluginNotLoaded_ReturnsFailure()
    {
        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plugin?)null);

        var resolver = CreateResolver();
        var result   = await resolver.BuildResolutionPlanAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(404);
    }

    [Fact]
    public async Task BuildResolutionPlanAsync_NoDependencies_PlanHasOneStep()
    {
        var plugin = MakePlugin("Standalone");

        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
        _mockLoader
            .Setup(l => l.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([plugin]);

        var resolver = CreateResolver();
        var result   = await resolver.BuildResolutionPlanAsync(plugin.Id);

        result.Success.Should().BeTrue();
        result.Data!.Steps.Should().HaveCount(1);
        result.Data!.Steps[0].PluginName.Should().Be("Standalone");
        result.Data!.IsExecutable.Should().BeTrue();
    }

    [Fact]
    public async Task BuildResolutionPlanAsync_WithSatisfiedDependency_StepMarkedAlreadySatisfied()
    {
        var dep    = MakePlugin("Dep", "2.0.0");
        var plugin = MakePlugin("App");
        plugin.AddDependency(new PluginDependency
        {
            PluginId           = plugin.Id,
            DependencyPluginId = dep.Id,
            MinimumVersion     = "1.0.0"
        });

        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
        _mockLoader
            .Setup(l => l.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([plugin, dep]);

        var resolver = CreateResolver();
        var result   = await resolver.BuildResolutionPlanAsync(plugin.Id);

        result.Success.Should().BeTrue();
        result.Data!.Steps.Should().HaveCount(2);
        result.Data!.IsExecutable.Should().BeTrue();

        var depStep = result.Data!.Steps.First(s => s.PluginId == dep.Id);
        depStep.Action.Should().Be(ResolutionAction.AlreadySatisfied);
    }

    [Fact]
    public async Task BuildResolutionPlanAsync_WithConflict_PlanIsNotExecutable()
    {
        var shared = MakePlugin("SharedLib");
        var a      = MakePlugin("A");
        var b      = MakePlugin("B");

        a.AddDependency(new PluginDependency { PluginId = a.Id, DependencyPluginId = shared.Id, MinimumVersion = "3.0.0" });
        b.AddDependency(new PluginDependency { PluginId = b.Id, DependencyPluginId = shared.Id, MinimumVersion = "1.0.0", MaximumVersion = "1.5.0" });
        // a also depends on b so it's in the transitive set
        a.AddDependency(new PluginDependency { PluginId = a.Id, DependencyPluginId = b.Id, MinimumVersion = "1.0.0" });

        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(a.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(a);
        _mockLoader
            .Setup(l => l.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([a, b, shared]);

        var resolver = CreateResolver();
        var result   = await resolver.BuildResolutionPlanAsync(a.Id);

        result.Success.Should().BeTrue();
        result.Data!.IsExecutable.Should().BeFalse();
        result.Data!.Conflicts.Should().NotBeEmpty();
    }
}
