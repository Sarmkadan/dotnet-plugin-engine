#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Utils.Helpers;
using PluginEngine.Utils.Validators;
using Xunit;

namespace PluginEngine.Tests;

public sealed class PluginValidatorTests
{
    private readonly Mock<ILogger<PluginValidator>> _mockLogger;
    private readonly VersionHelper _versionHelper;
    private readonly PluginValidator _sut;

    public PluginValidatorTests()
    {
        _mockLogger = new Mock<ILogger<PluginValidator>>();
        var mockVerLogger = new Mock<ILogger<VersionHelper>>();
        _versionHelper = new VersionHelper(mockVerLogger.Object);
        _sut = new PluginValidator(_mockLogger.Object, _versionHelper);
    }

    private static Plugin CreateValidPlugin(string name = "TestPlugin", string version = "1.0.0")
    {
        return new Plugin
        {
            Id = Guid.NewGuid(),
            Name = name,
            Version = version,
            AssemblyPath = "/plugins/test.dll"
        };
    }

    [Fact]
    public void Validate_WithValidPlugin_ReturnsValidResult()
    {
        var plugin = CreateValidPlugin();

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin();
        plugin.Name = string.Empty;

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be empty"));
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin();
        plugin.Name = "   ";

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be empty"));
    }

    [Fact]
    public void Validate_WithNameStartingWithSystemPrefix_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin("System.PluginName");

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("reserved prefixes"));
    }

    [Fact]
    public void Validate_WithNameStartingWithMicrosoftPrefix_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin("Microsoft.PluginName");

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("reserved prefixes"));
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin(new string('A', 101));

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("exceeds maximum length"));
    }

    [Fact]
    public void Validate_WithNameStartingWithSpecialCharacter_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin("@PluginName");

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("must start with a letter or digit"));
    }

    [Fact]
    public void Validate_WithNameContainingInvalidCharacters_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin("Plugin@Name!");

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("invalid characters"));
    }

    [Theory]
    [InlineData("ValidPlugin")]
    [InlineData("Valid-Plugin")]
    [InlineData("Valid_Plugin")]
    [InlineData("Valid.Plugin")]
    [InlineData("Plugin123")]
    public void Validate_WithValidNames_ReturnsValidResult(string validName)
    {
        var plugin = CreateValidPlugin(validName);

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyVersion_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin();
        plugin.Version = string.Empty;

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("version"));
    }

    [Fact]
    public void Validate_WithInvalidVersionFormat_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin("ValidPlugin", "not-a-version");

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("version"));
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("2.5.3")]
    [InlineData("0.0.1")]
    [InlineData("10.20.30")]
    [InlineData("1.0.0-alpha")]
    [InlineData("1.0.0-beta.1")]
    public void Validate_WithValidVersions_ReturnsValidResult(string validVersion)
    {
        var plugin = CreateValidPlugin("ValidPlugin", validVersion);

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidMetadata_ReturnsValidResult()
    {
        var plugin = CreateValidPlugin();
        plugin.Metadata = new PluginMetadata
        {
            PluginId = plugin.Id,
            Author = "TestAuthor",
            Description = "Test description"
        };

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyMetadataAuthor_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin();
        plugin.Metadata = new PluginMetadata
        {
            PluginId = plugin.Id,
            Author = string.Empty
        };

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("author"));
    }

    [Fact]
    public void Validate_WithValidDependencies_ReturnsValidResult()
    {
        var plugin = CreateValidPlugin();
        var dependency = new PluginDependency
        {
            PluginId = plugin.Id,
            DependencyPluginId = Guid.NewGuid(),
            MinimumVersion = "1.0.0"
        };
        plugin.AddDependency(dependency);

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidDependencyMinimumVersion_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin();
        var dependency = new PluginDependency
        {
            PluginId = plugin.Id,
            DependencyPluginId = Guid.NewGuid(),
            MinimumVersion = "invalid-version"
        };
        plugin.AddDependency(dependency);

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("minimum version"));
    }

    [Fact]
    public void Validate_WithDependencyMaximumVersionLowerThanMinimum_ReturnsInvalidWithError()
    {
        var plugin = CreateValidPlugin();
        var dependency = new PluginDependency
        {
            PluginId = plugin.Id,
            DependencyPluginId = Guid.NewGuid(),
            MinimumVersion = "2.0.0",
            MaximumVersion = "1.0.0"
        };
        plugin.AddDependency(dependency);

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("maximum version"));
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrorMessages()
    {
        var plugin = CreateValidPlugin("", "invalid");

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Validate_IncludesPluginIdAndNameInResult()
    {
        var plugin = CreateValidPlugin("MyPlugin");

        var result = _sut.Validate(plugin);

        result.PluginId.Should().Be(plugin.Id);
        result.PluginName.Should().Be("MyPlugin");
    }

    [Theory]
    [InlineData("Plugin")]
    [InlineData("A")]
    [InlineData("Z")]
    [InlineData("0Plugin")]
    public void Validate_WithNameStartingWithLetterOrDigit_IsValid(string name)
    {
        var plugin = CreateValidPlugin(name);

        var result = _sut.Validate(plugin);

        result.IsValid.Should().BeTrue();
    }
}
