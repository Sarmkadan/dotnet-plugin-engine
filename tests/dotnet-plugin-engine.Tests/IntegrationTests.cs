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

public sealed class IntegrationTests
{
    private readonly Mock<ILogger<DependencyResolutionService>> _mockDepLogger;
    private readonly Mock<ILogger<PluginValidator>> _mockValLogger;
    private readonly Mock<ILogger<VersionHelper>> _mockVerLogger;
    private readonly Mock<ILogger<FileSystemHelper>> _mockFsLogger;
    private readonly Mock<ILogger<PluginDiscoveryService>> _mockDiscoveryLogger;
    private readonly Mock<IPluginLoaderService> _mockLoaderService;

    public IntegrationTests()
    {
        _mockDepLogger = new Mock<ILogger<DependencyResolutionService>>();
        _mockValLogger = new Mock<ILogger<PluginValidator>>();
        _mockVerLogger = new Mock<ILogger<VersionHelper>>();
        _mockFsLogger = new Mock<ILogger<FileSystemHelper>>();
        _mockDiscoveryLogger = new Mock<ILogger<PluginDiscoveryService>>();
        _mockLoaderService = new Mock<IPluginLoaderService>();
    }

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

    [Fact]
    public async Task VersionValidation_WithConstraints_RespectsVersionRanges()
    {
        var versionHelper = new VersionHelper(_mockVerLogger.Object);

        versionHelper.SatisfiesConstraint("1.5.0", ">=1.0.0").Should().BeTrue();
        versionHelper.SatisfiesConstraint("0.9.0", ">=1.0.0").Should().BeFalse();

        versionHelper.SatisfiesConstraint("1.5.0", "^1.0.0").Should().BeTrue();
        versionHelper.SatisfiesConstraint("2.0.0", "^1.0.0").Should().BeFalse();
    }

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
