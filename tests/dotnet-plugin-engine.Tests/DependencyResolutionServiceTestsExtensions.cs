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
    /// <param name="_">The test fixture instance (unused).</param>
    /// <param name="id">The unique identifier for the plugin.</param>
    /// <param name="name">The name of the plugin. Defaults to "TestPlugin".</param>
    /// <param name="version">The version of the plugin. Defaults to "1.0.0".</param>
    /// <returns>A new <see cref="Plugin"/> instance with the specified properties.</returns>
    /// <exception cref="ArgumentException"><paramref name="id"/> is <see cref="Guid.Empty"/>.</exception>
    public static Plugin CreateTestPlugin(this DependencyResolutionServiceTests _, Guid id, string name = "TestPlugin", string version = "1.0.0")
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(version);

        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);

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
    /// <param name="_">The test fixture instance (unused).</param>
    /// <param name="pluginId">The ID of the plugin that has the dependency.</param>
    /// <param name="dependencyPluginId">The ID of the plugin that is depended upon.</param>
    /// <param name="minimumVersion">The minimum required version of the dependency. Defaults to "1.0.0".</param>
    /// <param name="isOptional">Whether the dependency is optional.</param>
    /// <returns>A new <see cref="PluginDependency"/> instance with the specified properties.</returns>
    /// <exception cref="ArgumentException"><paramref name="minimumVersion"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pluginId"/> or <paramref name="dependencyPluginId"/> is <see cref="Guid.Empty"/>.</exception>
    public static PluginDependency CreateTestDependency(this DependencyResolutionServiceTests _, Guid pluginId, Guid dependencyPluginId, string minimumVersion = "1.0.0", bool isOptional = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(minimumVersion);
        ArgumentOutOfRangeException.ThrowIfEqual(pluginId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(dependencyPluginId, Guid.Empty);

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
    /// <param name="tests">The test fixture instance.</param>
    /// <param name="plugins">The plugins to return when <see cref="IDependencyLoaderService.GetAllLoadedPluginsAsync"/> is called.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plugins"/> is <see langword="null"/>.</exception>
    public static void SetupLoaderService(this DependencyResolutionServiceTests tests, params Plugin[] plugins)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plugins);

        tests._mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugins);
    }

    /// <summary>
    /// Sets up the mock loader service to return a specific plugin by ID.
    /// </summary>
    /// <param name="tests">The test fixture instance.</param>
    /// <param name="plugin">The plugin to return when <see cref="IDependencyLoaderService.GetLoadedPluginAsync"/> is called.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static void SetupLoaderServiceForPlugin(this DependencyResolutionServiceTests tests, Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(plugin);

        tests._mockLoaderService
            .Setup(x => x.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
    }

    /// <summary>
    /// Asserts that the plugin has the expected number of dependencies.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <param name="expectedCount">The expected number of dependencies.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    public static void ShouldHaveDependencyCount(this Plugin plugin, int expectedCount)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        plugin.Dependencies.Should().HaveCount(expectedCount);
    }

    /// <summary>
    /// Asserts that the plugin has a dependency with the specified plugin ID.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <param name="dependencyPluginId">The ID of the dependency to find.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dependencyPluginId"/> is <see cref="Guid.Empty"/>.</exception>
    public static void ShouldHaveDependencyOn(this Plugin plugin, Guid dependencyPluginId)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentOutOfRangeException.ThrowIfEqual(dependencyPluginId, Guid.Empty);

        plugin.Dependencies.Should().Contain(d => d.DependencyPluginId == dependencyPluginId);
    }

    /// <summary>
    /// Asserts that the plugin has a dependency with the specified plugin ID and version.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <param name="dependencyPluginId">The ID of the dependency to find.</param>
    /// <param name="minimumVersion">The minimum version that the dependency should have.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plugin"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dependencyPluginId"/> is <see cref="Guid.Empty"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="minimumVersion"/> is <see langword="null"/> or empty.</exception>
    public static void ShouldHaveDependencyOn(this Plugin plugin, Guid dependencyPluginId, string minimumVersion)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentOutOfRangeException.ThrowIfEqual(dependencyPluginId, Guid.Empty);
        ArgumentException.ThrowIfNullOrEmpty(minimumVersion);

        plugin.Dependencies.Should().Contain(d => d.DependencyPluginId == dependencyPluginId && d.MinimumVersion == minimumVersion);
    }

    /// <summary>
    /// Asserts that the dependency resolution result contains plugins with the specified IDs.
    /// </summary>
    /// <param name="resolvedPlugins">The resolved plugins to check.</param>
    /// <param name="expectedIds">The expected plugin IDs.</param>
    /// <exception cref="ArgumentNullException"><paramref name="resolvedPlugins"/> is <see langword="null"/>.</exception>
    public static void ShouldContainPluginIds(this IEnumerable<Plugin> resolvedPlugins, params Guid[] expectedIds)
    {
        ArgumentNullException.ThrowIfNull(resolvedPlugins);

        resolvedPlugins.Should().HaveCount(expectedIds.Length);
        foreach (var id in expectedIds)
        {
            resolvedPlugins.Should().Contain(p => p.Id == id);
        }
    }

    /// <summary>
    /// Asserts that the validation result is true.
    /// </summary>
    /// <param name="validationResult">The validation result to check.</param>
    /// <exception cref="ArgumentException"><paramref name="validationResult"/> is <see langword="false"/>.</exception>
    public static void ShouldBeValid(this bool validationResult)
    {
        validationResult.Should().BeTrue();
    }

    /// <summary>
    /// Asserts that the validation result is false.
    /// </summary>
    /// <param name="validationResult">The validation result to check.</param>
    /// <exception cref="ArgumentException"><paramref name="validationResult"/> is <see langword="true"/>.</exception>
    public static void ShouldBeInvalid(this bool validationResult)
    {
        validationResult.Should().BeFalse();
    }

    /// <summary>
    /// Creates a plugin with multiple dependencies.
    /// </summary>
    /// <param name="tests">The test fixture instance.</param>
    /// <param name="pluginId">The ID of the plugin to create.</param>
    /// <param name="dependencies">The dependencies to add to the plugin.</param>
    /// <returns>A new <see cref="Plugin"/> instance with the specified dependencies.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pluginId"/> is <see cref="Guid.Empty"/>.</exception>
    public static Plugin CreatePluginWithDependencies(this DependencyResolutionServiceTests tests, Guid pluginId, params (Guid depId, string version)[] dependencies)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentOutOfRangeException.ThrowIfEqual(pluginId, Guid.Empty);

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
    /// <param name="resolvedDependency">The resolved dependency to check.</param>
    /// <param name="expectedPlugin">The expected plugin that should be resolved.</param>
    /// <exception cref="ArgumentNullException"><paramref name="resolvedDependency"/> or <paramref name="expectedPlugin"/> is <see langword="null"/>.</exception>
    public static void ShouldResolveTo(this Plugin resolvedDependency, Plugin expectedPlugin)
    {
        ArgumentNullException.ThrowIfNull(resolvedDependency);
        ArgumentNullException.ThrowIfNull(expectedPlugin);

        resolvedDependency.Should().BeSameAs(expectedPlugin);
    }
}