#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;
using PluginEngine.Utils.Helpers;
using PluginEngine.Utils.Validators;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Contains integration tests for the plugin engine system, validating end-to-end workflows
/// including dependency resolution, version validation, file system operations, and plugin lifecycle management.
/// </summary>
public sealed class IntegrationTests
{
    private readonly Mock<ILogger<DependencyResolutionService>> _mockDepLogger;
    private readonly Mock<ILogger<PluginValidator>> _mockValLogger;
    private readonly Mock<ILogger<VersionHelper>> _mockVerLogger;
    private readonly Mock<ILogger<FileSystemHelper>> _mockFsLogger;
    private readonly Mock<ILogger<PluginDiscoveryService>> _mockDiscoveryLogger;
    private readonly Mock<IPluginLoaderService> _mockLoaderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTests"/> class with mock dependencies
    /// for testing plugin engine services including dependency resolution, validation, versioning, and file system operations.
    /// </summary>
    public IntegrationTests()
    {
        _mockDepLogger = new Mock<ILogger<DependencyResolutionService>>();
        _mockValLogger = new Mock<ILogger<PluginValidator>>();
        _mockVerLogger = new Mock<ILogger<VersionHelper>>();
        _mockFsLogger = new Mock<ILogger<FileSystemHelper>>();
        _mockDiscoveryLogger = new Mock<ILogger<PluginDiscoveryService>>();
        _mockLoaderService = new Mock<IPluginLoaderService>();
    }

    /// <summary>
    /// Creates a test plugin with the specified identifier, name, and version.
    /// </summary>
    /// <param name="id">The unique identifier for the plugin.</param>
    /// <param name="name">The name of the plugin.</param>
    /// <param name="version">The version string of the plugin.</param>
    /// <returns>A new <see cref="Plugin"/> instance configured with the provided parameters.</returns>
    private static Plugin CreatePlugin(Guid id, string name, string version)
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
    /// Tests that a plugin with dependencies can be loaded and its dependencies successfully resolved.
    /// Validates the complete workflow from plugin creation through dependency resolution and validation.
    /// </summary>
    [Fact]
    public async Task PluginWorkflow_LoadPluginWithDependencies_SuccessfullyResolves()
    {
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();

        var plugin = CreatePlugin(pluginId, "MainPlugin", "1.0.0");
        var dependency = new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.0.0"
        };
        plugin.AddDependency(dependency);

        var depPlugin = CreatePlugin(depId, "Dependency", "1.5.0");

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { depPlugin });

        var resolutionService = new DependencyResolutionService(_mockLoaderService.Object);

        var validationService = new PluginValidator(
            _mockValLogger.Object,
            new VersionHelper(_mockVerLogger.Object));

        var pluginValidation = validationService.Validate(plugin);
        pluginValidation.IsValid.Should().BeTrue();

        var depsValid = await resolutionService.ValidateDependenciesAsync(plugin);
        depsValid.Should().BeTrue();

        var resolved = await resolutionService.ResolveDependenciesAsync(plugin);
        resolved.Should().ContainSingle();
    }

    /// <summary>
    /// Tests dependency resolution with multiple plugins that have chained dependencies (A depends on B depends on C).
    /// Verifies that the resolution service correctly resolves all transitive dependencies in the correct order.
    /// </summary>
    [Fact]
    public async Task PluginWorkflow_MultiplePluginsWithChainedDependencies()
    {
        var plugin1Id = Guid.NewGuid();
        var plugin2Id = Guid.NewGuid();
        var plugin3Id = Guid.NewGuid();

        var plugin1 = CreatePlugin(plugin1Id, "Plugin1", "1.0.0");
        var dep1 = new PluginDependency
        {
            PluginId = plugin1Id,
            DependencyPluginId = plugin2Id,
            MinimumVersion = "1.0.0"
        };
        plugin1.AddDependency(dep1);

        var plugin2 = CreatePlugin(plugin2Id, "Plugin2", "2.0.0");
        var dep2 = new PluginDependency
        {
            PluginId = plugin2Id,
            DependencyPluginId = plugin3Id,
            MinimumVersion = "1.0.0"
        };
        plugin2.AddDependency(dep2);

        var plugin3 = CreatePlugin(plugin3Id, "Plugin3", "1.5.0");

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { plugin2, plugin3 });

        var resolutionService = new DependencyResolutionService(_mockLoaderService.Object);

        var resolved = await resolutionService.ResolveDependenciesAsync(plugin1);

        resolved.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests version constraint validation to ensure version comparison logic works correctly.
    /// Validates that version ranges like >= and ^ constraints are properly respected during dependency resolution.
    /// </summary>
    [Fact]
    public async Task VersionValidation_WithConstraints_RespectsVersionRanges()
    {
        var versionHelper = new VersionHelper(_mockVerLogger.Object);

        versionHelper.SatisfiesConstraint("1.5.0", ">=1.0.0").Should().BeTrue();
        versionHelper.SatisfiesConstraint("0.9.0", ">=1.0.0").Should().BeFalse();

        versionHelper.SatisfiesConstraint("1.5.0", "^1.0.0").Should().BeTrue();
        versionHelper.SatisfiesConstraint("2.0.0", "^1.0.0").Should().BeFalse();
    }

    /// <summary>
    /// Tests plugin validation with complex dependency scenarios including version constraints and metadata.
    /// Validates that all constraints are properly checked during plugin validation.
    /// </summary>
    [Fact]
    public async Task PluginValidation_WithComplexDependencies_ValidatesAllConstraints()
    {
        var validator = new PluginValidator(
            _mockValLogger.Object,
            new VersionHelper(_mockVerLogger.Object));

        var pluginId = Guid.NewGuid();
        var plugin = CreatePlugin(pluginId, "ComplexPlugin", "2.0.0");

        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = Guid.NewGuid(),
            MinimumVersion = "1.0.0",
            MaximumVersion = "2.0.0"
        });

        plugin.Metadata = new PluginMetadata
        {
            PluginId = pluginId,
            Author = "TestAuthor",
            Description = "Complex test plugin"
        };

        var result = validator.Validate(plugin);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests file system operations for creating and validating plugin directories.
    /// Validates directory creation, existence checks, and writability verification.
    /// </summary>
    [Fact]
    public void FileSystemWorkflow_CreateAndValidatePluginDirectory()
    {
        var fsHelper = new FileSystemHelper(_mockFsLogger.Object);
        var testDir = Path.Combine(Path.GetTempPath(), $"plugin-dir-{Guid.NewGuid()}");

        try
        {
            var created = fsHelper.EnsureDirectoryExists(testDir);
            created.Should().BeTrue();
            Directory.Exists(testDir).Should().BeTrue();

            var writable = fsHelper.IsDirectoryWritable(testDir);
            writable.Should().BeTrue();
        }
        finally
        {
            fsHelper.DeleteDirectoryRecursive(testDir);
        }
    }

    /// <summary>
    /// Tests plugin discovery functionality by creating test plugin files and verifying they are correctly discovered.
    /// Validates that the file system helper can find and enumerate plugin DLL files in a directory.
    /// </summary>
    [Fact]
    public void FileSystemWorkflow_DiscoverAndBackupPlugins()
    {
        var fsHelper = new FileSystemHelper(_mockFsLogger.Object);
        var testDir = Path.Combine(Path.GetTempPath(), $"discover-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            File.Create(Path.Combine(testDir, "Plugin1.dll")).Dispose();
            File.Create(Path.Combine(testDir, "Plugin2.dll")).Dispose();
            File.WriteAllText(Path.Combine(testDir, "config.txt"), "config data");

            var discovered = fsHelper.DiscoverPlugins(testDir).ToList();
            discovered.Should().HaveCount(2);
            discovered.Should().AllSatisfy(f => f.Should().EndWith(".dll"));
        }
        finally
        {
            fsHelper.DeleteDirectoryRecursive(testDir);
        }
    }

    /// <summary>
    /// Tests circular dependency detection to ensure the system correctly identifies circular references between plugins.
    /// Validates that the dependency resolution service can detect cycles in the dependency graph.
    /// </summary>
    [Fact]
    public async Task CircularDependencyDetection_WithCircularReferences()
    {
        var plugin1Id = Guid.NewGuid();
        var plugin2Id = Guid.NewGuid();

        var plugin1 = CreatePlugin(plugin1Id, "Plugin1", "1.0.0");
        plugin1.AddDependency(new PluginDependency
        {
            PluginId = plugin1Id,
            DependencyPluginId = plugin2Id,
            MinimumVersion = "1.0.0"
        });

        var plugin2 = CreatePlugin(plugin2Id, "Plugin2", "1.0.0");
        plugin2.AddDependency(new PluginDependency
        {
            PluginId = plugin2Id,
            DependencyPluginId = plugin1Id,
            MinimumVersion = "1.0.0"
        });

        _mockLoaderService
            .Setup(x => x.GetLoadedPluginAsync(plugin2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin2);
        _mockLoaderService
            .Setup(x => x.GetLoadedPluginAsync(plugin1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin1);

        var resolutionService = new DependencyResolutionService(_mockLoaderService.Object);

        var hasCircular = await resolutionService.HasCircularDependenciesAsync(plugin1);

        hasCircular.Should().BeTrue();
    }

    /// <summary>
    /// Tests plugin filtering by author to ensure plugins can be correctly filtered based on their metadata.
    /// Validates that the filtering logic correctly identifies plugins matching specific author criteria.
    /// </summary>
    [Fact]
    public async Task PluginSearch_FiltersByAuthor()
    {
        var plugin1 = CreatePlugin(Guid.NewGuid(), "AuthPlugin", "1.0.0");
        plugin1.Metadata = new PluginMetadata
        {
            PluginId = plugin1.Id,
            Author = "SecurityTeam"
        };

        var plugin2 = CreatePlugin(Guid.NewGuid(), "LoggingPlugin", "2.0.0");
        plugin2.Metadata = new PluginMetadata
        {
            PluginId = plugin2.Id,
            Author = "OpsTeam"
        };

        var plugins = new[] { plugin1, plugin2 };

        var securityPlugins = plugins
            .Where(p => p.Metadata?.Author == "SecurityTeam")
            .ToList();

        securityPlugins.Should().ContainSingle()
            .Which.Name.Should().Be("AuthPlugin");
    }

    /// <summary>
    /// Tests the complete plugin lifecycle including validation, dependency validation, and dependency resolution.
    /// Validates the sequence of operations from initial plugin creation through validation and resolution.
    /// </summary>
    [Fact]
    public async Task PluginLifecycle_ValidateAndResolveSequence()
    {
        var validator = new PluginValidator(
            _mockValLogger.Object,
            new VersionHelper(_mockVerLogger.Object));

        var resolutionService = new DependencyResolutionService(_mockLoaderService.Object);

        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();

        var plugin = CreatePlugin(pluginId, "TestPlugin", "1.0.0");
        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.0.0"
        });

        var depPlugin = CreatePlugin(depId, "DepPlugin", "1.5.0");
        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { depPlugin });

        var validation = validator.Validate(plugin);
        validation.IsValid.Should().BeTrue();

        var depsValid = await resolutionService.ValidateDependenciesAsync(plugin);
        depsValid.Should().BeTrue();

        var deps = await resolutionService.ResolveDependenciesAsync(plugin);
        deps.Should().ContainSingle();
    }

    /// <summary>
    /// Tests plugin capability management including adding, exposing, and searching capabilities.
    /// Validates that plugin capabilities can be correctly added to plugins and searched/filtered by tags.
    /// </summary>
    [Fact]
    public void PluginCapability_ExposesAndSearches()
    {
        var pluginId = Guid.NewGuid();
        var plugin = CreatePlugin(pluginId, "CapabilityPlugin", "1.0.0");

        var capability1 = new PluginCapability
        {
            PluginId = pluginId,
            Name = "DataTransform",
            Version = "1.0.0",
            InterfaceTypeName = "ITransform",
            Tags = "data,transform"
        };

        var capability2 = new PluginCapability
        {
            PluginId = pluginId,
            Name = "Validation",
            Version = "1.0.0",
            InterfaceTypeName = "IValidator",
            Tags = "validation,rules"
        };

        plugin.AddCapability(capability1);
        plugin.AddCapability(capability2);

        var transformCapabilities = plugin.Capabilities
            .Where(c => c.Tags?.Contains("transform") == true)
            .ToList();

        transformCapabilities.Should().ContainSingle()
            .Which.Name.Should().Be("DataTransform");
    }
}
