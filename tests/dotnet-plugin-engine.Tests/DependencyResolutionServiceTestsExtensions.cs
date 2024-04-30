#nullable enable

using FluentAssertions;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Implementations;
using Xunit;

namespace PluginEngine.Tests;

public static class DependencyResolutionServiceTestsExtensions
{
    /// <summary>
    /// Creates a test plugin with the specified parameters.
    /// </summary>
    public static Plugin CreateTestPlugin(this DependencyResolutionServiceTests _, Guid id, string name = "TestPlugin", string version = "1.0.0")
    {
        return new Plugin
        {
            Id = id,
            Name = name,
            Version = version,
            AssemblyPath = $@"/plugins/{name}.dll"
        };
    }

    /// <summary>
    /// Creates a plugin dependency with the specified parameters.
    /// </summary>
    public static PluginDependency CreateTestDependency(this DependencyResolutionServiceTests _, Guid pluginId, Guid dependencyPluginId, string minimumVersion = "1.0.0", bool isOptional = false)
    {
        return new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = dependencyPluginId,
            MinimumVersion = minimumVersion,
            IsOptional = isOptional
        };
    }

    /// <summary>
    /// Sets up the mock loader service to return the specified plugins.
    /// </summary>
    public static void SetupLoaderService(this DependencyResolutionServiceTests tests, params Plugin[] plugins)
    {
        tests._mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugins);
    }

    /// <summary>
    /// Sets up the mock loader service to return a specific plugin by ID.
    /// </summary>
    public static void SetupLoaderServiceForPlugin(this DependencyResolutionServiceTests tests, Plugin plugin)
    {
        tests._mockLoaderService
            .Setup(x => x.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
    }

    /// <summary>
    /// Asserts that the plugin has the expected number of dependencies.
    /// </summary>
    public static void ShouldHaveDependencyCount(this Plugin plugin, int expectedCount)
    {
        plugin.Dependencies.Should().HaveCount(expectedCount);
    }

    /// <summary>
    /// Asserts that the plugin has a dependency with the specified plugin ID.
    /// </summary>
    public static void ShouldHaveDependencyOn(this Plugin plugin, Guid dependencyPluginId)
    {
        plugin.Dependencies.Should().Contain(d => d.DependencyPluginId == dependencyPluginId);
    }

    /// <summary>
    /// Asserts that the plugin has a dependency with the specified plugin ID and version.
    /// </summary>
    public static void ShouldHaveDependencyOn(this Plugin plugin, Guid dependencyPluginId, string minimumVersion)
    {
        plugin.Dependencies.Should().Contain(d => d.DependencyPluginId == dependencyPluginId && d.MinimumVersion == minimumVersion);
    }

    /// <summary>
    /// Asserts that the dependency resolution result contains plugins with the specified IDs.
    /// </summary>
    public static void ShouldContainPluginIds(this IEnumerable<Plugin> resolvedPlugins, params Guid[] expectedIds)
    {
        resolvedPlugins.Should().HaveCount(expectedIds.Length);
        foreach (var id in expectedIds)
        {
            resolvedPlugins.Should().Contain(p => p.Id == id);
        }
    }

    /// <summary>
    /// Asserts that the validation result is true.
    /// </summary>
    public static void ShouldBeValid(this bool validationResult)
    {
        validationResult.Should().BeTrue();
    }

    /// <summary>
    /// Asserts that the validation result is false.
    /// </summary>
    public static void ShouldBeInvalid(this bool validationResult)
    {
        validationResult.Should().BeFalse();
    }

    /// <summary>
    /// Creates a plugin with multiple dependencies.
    /// </summary>
    public static Plugin CreatePluginWithDependencies(this DependencyResolutionServiceTests tests, Guid pluginId, params (Guid depId, string version)[] dependencies)
    {
        var plugin = tests.CreateTestPlugin(pluginId);
        foreach (var (depId, version) in dependencies)
        {
            plugin.AddDependency(tests.CreateTestDependency(pluginId, depId, version));
        }
        return plugin;
    }

    /// <summary>
    /// Asserts that the resolved dependency is the expected plugin.
    /// </summary>
    public static void ShouldResolveTo(this Plugin resolvedDependency, Plugin expectedPlugin)
    {
        resolvedDependency.Should().BeSameAs(expectedPlugin);
    }
}