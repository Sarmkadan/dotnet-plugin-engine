#nullable enable
using FluentAssertions;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Exceptions;
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;
using Xunit;

namespace PluginEngine.Tests;

public sealed class DependencyResolutionServiceTests
{
    private readonly Mock<IPluginLoaderService> _mockLoaderService;
    private readonly DependencyResolutionService _sut;

    public DependencyResolutionServiceTests()
    {
        _mockLoaderService = new Mock<IPluginLoaderService>();
        _sut = new DependencyResolutionService(_mockLoaderService.Object);
    }

    private static Plugin CreatePlugin(Guid id, string name = "TestPlugin", string version = "1.0.0")
    {
        return new Plugin
        {
            Id = id,
            Name = name,
            Version = version,
            AssemblyPath = $"/plugins/{name}.dll"
        };
    }

    [Fact]
    public void Constructor_WithNullLoaderService_ThrowsArgumentNullException()
    {
        var act = () => new DependencyResolutionService(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("pluginLoaderService");
    }

    [Fact]
    public async Task ResolveDependenciesAsync_WithNullPlugin_ThrowsArgumentNullException()
    {
        var act = () => _sut.ResolveDependenciesAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("plugin");
    }

    [Fact]
    public async Task ResolveDependenciesAsync_WithPluginHavingNoDependencies_ReturnsEmptyList()
    {
        var plugin = CreatePlugin(Guid.NewGuid());

        var result = await _sut.ResolveDependenciesAsync(plugin);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveDependenciesAsync_WithSingleDependency_ResolvesDependency()
    {
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();
        var plugin = CreatePlugin(pluginId);
        var dependency = new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.0.0"
        };
        plugin.AddDependency(dependency);

        var depPlugin = CreatePlugin(depId, "Dependency", "1.0.0");
        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { depPlugin });

        var result = await _sut.ResolveDependenciesAsync(plugin);

        result.Should().ContainSingle()
            .Which.Id.Should().Be(depId);
    }

    [Fact]
    public async Task ResolveDependenciesAsync_WithMultipleDependencies_ResolvesAll()
    {
        var pluginId = Guid.NewGuid();
        var dep1Id = Guid.NewGuid();
        var dep2Id = Guid.NewGuid();

        var plugin = CreatePlugin(pluginId);
        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = dep1Id,
            MinimumVersion = "1.0.0"
        });
        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = dep2Id,
            MinimumVersion = "1.0.0"
        });

        var dep1 = CreatePlugin(dep1Id, "Dep1");
        var dep2 = CreatePlugin(dep2Id, "Dep2");

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { dep1, dep2 });

        var result = await _sut.ResolveDependenciesAsync(plugin);

        result.Should().HaveCount(2)
            .And.Contain(d => d.Id == dep1Id)
            .And.Contain(d => d.Id == dep2Id);
    }

    [Fact]
    public async Task ResolveDependenciesAsync_WithCachedResult_ReturnsCache()
    {
        var pluginId = Guid.NewGuid();
        var plugin = CreatePlugin(pluginId);

        var result1 = await _sut.ResolveDependenciesAsync(plugin);
        var result2 = await _sut.ResolveDependenciesAsync(plugin);

        result1.Should().BeSameAs(result2);
    }

    [Fact]
    public async Task ValidateDependenciesAsync_WithNullPlugin_ThrowsArgumentNullException()
    {
        var act = () => _sut.ValidateDependenciesAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("plugin");
    }

    [Fact]
    public async Task ValidateDependenciesAsync_WithAllRequiredDependenciesSatisfied_ReturnsTrue()
    {
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();
        var plugin = CreatePlugin(pluginId);
        var dependency = new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.0.0",
            IsOptional = false
        };
        plugin.AddDependency(dependency);

        var depPlugin = CreatePlugin(depId);
        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { depPlugin });

        var result = await _sut.ValidateDependenciesAsync(plugin);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateDependenciesAsync_WithMissingRequiredDependency_ReturnsFalse()
    {
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();
        var plugin = CreatePlugin(pluginId);
        var dependency = new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.0.0",
            IsOptional = false
        };
        plugin.AddDependency(dependency);

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Plugin>());

        var result = await _sut.ValidateDependenciesAsync(plugin);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateDependenciesAsync_WithMissingOptionalDependency_ReturnsTrue()
    {
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();
        var plugin = CreatePlugin(pluginId);
        var dependency = new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.0.0",
            IsOptional = true
        };
        plugin.AddDependency(dependency);

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Plugin>());

        var result = await _sut.ValidateDependenciesAsync(plugin);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasCircularDependenciesAsync_WithNullPlugin_ThrowsArgumentNullException()
    {
        var act = () => _sut.HasCircularDependenciesAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("plugin");
    }

    [Fact]
    public async Task HasCircularDependenciesAsync_WithNoCircularDependencies_ReturnsFalse()
    {
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();
        var plugin = CreatePlugin(pluginId);
        var dependency = new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.0.0"
        };
        plugin.AddDependency(dependency);

        var depPlugin = CreatePlugin(depId);

        _mockLoaderService
            .Setup(x => x.GetLoadedPluginAsync(depId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(depPlugin);

        var result = await _sut.HasCircularDependenciesAsync(plugin);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveSingleDependencyAsync_WithValidDependency_ReturnsDependencyPlugin()
    {
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();
        var dependency = new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.0.0"
        };

        var depPlugin = CreatePlugin(depId);
        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { depPlugin });

        var result = await _sut.ResolveSingleDependencyAsync(dependency);

        result.Should().NotBeNull()
            .And.BeSameAs(depPlugin);
    }

    [Fact]
    public async Task ResolveSingleDependencyAsync_WithMissingDependency_ReturnsNull()
    {
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();
        var dependency = new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.0.0"
        };

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Plugin>());

        var result = await _sut.ResolveSingleDependencyAsync(dependency);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDependentsAsync_ReturnsPluginsThatDependOnGivenPlugin()
    {
        var targetId = Guid.NewGuid();
        var dependent1Id = Guid.NewGuid();
        var dependent2Id = Guid.NewGuid();

        var dependent1 = CreatePlugin(dependent1Id, "Dependent1");
        dependent1.AddDependency(new PluginDependency
        {
            PluginId = dependent1Id,
            DependencyPluginId = targetId,
            MinimumVersion = "1.0.0"
        });

        var dependent2 = CreatePlugin(dependent2Id, "Dependent2");
        dependent2.AddDependency(new PluginDependency
        {
            PluginId = dependent2Id,
            DependencyPluginId = targetId,
            MinimumVersion = "1.0.0"
        });

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { dependent1, dependent2 });

        var result = await _sut.GetDependentsAsync(targetId);

        result.Should().HaveCount(2)
            .And.Contain(d => d.Id == dependent1Id)
            .And.Contain(d => d.Id == dependent2Id);
    }

    [Fact]
    public async Task ClearDependencyCacheAsync_ClearsTheCache()
    {
        var pluginId = Guid.NewGuid();
        var plugin = CreatePlugin(pluginId);

        var preClear = await _sut.ResolveDependenciesAsync(plugin);
        var preClearAgain = await _sut.ResolveDependenciesAsync(plugin);
        preClear.Should().BeSameAs(preClearAgain);

        await _sut.ClearDependencyCacheAsync();
        var postClear = await _sut.ResolveDependenciesAsync(plugin);

        postClear.Should().NotBeSameAs(preClear);
    }
}
