#nullable enable
using FluentAssertions;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Exceptions;
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Provides unit tests for the <see cref="DependencyResolutionService"/> class.
/// Tests dependency resolution, validation, and circular dependency detection functionality.
/// </summary>
public sealed class DependencyResolutionServiceTests
{
	/// <summary>
	/// Mock service for loading plugins during testing.
	/// </summary>
	internal readonly Mock<IPluginLoaderService> _mockLoaderService;

	/// <summary>
	/// System under test - the dependency resolution service being tested.
	/// </summary>
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

	/// <summary>
	/// Tests that the constructor throws <see cref="ArgumentNullException"/> when provided with a null plugin loader service.
	/// </summary>
	[Fact]
	public void Constructor_WithNullLoaderService_ThrowsArgumentNullException()
	{
		var act = () => new DependencyResolutionService(null!);

		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("pluginLoaderService");
	}

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ResolveDependenciesAsync"/> throws <see cref="ArgumentNullException"/> when provided with a null plugin.
	/// </summary>
	[Fact]
	public async Task ResolveDependenciesAsync_WithNullPlugin_ThrowsArgumentNullException()
	{
		var act = () => _sut.ResolveDependenciesAsync(null!);

		await act.Should().ThrowAsync<ArgumentNullException>()
			.WithParameterName("plugin");
	}

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ResolveDependenciesAsync"/> returns an empty list when the plugin has no dependencies.
	/// </summary>
	[Fact]
	public async Task ResolveDependenciesAsync_WithPluginHavingNoDependencies_ReturnsEmptyList()
	{
		var plugin = CreatePlugin(Guid.NewGuid());

		var result = await _sut.ResolveDependenciesAsync(plugin);

		result.Should().BeEmpty();
	}

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ResolveDependenciesAsync"/> correctly resolves a single dependency.
	/// </summary>
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

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ResolveDependenciesAsync"/> correctly resolves multiple dependencies.
	/// </summary>
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

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ResolveDependenciesAsync"/> returns the cached result on subsequent calls.
	/// </summary>
	[Fact]
	public async Task ResolveDependenciesAsync_WithCachedResult_ReturnsCache()
	{
		var pluginId = Guid.NewGuid();
		var plugin = CreatePlugin(pluginId);

		var result1 = await _sut.ResolveDependenciesAsync(plugin);
		var result2 = await _sut.ResolveDependenciesAsync(plugin);

		result1.Should().BeSameAs(result2);
	}

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ValidateDependenciesAsync"/> throws <see cref="ArgumentNullException"/> when provided with a null plugin.
	/// </summary>
	[Fact]
	public async Task ValidateDependenciesAsync_WithNullPlugin_ThrowsArgumentNullException()
	{
		var act = () => _sut.ValidateDependenciesAsync(null!);

		await act.Should().ThrowAsync<ArgumentNullException>()
			.WithParameterName("plugin");
	}

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ValidateDependenciesAsync"/> returns true when all required dependencies are satisfied.
	/// </summary>
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

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ValidateDependenciesAsync"/> returns false when a required dependency is missing.
	/// </summary>
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

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ValidateDependenciesAsync"/> returns true when an optional dependency is missing.
	/// </summary>
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

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.HasCircularDependenciesAsync"/> throws <see cref="ArgumentNullException"/> when provided with a null plugin.
	/// </summary>
	[Fact]
	public async Task HasCircularDependenciesAsync_WithNullPlugin_ThrowsArgumentNullException()
	{
		var act = () => _sut.HasCircularDependenciesAsync(null!);

		await act.Should().ThrowAsync<ArgumentNullException>()
			.WithParameterName("plugin");
	}

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.HasCircularDependenciesAsync"/> returns false when there are no circular dependencies.
	/// </summary>
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

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ResolveSingleDependencyAsync"/> returns the dependency plugin when it's available.
	/// </summary>
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

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ResolveSingleDependencyAsync"/> returns null when the dependency is missing.
	/// </summary>
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

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.GetDependentsAsync"/> returns plugins that depend on the given plugin.
	/// </summary>
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

	/// <summary>
	/// Tests that <see cref="DependencyResolutionService.ClearDependencyCacheAsync"/> clears the dependency cache.
	/// </summary>
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