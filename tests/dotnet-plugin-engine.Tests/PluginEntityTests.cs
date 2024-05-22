#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using PluginEngine.Domain.Entities;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Contains unit tests for the <see cref="Plugin"/>, <see cref="PluginDependency"/>, and <see cref="PluginCapability"/> entities.
/// Tests various operations including dependency management, capability handling,
/// and validation logic for plugin entities within the plugin engine.
/// </summary>
public sealed class PluginEntityTests
{
	/// <summary>
	/// Creates a valid <see cref="Plugin"/> instance for testing purposes.
	/// </summary>
	/// <param name="name">The name of the plugin. Defaults to "TestPlugin".</param>
	/// <param name="version">The version of the plugin. Defaults to "1.0.0".</param>
	/// <returns>A new <see cref="Plugin"/> instance with valid required properties.</returns>
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

	/// <summary>
	/// Creates a <see cref="PluginDependency"/> instance for testing purposes.
	/// </summary>
	/// <param name="pluginId">The ID of the plugin that has the dependency. Defaults to a new GUID.</param>
	/// <param name="dependencyPluginId">The ID of the plugin being depended on. Defaults to a new GUID.</param>
	/// <param name="minimumVersion">The minimum required version. Defaults to "1.0.0".</param>
	/// <param name="maximumVersion">The maximum allowed version. Defaults to null (no maximum).</param>
	/// <returns>A new <see cref="PluginDependency"/> instance with specified properties.</returns>
	private static PluginDependency CreateDependency(
		Guid? pluginId = null,
		Guid? dependencyPluginId = null,
		string minimumVersion = "1.0.0",
		string? maximumVersion = null)
	{
		return new PluginDependency
		{
			PluginId = pluginId ?? Guid.NewGuid(),
			DependencyPluginId = dependencyPluginId ?? Guid.NewGuid(),
			MinimumVersion = minimumVersion,
			MaximumVersion = maximumVersion ?? string.Empty
		};
	}

	// ── Plugin entity ──────────────────────────────────────────────────

	/// <summary>
	/// Tests that adding a null dependency throws an <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public void AddDependency_WithNullDependency_ThrowsArgumentNullException()
	{
		var plugin = CreateValidPlugin();

		var act = () => plugin.AddDependency(null!);

		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("dependency");
	}

	/// <summary>
	/// Tests that adding a valid dependency appends it to the plugin's dependency list.
	/// </summary>
	[Fact]
	public void AddDependency_WithValidDependency_AppendsToDependencyList()
	{
		var plugin = CreateValidPlugin();
		var dependency = CreateDependency(pluginId: plugin.Id);

		plugin.AddDependency(dependency);

		plugin.Dependencies.Should().ContainSingle()
			.Which.DependencyPluginId.Should().Be(dependency.DependencyPluginId);
	}

	/// <summary>
	/// Tests that adding a null capability throws an <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public void AddCapability_WithNullCapability_ThrowsArgumentNullException()
	{
		var plugin = CreateValidPlugin();

		var act = () => plugin.AddCapability(null!);

		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("capability");
	}

	/// <summary>
	/// Tests that adding a capability with a duplicate name does not add a second entry.
	/// </summary>
	[Fact]
	public void AddCapability_WithDuplicateName_DoesNotAddSecondEntry()
	{
		var plugin = CreateValidPlugin();
		var capabilityA = new PluginCapability { PluginId = plugin.Id, Name = "DataTransform", Version = "1.0.0", InterfaceTypeName = "ITransform" };
		var capabilityB = new PluginCapability { PluginId = plugin.Id, Name = "DataTransform", Version = "2.0.0", InterfaceTypeName = "ITransform" };

		plugin.AddCapability(capabilityA);
		plugin.AddCapability(capabilityB);

		plugin.Capabilities.Should().HaveCount(1);
	}

	/// <summary>
	/// Tests that removing an existing dependency removes it and returns true.
	/// </summary>
	[Fact]
	public void RemoveDependency_WhenDependencyExists_RemovesAndReturnsTrue()
	{
		var plugin = CreateValidPlugin();
		var dependency = CreateDependency(pluginId: plugin.Id);
		plugin.AddDependency(dependency);

		var removed = plugin.RemoveDependency(dependency.Id);

		removed.Should().BeTrue();
		plugin.Dependencies.Should().BeEmpty();
	}

	/// <summary>
	/// Tests that removing a non-existent dependency returns false.
	/// </summary>
	[Fact]
	public void RemoveDependency_WithNonExistentId_ReturnsFalse()
	{
		var plugin = CreateValidPlugin();

		var removed = plugin.RemoveDependency(Guid.NewGuid());

		removed.Should().BeFalse();
	}

	/// <summary>
	/// Tests that a plugin with all required fields is considered valid.
	/// </summary>
	[Fact]
	public void IsValid_WithAllRequiredFields_ReturnsTrue()
	{
		var plugin = CreateValidPlugin();

		plugin.IsValid().Should().BeTrue();
	}

	/// <summary>
	/// Tests that a plugin with an empty name is considered invalid.
	/// </summary>
	[Fact]
	public void IsValid_WithEmptyName_ReturnsFalse()
	{
		var plugin = CreateValidPlugin();
		plugin.Name = string.Empty;

		plugin.IsValid().Should().BeFalse();
	}

	/// <summary>
	/// Tests that a plugin with an empty GUID is considered invalid.
	/// </summary>
	[Fact]
	public void IsValid_WithEmptyGuid_ReturnsFalse()
	{
		var plugin = CreateValidPlugin();
		plugin.Id = Guid.Empty;

		plugin.IsValid().Should().BeFalse();
	}

	/// <summary>
	/// Tests that getting a validation error for a plugin with missing assembly path returns a descriptive message.
	/// </summary>
	[Fact]
	public void GetValidationError_WithMissingAssemblyPath_ReturnsDescriptiveMessage()
	{
		var plugin = CreateValidPlugin();
		plugin.AssemblyPath = string.Empty;

		var error = plugin.GetValidationError();

		error.Should().Contain("Assembly path");
	}

	// ── PluginDependency entity ────────────────────────────────────────

	/// <summary>
	/// Tests that a dependency is satisfied when the version exactly matches the minimum version.
	/// </summary>
	[Fact]
	public void PluginDependency_IsSatisfiedBy_VersionAtMinimum_ReturnsTrue()
	{
		var dep = CreateDependency(minimumVersion: "2.0.0");

		dep.IsSatisfiedBy("2.0.0").Should().BeTrue();
	}

	/// <summary>
	/// Tests that a dependency is satisfied when the version is above the minimum version.
	/// </summary>
	[Fact]
	public void PluginDependency_IsSatisfiedBy_VersionAboveMinimum_ReturnsTrue()
	{
		var dep = CreateDependency(minimumVersion: "1.0.0");

		dep.IsSatisfiedBy("2.5.0").Should().BeTrue();
	}

	/// <summary>
	/// Tests that a dependency is not satisfied when the version is below the minimum version.
	/// </summary>
	[Fact]
	public void PluginDependency_IsSatisfiedBy_VersionBelowMinimum_ReturnsFalse()
	{
		var dep = CreateDependency(minimumVersion: "2.0.0");

		dep.IsSatisfiedBy("1.9.9").Should().BeFalse();
	}

	/// <summary>
	/// Tests that a dependency is not satisfied when the version exceeds the maximum version.
	/// </summary>
	[Fact]
	public void PluginDependency_IsSatisfiedBy_VersionExceedsMaximum_ReturnsFalse()
	{
		var dep = CreateDependency(minimumVersion: "1.0.0", maximumVersion: "1.5.0");

		dep.IsSatisfiedBy("2.0.0").Should().BeFalse();
	}

	/// <summary>
	/// Tests that a dependency returns false when checking an invalid version string.
	/// </summary>
	[Fact]
	public void PluginDependency_IsSatisfiedBy_WithInvalidVersionString_ReturnsFalse()
	{
		var dep = CreateDependency(minimumVersion: "1.0.0");

		dep.IsSatisfiedBy("not-a-version").Should().BeFalse();
	}

	/// <summary>
	/// Tests that getting a version constraint without a maximum version returns a greater-than-or-equal expression.
	/// </summary>
	[Fact]
	public void PluginDependency_GetVersionConstraint_WithoutMaximum_ReturnsGreaterThanOrEqualExpression()
	{
		var dep = CreateDependency(minimumVersion: "3.2.1");

		var constraint = dep.GetVersionConstraint();

		constraint.Should().Be(">= 3.2.1");
	}

	/// <summary>
	/// Tests that getting a version constraint with a maximum version includes both bounds in the expression.
	/// </summary>
	[Fact]
	public void PluginDependency_GetVersionConstraint_WithMaximum_IncludesBothBounds()
	{
		var dep = CreateDependency(minimumVersion: "1.0.0", maximumVersion: "2.0.0");

		var constraint = dep.GetVersionConstraint();

		constraint.Should().Contain(">=").And.Contain("<=").And.Contain("2.0.0");
	}

	// ── PluginCapability entity ────────────────────────────────────────

	/// <summary>
	/// Tests that a capability's <see cref="PluginCapability.HasTag"/> method returns true for a matching tag, ignoring case.
	/// </summary>
	[Fact]
	public void PluginCapability_HasTag_WithMatchingTagIgnoringCase_ReturnsTrue()
	{
		var capability = new PluginCapability
		{
			PluginId = Guid.NewGuid(),
			Name = "Encryption",
			Version = "1.0.0",
			InterfaceTypeName = "IEncryption",
			Tags = "security,encryption,aes"
		};

		capability.HasTag("SECURITY").Should().BeTrue();
		capability.HasTag("aes").Should().BeTrue();
	}

	/// <summary>
	/// Tests that a capability's <see cref="PluginCapability.HasTag"/> method returns false for a non-existent tag.
	/// </summary>
	[Fact]
	public void PluginCapability_HasTag_WithNonExistentTag_ReturnsFalse()
	{
		var capability = new PluginCapability
		{
			PluginId = Guid.NewGuid(),
			Name = "Logging",
			Version = "1.0.0",
			InterfaceTypeName = "ILogger",
			Tags = "logging,audit"
		};

		capability.HasTag("encryption").Should().BeFalse();
	}
}