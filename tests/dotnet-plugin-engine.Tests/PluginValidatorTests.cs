#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Utils.Helpers;
using PluginEngine.Utils.Validators;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Contains unit tests for the <see cref="PluginValidator"/> class.
/// Tests various validation scenarios for plugin entities including name validation,
/// version validation, metadata validation, and dependency validation.
/// </summary>
public sealed class PluginValidatorTests
{
	/// <summary>
	/// Gets the mock logger instance used for testing.
	/// </summary>
	private readonly Mock<ILogger<PluginValidator>> _mockLogger;

	/// <summary>
	/// Gets the version helper instance used for version validation.
	/// </summary>
	private readonly VersionHelper _versionHelper;

	/// <summary>
	/// Gets the system under test - the <see cref="PluginValidator"/> instance being tested.
	/// </summary>
	private readonly PluginValidator _sut;

	/// <summary>
	/// Initializes a new instance of the <see cref="PluginValidatorTests"/> class.
	/// Sets up mock logger, version helper, and the plugin validator instance for testing.
	/// </summary>
	public PluginValidatorTests()
	{
		_mockLogger = new Mock<ILogger<PluginValidator>>();
		var mockVerLogger = new Mock<ILogger<VersionHelper>>();
		_versionHelper = new VersionHelper(mockVerLogger.Object);
		_sut = new PluginValidator(_mockLogger.Object, _versionHelper);
	}

	/// <summary>
	/// Creates a valid plugin instance for testing purposes.
	/// </summary>
	/// <param name="name">The plugin name. Defaults to "TestPlugin".</param>
	/// <param name="version">The plugin version. Defaults to "1.0.0".</param>
	/// <returns>A new <see cref="Plugin"/> instance with valid default values.</returns>
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
	/// <summary>
	/// Tests that validation returns valid result when plugin has all valid properties.
	/// </summary>
	public void Validate_WithValidPlugin_ReturnsValidResult()
	{
		var plugin = CreateValidPlugin();

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns invalid result when plugin name is empty.
	/// </summary>
	public void Validate_WithEmptyName_ReturnsInvalidWithError()
	{
		var plugin = CreateValidPlugin();
		plugin.Name = string.Empty;

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.Contains("cannot be empty"));
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns invalid result when plugin name contains only whitespace.
	/// </summary>
	public void Validate_WithWhitespaceName_ReturnsInvalidWithError()
	{
		var plugin = CreateValidPlugin();
		plugin.Name = " ";

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.Contains("cannot be empty"));
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns invalid result when plugin name starts with "System." prefix.
	/// </summary>
	public void Validate_WithNameStartingWithSystemPrefix_ReturnsInvalidWithError()
	{
		var plugin = CreateValidPlugin("System.PluginName");

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.Contains("reserved prefixes"));
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns invalid result when plugin name starts with "Microsoft." prefix.
	/// </summary>
	public void Validate_WithNameStartingWithMicrosoftPrefix_ReturnsInvalidWithError()
	{
		var plugin = CreateValidPlugin("Microsoft.PluginName");

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.Contains("reserved prefixes"));
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns invalid result when plugin name exceeds maximum length (100 characters).
	/// </summary>
	public void Validate_WithNameExceedingMaxLength_ReturnsInvalidWithError()
	{
		var plugin = CreateValidPlugin(new string('A', 101));

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.Contains("exceeds maximum length"));
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns invalid result when plugin name starts with a special character.
	/// </summary>
	public void Validate_WithNameStartingWithSpecialCharacter_ReturnsInvalidWithError()
	{
		var plugin = CreateValidPlugin("@PluginName");

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.Contains("must start with a letter or digit"));
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns invalid result when plugin name contains invalid characters.
	/// </summary>
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
	/// <summary>
	/// Tests that validation returns valid result for various valid plugin names.
	/// </summary>
	/// <param name="validName">A valid plugin name to test.</param>
	public void Validate_WithValidNames_ReturnsValidResult(string validName)
	{
		var plugin = CreateValidPlugin(validName);

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns invalid result when plugin version is empty.
	/// </summary>
	public void Validate_WithEmptyVersion_ReturnsInvalidWithError()
	{
		var plugin = CreateValidPlugin();
		plugin.Version = string.Empty;

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.Contains("version"));
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns invalid result when plugin version has invalid format.
	/// </summary>
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
	/// <summary>
	/// Tests that validation returns valid result for various valid version formats.
	/// </summary>
	/// <param name="validVersion">A valid version string to test.</param>
	public void Validate_WithValidVersions_ReturnsValidResult(string validVersion)
	{
		var plugin = CreateValidPlugin("ValidPlugin", validVersion);

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeTrue();
	}

	[Fact]
	/// <summary>
	/// Tests that validation returns valid result when plugin has valid metadata.
	/// </summary>
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
	/// <summary>
	/// Tests that validation returns invalid result when plugin metadata author is empty.
	/// </summary>
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
	/// <summary>
	/// Tests that validation returns valid result when plugin has valid dependencies.
	/// </summary>
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
	/// <summary>
	/// Tests that validation returns invalid result when dependency has invalid minimum version.
	/// </summary>
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
	/// <summary>
	/// Tests that validation returns invalid result when dependency maximum version is lower than minimum version.
	/// </summary>
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
	/// <summary>
	/// Tests that validation returns all error messages when plugin has multiple validation errors.
	/// </summary>
	public void Validate_WithMultipleErrors_ReturnsAllErrorMessages()
	{
		var plugin = CreateValidPlugin("", "invalid");

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCountGreaterThan(1);
	}

	[Fact]
	/// <summary>
	/// Tests that validation result includes plugin ID and name.
	/// </summary>
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
	/// <summary>
	/// Tests that validation returns valid result when plugin name starts with a letter or digit.
	/// </summary>
	/// <param name="name">A plugin name that starts with a letter or digit.</param>
	public void Validate_WithNameStartingWithLetterOrDigit_IsValid(string name)
	{
		var plugin = CreateValidPlugin(name);

		var result = _sut.Validate(plugin);

		result.IsValid.Should().BeTrue();
	}
}
